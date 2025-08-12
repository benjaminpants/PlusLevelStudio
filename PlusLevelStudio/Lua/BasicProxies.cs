using MoonSharp.Interpreter;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class ColorProxy
    {
        public int r { get; private set; }
        public int g { get; private set; }
        public int b { get; private set; }

        public ColorProxy()
        {
            r = 0;
            g = 0;
            b = 0;
        }

        public ColorProxy(int r, int g, int b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public ColorProxy(Color color)
        {
            r = Mathf.RoundToInt(color.r * 255f);
            g = Mathf.RoundToInt(color.g * 255f);
            b = Mathf.RoundToInt(color.b * 255f);
        }

        [MoonSharpHidden]
        public Color ToColor()
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }

    [MoonSharpUserData]
    public class Vector3Proxy
    {
        public override string ToString()
        {
            return string.Format("({0},{1},{2})", x, y, z);
        }

        public float x { get; private set; }
        public float y { get; private set; }
        public float z { get; private set; }


        public Vector3Proxy()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3Proxy(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Proxy(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public static Vector3Proxy operator +(Vector3Proxy a, Vector3Proxy b) => new Vector3Proxy(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3Proxy operator -(Vector3Proxy a, Vector3Proxy b) => new Vector3Proxy(a.x - b.x, a.y - b.y, a.z - b.z);
        public static bool operator ==(Vector3Proxy a, Vector3Proxy b) => ((a.x == b.x) && (a.y == b.y) && (a.z == b.z));
        public static bool operator !=(Vector3Proxy a, Vector3Proxy b) => ((a.x != b.x) || (a.y != b.y) || (a.z != b.z));

        [MoonSharpHidden]
        public Vector3 ToVector()
        {
            return new Vector3(x, y, z);
        }

        public float DistanceFrom(Vector3Proxy other)
        {
            return Vector3.Distance(ToVector(),other.ToVector());
        }

    }

    [MoonSharpUserData]
    public class IntVector2Proxy
    {
        public override string ToString()
        {
            return string.Format("({0},{1})",x,z);
        }

        public int x { get; private set; }
        public int z { get; private set; }


        public IntVector2Proxy()
        {
            x = 0;
            z = 0;
        }

        public IntVector2Proxy(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public IntVector2Proxy(IntVector2 vector)
        {
            x = vector.x;
            z = vector.z;
        }

        [MoonSharpHidden]
        public IntVector2 ToVector()
        {
            return new IntVector2(x, z);
        }

        public static IntVector2Proxy operator +(IntVector2Proxy a, IntVector2Proxy b) => new IntVector2Proxy(a.x + b.x, a.z + b.z);
        public static IntVector2Proxy operator -(IntVector2Proxy a, IntVector2Proxy b) => new IntVector2Proxy(a.x - b.x, a.z - b.z);
        public static bool operator ==(IntVector2Proxy a, IntVector2Proxy b) => ((a.x == b.x) && (a.z == b.z));
        public static bool operator !=(IntVector2Proxy a, IntVector2Proxy b) => ((a.x != b.x) || (a.z != b.z));
    }
}
