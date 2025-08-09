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

        public static MystIntVector2 ReadMystIntVector2(this BinaryReader reader)
        {
            return new MystIntVector2(reader.ReadInt32(), reader.ReadInt32());
        }

        public static UnityVector3 ReadUnityVector3(this BinaryReader reader)
        {
            return new UnityVector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static UnityQuaternion ReadUnityQuaternion(this BinaryReader reader)
        {
            return new UnityQuaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
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

        public static void Write(this BinaryWriter writer, MystIntVector2 vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.z);
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

        public static void Write(this BinaryWriter writer, UnityQuaternion vector)
        {
            writer.Write(vector.x);
            writer.Write(vector.y);
            writer.Write(vector.z);
            writer.Write(vector.w);
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

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) != 0;

            // reverse the array
            Array.Reverse(result);

            return result;
        }

        public static void Write(this BinaryWriter writer, bool[] flags)
        {
            writer.Write(flags.Length);
            for (int i = 0; i < flags.Length; i += 8)
            {
                bool[] bytes = new bool[8];
                int z = 0;
                for (int y = i; y < i + 8; y++)
                {
                    if (y >= flags.Length)
                    {
                        bytes[z] = false;
                    }
                    else
                    {
                        bytes[z] = flags[y];
                    }
                    z++;
                }
                writer.Write(ConvertBoolArrayToByte(bytes));
            }
        }

        public static bool[] ReadBoolArray(this BinaryReader reader)
        {
            int actLength = reader.ReadInt32();
            int length = (int)Math.Ceiling(actLength / 8f);
            bool[] result = new bool[length * 8]; //this rounds us up to read the extra byte
            for (int i = 0; i < length; i++)
            {
                bool[] bools = ConvertByteToBoolArray(reader.ReadByte());
                for (int y = 0; y < bools.Length; y++)
                {
                    result[(i * 8) + y] = bools[y];
                }
            }
            bool[] actualResult = new bool[actLength];
            for (int i = 0; i < actualResult.Length; i++)
            {
                actualResult[i] = result[i];
            }
            return actualResult;
        }
    }
}
