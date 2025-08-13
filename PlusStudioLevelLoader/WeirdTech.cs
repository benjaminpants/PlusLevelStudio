using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PlusStudioLevelLoader
{
    public static class WeirdTechExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct FloatAndUIntUnion
        {
            [FieldOffset(0)]
            public int intBits;
            [FieldOffset(0)]
            public float floatBits;
        }

        internal static FloatAndUIntUnion union = new FloatAndUIntUnion();

        /// <summary>
        /// Converts the underlying bits that represent the float into an int.
        /// Note that the "int" returned by this method is nonsense until converted bakc into a float.
        /// For example, passing in 10f will not get you an int with the value of "10."
        /// This method is used to store floats in fields that typically only support ints.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ConvertToIntNoRecast(this float value)
        {
            union.floatBits = value;
            return union.intBits;
        }

        /// <summary>
        /// Converts the underlying bits that represent the int into a float.
        /// Note that the "float" returned by this method is nonsense until converted back into a int.
        /// Use this to convert "ints" made by ConvertToIntNoRecast back into floats.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float ConvertToFloatNoRecast(this int value)
        {
            union.intBits = value;
            return union.floatBits;
        }
    }
}
