using System;

namespace SGF.G3Lite
{
    public struct Vector2 : IComparable<Vector2>, IEquatable<Vector2>
    {
        public float x;
        public float y;

        public Vector2(float f) { x = y = f; }
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public Vector2(float[] v2) { x = v2[0]; y = v2[1]; }
        public Vector2(double f) { x = y = (float)f; }
        public Vector2(double x, double y) { this.x = (float)x; this.y = (float)y; }
        public Vector2(double[] v2) { x = (float)v2[0]; y = (float)v2[1]; }
        public Vector2(Vector2 copy) { x = copy[0]; y = copy[1]; }
 

        static public readonly Vector2 Zero = new Vector2(0.0f, 0.0f);
        static public readonly Vector2 One = new Vector2(1.0f, 1.0f);
        static public readonly Vector2 AxisX = new Vector2(1.0f, 0.0f);
        static public readonly Vector2 AxisY = new Vector2(0.0f, 1.0f);
        static public readonly Vector2 MaxValue = new Vector2(float.MaxValue, float.MaxValue);
        static public readonly Vector2 MinValue = new Vector2(float.MinValue, float.MinValue);

        public float this[int key]
        {
            get { return (key == 0) ? x : y; }
            set { if (key == 0) x = value; else y = value; }
        }


        public float LengthSquared
        {
            get { return x * x + y * y; }
        }
        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        public float Normalize(float epsilon = MathUtil.Epsilonf)
        {
            float length = Length;
            if (length > epsilon)
            {
                float invLength = 1.0f / length;
                x *= invLength;
                y *= invLength;
            }
            else
            {
                length = 0;
                x = y = 0;
            }
            return length;
        }
        public Vector2 Normalized
        {
            get
            {
                float length = Length;
                if (length > MathUtil.Epsilonf)
                {
                    float invLength = 1 / length;
                    return new Vector2(x * invLength, y * invLength);
                }
                else
                    return Vector2.Zero;
            }
        }

        public bool IsNormalized
        {
            get { return Math.Abs((x * x + y * y) - 1) < MathUtil.ZeroTolerancef; }
        }

        public bool IsFinite
        {
            get { float f = x + y; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals)
        {
            x = (float)Math.Round(x, nDecimals);
            y = (float)Math.Round(y, nDecimals);
        }

        public float Dot(Vector2 v2)
        {
            return x * v2.x + y * v2.y;
        }


        public float Cross(Vector2 v2)
        {
            return x * v2.y - y * v2.x;
        }


        public Vector2 Perp
        {
            get { return new Vector2(y, -x); }
        }
        public Vector2 UnitPerp
        {
            get { return new Vector2(y, -x).Normalized; }
        }
        public float DotPerp(Vector2 v2)
        {
            return x * v2.y - y * v2.x;
        }


        public float AngleD(Vector2 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot) * MathUtil.Rad2Deg);
        }
        public static float AngleD(Vector2 v1, Vector2 v2)
        {
            return v1.AngleD(v2);
        }
        public float AngleR(Vector2 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot));
        }
        public static float AngleR(Vector2 v1, Vector2 v2)
        {
            return v1.AngleR(v2);
        }



        public float DistanceSquared(Vector2 v2)
        {
            float dx = v2.x - x, dy = v2.y - y;
            return dx * dx + dy * dy;
        }
        public float Distance(Vector2 v2)
        {
            float dx = v2.x - x, dy = v2.y - y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }


        public void Set(Vector2 o)
        {
            x = o.x; y = o.y;
        }
        public void Set(float fX, float fY)
        {
            x = fX; y = fY;
        }
        public void Add(Vector2 o)
        {
            x += o.x; y += o.y;
        }
        public void Subtract(Vector2 o)
        {
            x -= o.x; y -= o.y;
        }


        public static Vector2 operator -(Vector2 v)
        {
            return new Vector2(-v.x, -v.y);
        }

        public static Vector2 operator +(Vector2 a, Vector2 o)
        {
            return new Vector2(a.x + o.x, a.y + o.y);
        }
        public static Vector2 operator +(Vector2 a, float f)
        {
            return new Vector2(a.x + f, a.y + f);
        }

        public static Vector2 operator -(Vector2 a, Vector2 o)
        {
            return new Vector2(a.x - o.x, a.y - o.y);
        }
        public static Vector2 operator -(Vector2 a, float f)
        {
            return new Vector2(a.x - f, a.y - f);
        }

        public static Vector2 operator *(Vector2 a, float f)
        {
            return new Vector2(a.x * f, a.y * f);
        }
        public static Vector2 operator *(float f, Vector2 a)
        {
            return new Vector2(a.x * f, a.y * f);
        }
        public static Vector2 operator /(Vector2 v, float f)
        {
            return new Vector2(v.x / f, v.y / f);
        }
        public static Vector2 operator /(float f, Vector2 v)
        {
            return new Vector2(f / v.x, f / v.y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x * b.x, a.y * b.y);
        }
        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x / b.x, a.y / b.y);
        }


        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return (a.x != b.x || a.y != b.y);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector2)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector2 other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector2 other)
        {
            return (x == other.x && y == other.y);
        }


        public bool EpsilonEqual(Vector2 v2, float epsilon)
        {
            return (float)Math.Abs(x - v2.x) <= epsilon &&
                   (float)Math.Abs(y - v2.y) <= epsilon;
        }


        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            float s = 1 - t;
            return new Vector2(s * a.x + t * b.x, s * a.y + t * b.y);
        }
        public static Vector2 Lerp(ref Vector2 a, ref Vector2 b, float t)
        {
            float s = 1 - t;
            return new Vector2(s * a.x + t * b.x, s * a.y + t * b.y);
        }


        public override string ToString()
        {
            return string.Format("{0:F8} {1:F8}", x, y);
        }


        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

#if G3_USING_UNITY
        public static implicit operator Vector2(UnityEngine.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }
        public static implicit operator Vector2(Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }
#endif

    }
}