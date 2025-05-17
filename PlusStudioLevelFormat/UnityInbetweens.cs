using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelFormat
{
    public struct UnityVector3
    {
        public float x;
        public float y;
        public float z;

        public UnityVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public struct UnityColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public UnityColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public UnityColor(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1f;
        }
    }

    public struct UnityQuaternion 
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public UnityQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}
