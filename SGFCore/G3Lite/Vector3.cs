using System;

namespace SGF.G3Lite
{
    public struct Vector3 : IComparable<Vector3>, IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float f) { x = y = z = f; }
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Vector3(float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; }
        public Vector3(Vector3 copy) { x = copy.x; y = copy.y; z = copy.z; }

        public Vector3(double f) { x = y = z = (float)f; }
        public Vector3(double x, double y, double z) { this.x = (float)x; this.y = (float)y; this.z = (float)z; }
        public Vector3(double[] v2) { x = (float)v2[0]; y = (float)v2[1]; z = (float)v2[2]; }

        static public readonly Vector3 zero = new Vector3(0.0f, 0.0f, 0.0f);
        static public readonly Vector3 one = new Vector3(1.0f, 1.0f, 1.0f);
        static public readonly Vector3 forward = new Vector3(0.0f, 0.0f, 1.0f);
        static public readonly Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);
        static public readonly Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        static public readonly Vector3 down = new Vector3(0.0f, -1.0f, 0.0f);
        static public readonly Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        static public readonly Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);

        static public readonly Vector3 OneNormalized = new Vector3(1.0f, 1.0f, 1.0f).normalized;
        static public readonly Vector3 Invalid = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        static public readonly Vector3 AxisX = new Vector3(1.0f, 0.0f, 0.0f);
        static public readonly Vector3 AxisY = new Vector3(0.0f, 1.0f, 0.0f);
        static public readonly Vector3 AxisZ = new Vector3(0.0f, 0.0f, 1.0f);
        static public readonly Vector3 MaxValue = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        static public readonly Vector3 MinValue = new Vector3(float.MinValue, float.MinValue, float.MinValue);



        public float this[int key]
        {
            get { return (key == 0) ? x : (key == 1) ? y : z; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else z = value; }
        }

        public Vector2 xy
        {
            get { return new Vector2(x, y); }
            set { x = value.x; y = value.y; }
        }
        public Vector2 xz
        {
            get { return new Vector2(x, z); }
            set { x = value.x; z = value.y; }
        }
        public Vector2 yz
        {
            get { return new Vector2(y, z); }
            set { y = value.x; z = value.y; }
        }

        public float sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }
        public float magnitude
        {
            get { return (float)Math.Sqrt(sqrMagnitude); }
        }

        public float LengthL1
        {
            get { return Math.Abs(x) + Math.Abs(y) + Math.Abs(z); }
        }

        public float Max
        {
            get { return Math.Max(x, Math.Max(y, z)); }
        }
        public float Min
        {
            get { return Math.Min(x, Math.Min(y, z)); }
        }
        public float MaxAbs
        {
            get { return Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z))); }
        }
        public float MinAbs
        {
            get { return Math.Min(Math.Abs(x), Math.Min(Math.Abs(y), Math.Abs(z))); }
        }


        public float Normalize(float epsilon = MathUtil.Epsilonf)
        {
            float length = magnitude;
            if (length > epsilon)
            {
                float invLength = 1.0f / length;
                x *= invLength;
                y *= invLength;
                z *= invLength;
            }
            else
            {
                length = 0;
                x = y = z = 0;
            }
            return length;
        }
        public Vector3 normalized
        {
            get
            {
                float length = magnitude;
                if (length > MathUtil.Epsilonf)
                {
                    float invLength = 1 / length;
                    return new Vector3(x * invLength, y * invLength, z * invLength);
                }
                else
                    return Vector3.zero;
            }
        }

        public bool IsNormalized
        {
            get { return Math.Abs((x * x + y * y + z * z) - 1) < MathUtil.ZeroTolerancef; }
        }

        public bool IsFinite
        {
            get { float f = x + y + z; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
        }


        public void Round(int nDecimals)
        {
            x = (float)Math.Round(x, nDecimals);
            y = (float)Math.Round(y, nDecimals);
            z = (float)Math.Round(z, nDecimals);
        }


        public float Dot(Vector3 v2)
        {
            return x * v2[0] + y * v2[1] + z * v2[2];
        }
        public static float Dot(Vector3 v1, Vector3 v2)
        {
            return v1.Dot(v2);
        }


        public Vector3 Cross(Vector3 v2)
        {
            return new Vector3(
                y * v2.z - z * v2.y,
                z * v2.x - x * v2.z,
                x * v2.y - y * v2.x);
        }
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return v1.Cross(v2);
        }

        public Vector3 UnitCross(Vector3 v2)
        {
            Vector3 n = new Vector3(
                y * v2.z - z * v2.y,
                z * v2.x - x * v2.z,
                x * v2.y - y * v2.x);
            n.Normalize();
            return n;
        }

        public float AngleD(Vector3 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot) * MathUtil.Rad2Deg);
        }
        public static float AngleD(Vector3 v1, Vector3 v2)
        {
            return v1.AngleD(v2);
        }
        public float AngleR(Vector3 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)(Math.Acos(fDot));
        }
        public static float AngleR(Vector3 v1, Vector3 v2)
        {
            return v1.AngleR(v2);
        }


        public float DistanceSquared(Vector3 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z;
            return dx * dx + dy * dy + dz * dz;
        }
        public float Distance(Vector3 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }



        public void Set(Vector3 o)
        {
            x = o[0]; y = o[1]; z = o[2];
        }
        public void Set(float fX, float fY, float fZ)
        {
            x = fX; y = fY; z = fZ;
        }
        public void Add(Vector3 o)
        {
            x += o[0]; y += o[1]; z += o[2];
        }
        public void Subtract(Vector3 o)
        {
            x -= o[0]; y -= o[1]; z -= o[2];
        }



        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static Vector3 operator *(float f, Vector3 v)
        {
            return new Vector3(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3 operator *(Vector3 v, float f)
        {
            return new Vector3(f * v.x, f * v.y, f * v.z);
        }
        public static Vector3 operator /(Vector3 v, float f)
        {
            return new Vector3(v.x / f, v.y / f, v.z / f);
        }
        public static Vector3 operator /(float f, Vector3 v)
        {
            return new Vector3(f / v.x, f / v.y, f / v.z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Vector3 operator /(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }


        public static Vector3 operator +(Vector3 v0, Vector3 v1)
        {
            return new Vector3(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
        }
        public static Vector3 operator +(Vector3 v0, float f)
        {
            return new Vector3(v0.x + f, v0.y + f, v0.z + f);
        }

        public static Vector3 operator -(Vector3 v0, Vector3 v1)
        {
            return new Vector3(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
        }
        public static Vector3 operator -(Vector3 v0, float f)
        {
            return new Vector3(v0.x - f, v0.y - f, v0.z - f);
        }


        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z);
        }
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector3)obj;
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;
                // Suitable nullity checks etc, of course :)
                hash = (hash * 16777619) ^ x.GetHashCode();
                hash = (hash * 16777619) ^ y.GetHashCode();
                hash = (hash * 16777619) ^ z.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector3 other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            else if (z != other.z)
                return z < other.z ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector3 other)
        {
            return (x == other.x && y == other.y && z == other.z);
        }


        public bool EpsilonEqual(Vector3 v2, float epsilon)
        {
            return (float)Math.Abs(x - v2.x) <= epsilon &&
                   (float)Math.Abs(y - v2.y) <= epsilon &&
                   (float)Math.Abs(z - v2.z) <= epsilon;
        }


        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            float s = 1 - t;
            return new Vector3(s * a.x + t * b.x, s * a.y + t * b.y, s * a.z + t * b.z);
        }



        public override string ToString()
        {
            return string.Format("{0:F8} {1:F8} {2:F8}", x, y, z);
        }
        public string ToString(string fmt)
        {
            return string.Format("{0} {1} {2}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt));
        }


        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0);
        }


#if G3_USING_UNITY
        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        public static implicit operator Vector3(Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        public static implicit operator Color(Vector3 v)
        {
            return new Color(v.x, v.y, v.z, 1.0f);
        }
        public static implicit operator Vector3(Color c)
        {
            return new Vector3(c.r, c.g, c.b);
        }
#endif

    }
}