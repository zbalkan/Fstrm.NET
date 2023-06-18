using System.Text;

namespace Fstrm.NET.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_ControlReady()
        {
            var codec = new FstrmCodec();
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
    }
}