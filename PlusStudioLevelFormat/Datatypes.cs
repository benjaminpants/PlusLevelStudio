using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusStudioLevelFormat
{
    public struct ByteVector2
    {
        private byte _x;
        private byte _y;
        public byte x => _x;
        public byte y => _y;

        public ByteVector2(byte x, byte y)
        {
            _x = x;
            _y = y;
        }

        public ByteVector2(int x, int y)
        {
            _x = (byte)x;
            _y = (byte)y;
        }

        public static ByteVector2 one => new ByteVector2(1, 1);

        public static ByteVector2 operator +(ByteVector2 a, ByteVector2 b) => new ByteVector2(a.x + b.x, a.y + b.y);

        public static ByteVector2 operator -(ByteVector2 a, ByteVector2 b) => new ByteVector2(a.x - b.x, a.y - b.y);

        public static ByteVector2 operator *(ByteVector2 a, int b) => new ByteVector2(a.x * b, a.y * b);

        public static ByteVector2 operator /(ByteVector2 a, int b) => new ByteVector2(a.x / b, a.y / b);

        public static bool operator ==(ByteVector2 a, ByteVector2 b) => ((a.x == b.x) && (a.y == b.y));
        public static bool operator !=(ByteVector2 a, ByteVector2 b) => !(a == b);

        public override bool Equals(object obj)
        {
            return obj is ByteVector2 vector &&
                   _x == vector._x &&
                   _y == vector._y;
        }

        public override int GetHashCode()
        {
            int hashCode = 979593255;
            hashCode = hashCode * -1521134295 + _x.GetHashCode();
            hashCode = hashCode * -1521134295 + _y.GetHashCode();
            return hashCode;
        }
    }

    public struct MystIntVector2
    {
        private int _x;
        private int _z;
        public int x => _x;
        public int z => _z;

        public MystIntVector2(byte x, byte y)
        {
            _x = x;
            _z = y;
        }

        public MystIntVector2(int x, int y)
        {
            _x = (byte)x;
            _z = (byte)y;
        }

        public static MystIntVector2 one => new MystIntVector2(1, 1);

        public static MystIntVector2 operator +(MystIntVector2 a, MystIntVector2 b) => new MystIntVector2(a.x + b.x, a.z + b.z);

        public static MystIntVector2 operator -(MystIntVector2 a, MystIntVector2 b) => new MystIntVector2(a.x - b.x, a.z - b.z);

        public static MystIntVector2 operator *(MystIntVector2 a, int b) => new MystIntVector2(a.x * b, a.z * b);

        public static MystIntVector2 operator /(MystIntVector2 a, int b) => new MystIntVector2(a.x / b, a.z / b);

        public static bool operator ==(MystIntVector2 a, MystIntVector2 b) => ((a.x == b.x) && (a.z == b.z));
        public static bool operator !=(MystIntVector2 a, MystIntVector2 b) => !(a == b);

        public override bool Equals(object obj)
        {
            return obj is MystIntVector2 vector &&
                   _x == vector._x &&
                   _z == vector._z;
        }

        public override int GetHashCode()
        {
            int hashCode = 929260398;
            hashCode = hashCode * -1521134295 + _x.GetHashCode();
            hashCode = hashCode * -1521134295 + _z.GetHashCode();
            return hashCode;
        }
    }

    // TODO: Write Nybble array writer and Nybble array reader for BinaryReader and BinaryWriter
    public static class NybbleExtensions
    {
        public static byte MergeWith(this Nybble left, Nybble right)
        {
            return Nybble.MergeIntoByte(left, right);
        }
        public static Nybble[] Split(this byte me)
        {
            return Nybble.SplitIntoNybbles(me);
        }
        public static void Write(this BinaryWriter writer, Nybble[] nybbles)
        {
            writer.Write(nybbles.Length);
            for (int i = 0; i < nybbles.Length; i += 2)
            {
                if (i + 1 < nybbles.Length)
                {
                    byte mergedByte = Nybble.MergeIntoByte(nybbles[i], nybbles[i + 1]);
                    writer.Write(mergedByte);
                }
                else
                {
                    // If the array has an odd length, write the last Nybble as a single byte
                    writer.Write((byte)(((byte)nybbles[i]) << 4));
                }
            }
        }

        public static Nybble[] ReadNybbles(this BinaryReader reader)
        {
            int nybbleCount = reader.ReadInt32();
            List<Nybble> nybbles = new List<Nybble>();
            for (int i = 0; i < nybbleCount; i += 2)
            {
                Nybble[] pair = reader.ReadByte().Split();
                if ((i + 1) < nybbleCount)
                {
                    nybbles.AddRange(pair);
                }
                else
                {
                    nybbles.Add(pair[0]);
                }
            }
            return nybbles.ToArray();
        }
    }


    // a nybble represents one half of a byte (4 bits).
    // BB+ uses nybbles to store the state of each wall, one bit determining if a wall is on or off.
    public struct Nybble
    {
        private byte _internal;

        public Nybble(int value)
        {
            _internal = (byte)(value & 0b_0000_1111);
        }

        public static byte MergeIntoByte(Nybble left, Nybble right)
        {
            return (byte)((left << 4) + (right));
        }

        public static Nybble[] SplitIntoNybbles(byte toSplit)
        {
            Nybble[] result = new Nybble[2];
            result[0] = new Nybble((toSplit & 0b_1111_0000) >> 4);
            result[1] = new Nybble(toSplit & 0b_0000_1111);
            return result;
        }

        public override bool Equals(object obj)
        {
            return _internal.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _internal.GetHashCode() + 1;
        }

        public override string ToString()
        {
            return _internal.ToString();
        }

        public static Nybble operator +(Nybble a, Nybble b) => new Nybble(a._internal + b._internal);

        public static Nybble operator -(Nybble a, Nybble b) => new Nybble(a._internal - b._internal);

        public static Nybble operator &(Nybble a, Nybble b) => new Nybble(a._internal & b._internal);

        public static Nybble operator |(Nybble a, Nybble b) => new Nybble(a._internal | b._internal);

        public static implicit operator byte(Nybble a) => a._internal;

        public static implicit operator int(Nybble a) => a._internal;

        public static explicit operator Nybble(byte v) => new Nybble(v);
    }
}
