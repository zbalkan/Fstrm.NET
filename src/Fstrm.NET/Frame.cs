using System;
using System.Linq;

namespace Fstrm.NET
{
    public struct Frame : IEquatable<Frame>
    {
        public FrameTypeEnum FrameType;
        public byte[] Content;
        public byte[] Payload;

        public Frame(FrameTypeEnum frameType, byte[] content, byte[] payload)
        {
            FrameType = frameType;
            Content = content;
            Payload = payload;
        }

        public Frame(FrameTypeEnum frameType, byte[] content) : this(frameType, content, Array.Empty<byte>())
        {
        }

        public bool Equals(Frame other) => Payload.SequenceEqual(other.Payload);

        public override bool Equals(object? obj) => obj is Frame frame && Equals(frame);

        public static bool operator ==(Frame left, Frame right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Frame left, Frame right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => Payload.GetHashCode();
    }
}