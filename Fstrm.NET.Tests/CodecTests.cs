using System.Text;

namespace Fstrm.NET.Tests
{
    public class Tests
    {
        private FstrmCodec codec;
        [SetUp]
        public void Setup() => codec = new FstrmCodec();

        [Test]
        public void Test_ControlReady()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_CONTROL_READY, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_CONTROL_READY));
        }

        [Test]
        public void Test_ControlAccept()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_CONTROL_ACCEPT, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_CONTROL_ACCEPT));
        }

        [Test]
        public void Test_ControlFinish()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_CONTROL_FINISH, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_CONTROL_FINISH));
        }

        [Test]
        public void Test_ControlStart()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_CONTROL_START, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_CONTROL_START));
        }

        [Test]
        public void Test_ControlStop()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_CONTROL_STOP, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_CONTROL_STOP));
        }

        [Test]
        public void Test_DataFrame()
        {
            codec.Reset();
            var content = Encoding.ASCII.GetBytes("protobuf:dnstap.Dnstap");
            var frame = new Frame(FrameTypeEnum.FSTRM_DATA_FRAME, content);
            var binaryData = codec.Encode(frame);
            codec.Append(binaryData);

            Frame actualFrame = default;
            if (codec.Process())
            {
                actualFrame = codec.Decode();
            }

            Assert.That(actualFrame.FrameType, Is.EqualTo(FrameTypeEnum.FSTRM_DATA_FRAME));
        }
    }
}