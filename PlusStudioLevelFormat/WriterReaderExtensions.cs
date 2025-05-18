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
        public static void Write(this BinaryWriter writer, ByteVector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
        }
    }
}
