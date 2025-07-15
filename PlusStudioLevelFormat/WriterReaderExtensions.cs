using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusStudioLevelFormat
{
    public static class WriterReaderExtensions
    {
        public static ByteVector2 ReadByteVector2(this BinaryReader reader)
        {
            return new ByteVector2(reader.ReadByte(), reader.ReadByte());
        }

        public static UnityVector3 ReadUnityVector3(this BinaryReader reader)
        {
            return new UnityVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static UnityVector2 ReadUnityVector2(this BinaryReader reader)
        {
            return new UnityVector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static void Write(this BinaryWriter writer, ByteVector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }

        public static void Write(this BinaryWriter writer, UnityColor color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static void Write(this BinaryWriter writer, UnityVector3 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
        }

        public static void Write(this BinaryWriter writer, UnityVector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }

        public static UnityColor ReadUnityColor(this BinaryReader reader)
        {
            return new UnityColor(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
