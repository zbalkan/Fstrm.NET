using System;
using System.Collections.Generic;
using System.Linq;

namespace Fstrm.NET
{
    /*
     * fstrm is a C implementation of the Frame Streams data transport protocol.
     * https://farsightsec.github.io/fstrm/
     * 
     * Frame Streams Control Frame Format - Data frame length equals 00 00 00 00
     * 
     * |------------------------------------|----------------------|
     * | Data frame length                  | 4 bytes              |  
     * |------------------------------------|----------------------|
     * | Control frame length               | 4 bytes              |
     * |------------------------------------|----------------------|
     * | Control frame type                 | 4 bytes              |
     * |------------------------------------|----------------------|
     * | Control frame content type         | 4 bytes (optional)   |
     * |------------------------------------|----------------------|
     * | Control frame content type length  | 4 bytes (optional)   |
     * |------------------------------------|----------------------|
     * | Content type payload               | xx bytes             |     
     * |------------------------------------|----------------------|
     * 
     * Frame Streams Data Frame Format
     * 
     * |------------------------------------|----------------------|
     * | Data frame length                  | 4 bytes              |
     * |------------------------------------|----------------------|
     * | Payload - Protobuf                 | xx bytes             |
     * |------------------------------------|----------------------|
     * 
     * 
     * The C# implementation is based on the Python implementation.
     * https://github.com/dmachard/python-framestream
     * 
     */
    public class FstrmCodec
    {
        private const int FSTRM_CONTROL_FIELD_CONTENT_TYPE = 1;
        private const int CONTROL_FRAME_TYPE_SIZE = 4;
        private const int CONTROL_FRAME_CONTENT_TYPE_SIZE = 4;
        private const int CONTROL_FRAME_CONTENT_TYPE_LENGTH_SIZE = 4;

        private const int FRAME_LENGTH_SECTION_SIZE = 4;
        private List<byte> _buffer;
        private int? _dataframeLength;
        private int? _controlframeLength;

        public FstrmCodec()
        {
            _buffer = new List<byte>();
            _dataframeLength = null;
            _controlframeLength = null;
        }

        public FstrmCodec(byte[] frameData)
        {
            _buffer = new List<byte>(frameData);
            _dataframeLength = null;
            _controlframeLength = null;
        }

        public void Reset()
        {
            _dataframeLength = null;
            _controlframeLength = null;
        }

        public int GetPendingNumberOfBytes()
        {
            if (_dataframeLength.HasValue && _dataframeLength > 0)
            {
                return _dataframeLength.Value - _buffer.Count;
            }

            if (_controlframeLength.HasValue && _controlframeLength > 0)
            {
                return _controlframeLength.Value - _buffer.Count;
            }

            return FRAME_LENGTH_SECTION_SIZE;
        }

        public void Append(byte[] frameData) => _buffer.AddRange(frameData);

        public bool Process()
        {
            if (!_dataframeLength.HasValue)
            {
                // need more data ?
                if (_buffer.Count < FRAME_LENGTH_SECTION_SIZE)
                {
                    return false;
                }

                // enough data, decode frame length
                _dataframeLength = Convert.ToInt32(_buffer.Take(FRAME_LENGTH_SECTION_SIZE).ToArray());
                _buffer = _buffer.Skip<byte>(FRAME_LENGTH_SECTION_SIZE).ToList();
            }

            //  control frame ?
            if (_dataframeLength.Value == 0)
            {
                // need more data ?
                if (_buffer.Count < FRAME_LENGTH_SECTION_SIZE)
                {
                    return false;
                }

                if (!_controlframeLength.HasValue)
                {
                    _controlframeLength = UnpackInt(_buffer, FRAME_LENGTH_SECTION_SIZE);
                    _buffer = _buffer.Skip<byte>(FRAME_LENGTH_SECTION_SIZE).ToList();
                }

                // need more data ?
                if (_buffer.Count < _controlframeLength.Value)
                {
                    return false;
                }
                else
                {
                    // we have received enough data, the frame is complete
                    return true;
                }
            }
            else
            {
                // need more data ?
                return _buffer.Count >= _dataframeLength.Value;
            }
        }

        public bool AppendAndProcess(byte[] frameData)
        {
            Append(frameData);
            return Process();
        }

