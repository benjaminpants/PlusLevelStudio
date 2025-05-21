using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusStudioLevelFormat
{
    // is this over-engineered? maybe?
    // but idk i have a feeling someone at some point is going to try to use an absurd amount of textures or posters or whatever
    // and i don't want to have to deal with any of that
    public class StringCompressor
    {
        private List<string> storedStrings = new List<string>();
        bool finalized = false;
        byte byteCount = 0;
        public void AddString(string str)
        {
            if (storedStrings.Contains(str)) return;
            storedStrings.Add(str);
        }
        public void AddStrings(IEnumerable<string> strings)
        {
            foreach (string str in strings)
            {
                AddString(str);
            }
        }

        private void WriteAppropiateType(BinaryWriter writer, int number)
        {
            if (!finalized) throw new Exception("Finalize StringCompressor before calling WriteAppropiateType!");
            switch (byteCount)
            {
                case 1:
                    writer.Write((byte)number);
                    break;
                case 2:
                    writer.Write((ushort)number);
                    break;
                default:
                    writer.Write(number);
                    break;
            }
        }

        private static int ReadAppropiateType(byte byteCount, BinaryReader reader)
        {
            switch (byteCount)
            {
                case 1:
                    return reader.ReadByte();
                case 2:
                    return reader.ReadUInt16();
                default:
                    return reader.ReadInt32();
            }
            throw new NotImplementedException("OOB bytecount!");
        }

        private int ReadAppropiateType(BinaryReader reader)
        {
            return ReadAppropiateType(byteCount,reader);
        }

        public void FinalizeDatabase()
        {
            finalized = true;
            byteCount = 1;
            // either of these secondary cases is almost guranteed to never happen
            if (storedStrings.Count > byte.MaxValue) { byteCount = 2; }
            else if (storedStrings.Count > ushort.MaxValue) { byteCount = 4; }
        }

        public void WriteStringDatabase(BinaryWriter writer)
        {
            if (!finalized) throw new InvalidOperationException("Finalize StringCompressor before attempting to write database!");
            writer.Write(byteCount);
            WriteAppropiateType(writer, storedStrings.Count);
            for (int i = 0; i < storedStrings.Count; i++)
            {
                writer.Write(storedStrings[i]);
            }
        }

        public static StringCompressor ReadStringDatabase(BinaryReader reader)
        {
            StringCompressor compressor = new StringCompressor();
            byte byteCount = reader.ReadByte();
            int count = ReadAppropiateType(byteCount, reader);
            for (int i = 0; i < count; i++)
            {
                compressor.AddString(reader.ReadString());
            }
            compressor.FinalizeDatabase();
            return compressor;

        }

        public void WriteStoredString(BinaryWriter writer, string str)
        {
            if (!finalized) throw new InvalidOperationException("StringCompressor hasn't been finalized!");
            int index = storedStrings.IndexOf(str);
            if (index == -1) throw new Exception("String " + str + "not found in StringCompressor!");
            WriteAppropiateType(writer, index);
        }

        public string ReadStoredString(BinaryReader reader, string str)
        {
            if (!finalized) throw new InvalidOperationException("StringCompressor hasn't been finalized!");
            return storedStrings[ReadAppropiateType(reader)];
        }
    }
}
