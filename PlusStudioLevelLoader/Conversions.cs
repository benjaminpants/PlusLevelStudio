using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusStudioLevelLoader
{
    public static class Extensions
    {
        public static Color ToStandard(this UnityColor color)
        {
            return new Color(color.r, color.g, color.b, color.a);
        }

        public static UnityColor ToData(this Color color)
        {
            return new UnityColor(color.r, color.g, color.b, color.a);
        }

        public static IntVector2 ToInt(this ByteVector2 me)
        {
            return new IntVector2(me.x, me.y);
        }

        public static ByteVector2 ToByte(this IntVector2 me)
        {
            return new ByteVector2(me.x, me.z);
        }

        public static IntVector2 ToStandard(this MystIntVector2 me)
        {
            return new IntVector2(me.x, me.z);
        }

        public static MystIntVector2 ToData(this IntVector2 me)
        {
            return new MystIntVector2(me.x, me.z);
        }

        public static Vector3 ToUnity(this UnityVector3 me)
        {
            return new Vector3(me.x, me.y, me.z);
        }

        public static Vector2 ToUnity(this UnityVector2 me)
        {
            return new Vector2(me.x, me.y);
        }

        public static UnityVector2 ToData(this Vector2 me)
        {
            return new UnityVector2(me.x, me.y);
        }

        public static Quaternion ToUnity(this UnityQuaternion me)
        {
            return new Quaternion(me.x, me.y, me.z, me.w);
        }

        public static UnityVector3 ToData(this Vector3 me)
        {
            return new UnityVector3(me.x, me.y, me.z);
        }

        public static UnityQuaternion ToData(this Quaternion me)
        {
            return new UnityQuaternion(me.x, me.y, me.z, me.w);
        }
    }
}