        public Frame Decode()
        {
            var isControlFrame = false;

            byte[] payload;
            // read from the buffer
            if (_dataframeLength == 0)
            {
                payload = _buffer.Take(_controlframeLength.Value).ToArray();
                _buffer = _buffer.Skip(_controlframeLength.Value).ToList();
                isControlFrame = true;
            }
            else
            {
                payload = _buffer.Take(_dataframeLength.Value).ToArray();
                _buffer = _buffer.Skip(_dataframeLength.Value).ToList();
            }

            // reset to process next frame
            Reset();

            // data frame ?

            if (!isControlFrame)
            {
                return CreateDataFrame(payload);
            }

            //     decode control frame

            var controlframeType = (FrameTypeEnum)UnpackInt(payload, CONTROL_FRAME_TYPE_SIZE);
            payload = payload.Skip(CONTROL_FRAME_TYPE_SIZE).ToArray();

            var content = new List<byte>(payload.Length);

            while (payload.Length > 8)
            {
                var controlframeContentType = UnpackInt(payload, CONTROL_FRAME_CONTENT_TYPE_SIZE);
                var controlframeContentLength = UnpackInt(payload, CONTROL_FRAME_CONTENT_TYPE_LENGTH_SIZE);
                payload = payload.Skip(CONTROL_FRAME_CONTENT_TYPE_SIZE + CONTROL_FRAME_CONTENT_TYPE_LENGTH_SIZE).ToArray();

                if (controlframeContentType != FSTRM_CONTROL_FIELD_CONTENT_TYPE)
                {
                    throw new FstrmException("control ready - control type invalid");
                }

                if (controlframeContentLength > payload.Length)
                {
                    throw new FstrmException("control ready - content length invalid");
                }

                content.AddRange(payload.Take(controlframeContentLength));
                payload = payload.Skip(controlframeContentLength).ToArray();
            }

            return CreateControlFrame(controlframeType, content.ToArray(), payload);
        }

        public byte[] Encode(Frame frame)
        {
            List<byte> payload;

            // data frame ?
            if (frame.FrameType == FrameTypeEnum.FSTRM_DATA_FRAME)
            {
                var payloadLengthBytes = BitConverter.GetBytes(frame.Payload.Length);

                payload = new List<byte>(frame.Payload.Length + sizeof(int));
                payload.AddRange(payloadLengthBytes);
                payload.AddRange(frame.Payload);
                payload.TrimExcess();

                return payload.ToArray();
            }

            // control frame ?

            var length = 4 + (9 * frame.Content.Length);
            var zero = BitConverter.GetBytes((uint)0);
            var lengthBytes = BitConverter.GetBytes(length);
            var frameTypeBytes = BitConverter.GetBytes((int)frame.FrameType);

            payload = new List<byte>(zero.Length + lengthBytes.Length + frameTypeBytes.Length);
            payload.AddRange(zero);
            payload.AddRange(lengthBytes);
            payload.AddRange(frameTypeBytes);

            foreach (var c in frame.Content)
            {
                var contentTypeBytes = BitConverter.GetBytes((uint)FSTRM_CONTROL_FIELD_CONTENT_TYPE);
                payload.AddRange(contentTypeBytes);

                var contentSizeBytes = BitConverter.GetBytes(sizeof(int));
                payload.AddRange(contentSizeBytes);

                payload.Add(c);
            }

            payload.TrimExcess();
            return payload.ToArray();
        }

        public byte[] EncodeReady(byte[] content) => Encode(new Frame(FrameTypeEnum.FSTRM_CONTROL_READY, content));

        public byte[] EncodeAccept(byte[] content) => Encode(new Frame(FrameTypeEnum.FSTRM_CONTROL_ACCEPT, content));

        public byte[] EncodeStart(byte[] content) => Encode(new Frame(FrameTypeEnum.FSTRM_CONTROL_START, content));

        public byte[] EncodeStop(byte[] content) => Encode(new Frame(FrameTypeEnum.FSTRM_CONTROL_STOP, content));

        public bool IsAccept(byte[] data)
        {
            if (!AppendAndProcess(data))
            {
                return false;
            }

            var ctrl = Decode().FrameType;
            if (ctrl != FrameTypeEnum.FSTRM_CONTROL_ACCEPT) { throw new FstrmException($"Unexpected control frame received: {ctrl}"); }

            return true;
        }

        public bool IsReady(byte[] data)
        {
            if (!AppendAndProcess(data))
            {
                return false;
            }

            var ctrl = Decode().FrameType;
            if (ctrl != FrameTypeEnum.FSTRM_CONTROL_READY) { throw new FstrmException($"Unexpected control frame received: {ctrl}"); }

            return true;
        }

        public bool IsStart(byte[] data)
        {
            if (!AppendAndProcess(data))
            {
                return false;
            }

            var ctrl = Decode().FrameType;
            if (ctrl != FrameTypeEnum.FSTRM_CONTROL_START) { throw new FstrmException($"Unexpected control frame received: {ctrl}"); }

            return true;
        }

        public byte[]? IsData(byte[] data)
        {
            if (!AppendAndProcess(data))
            {
                return null;
            }

            var frame = Decode().FrameType;

            if (frame != FrameTypeEnum.FSTRM_CONTROL_ACCEPT) { throw new FstrmException($"Unexpected data frame received: {frame}"); }

            return Decode().Payload;
        }

        private static int UnpackInt(IEnumerable<byte> bytes, int size) => Convert.ToInt32(bytes.Take(size).ToArray());

        private static Frame CreateDataFrame(byte[] frame) => new Frame(FrameTypeEnum.FSTRM_DATA_FRAME, Array.Empty<byte>(), frame);

        private static Frame CreateControlFrame(FrameTypeEnum controlframeType, byte[] content, byte[] payload) => new Frame(controlframeType, content, payload);
    }
}