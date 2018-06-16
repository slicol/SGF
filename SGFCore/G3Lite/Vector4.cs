using System;

namespace SGF.G3Lite
{
    public struct Vector4 : IComparable<Vector4>, IEquatable<Vector4>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float f) { x = y = z = w = f; }
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public Vector4(float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
        public Vector4(Vector4 copy) { x = copy.x; y = copy.y; z = copy.z; w = copy.w; }

        static public readonly Vector4 zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        static public readonly Vector4 one = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        public float this[int key]
        {
            get { return (key < 2) ? ((key == 0) ? x : y) : ((key == 2) ? z : w); }
            set
            {
                if (key < 2) { if (key == 0) x = value; else y = value; }
                else { if (key == 2) z = value; else w = value; }
            }
        }

        public float LengthSquared
        {
            get { return x * x + y * y + z * z + w * w; }
        }
        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        public float LengthL1
        {
            get { return Math.Abs(x) + Math.Abs(y) + Math.Abs(z) + Math.Abs(w); }
        }


        public float Normalize(float epsilon = MathUtil.Epsilonf)
        {
            float length = Length;
            if (length > epsilon)
            {
                float invLength = 1.0f / length;
                x *= invLength;
                y *= invLength;
                z *= invLength;
                w *= invLength;
            }
            else
            {
                length = 0;
                x = y = z = w = 0;
            }
            return length;
        }
        public Vector4 Normalized
        {
            get
            {
                float length = Length;
                if (length > MathUtil.Epsilon)
                {
                    float invLength = 1.0f / length;
                    return new Vector4(x * invLength, y * invLength, z * invLength, w * invLength);
                }
                else
                    return Vector4.zero;
            }
        }

        public bool IsNormalized
        {
            get { return Math.Abs((x * x + y * y + z * z + w * w) - 1) < MathUtil.ZeroTolerance; }
        }


        public bool IsFinite
        {
            get { float f = x + y + z + w; return float.IsNaN(f) == false && float.IsInfinity(f) == false; }
        }

        public void Round(int nDecimals)
        {
            x = (float)Math.Round(x, nDecimals);
            y = (float)Math.Round(y, nDecimals);
            z = (float)Math.Round(z, nDecimals);
            w = (float)Math.Round(w, nDecimals);
        }


        public float Dot(Vector4 v2)
        {
            return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
        }
        public float Dot(ref Vector4 v2)
        {
            return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
        }

        public static float Dot(Vector4 v1, Vector4 v2)
        {
            return v1.Dot(v2);
        }


        public float AngleD(Vector4 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)Math.Acos(fDot) * MathUtil.Rad2Degf;
        }
        public static float AngleD(Vector4 v1, Vector4 v2)
        {
            return v1.AngleD(v2);
        }
        public float AngleR(Vector4 v2)
        {
            float fDot = MathUtil.Clamp(Dot(v2), -1, 1);
            return (float)Math.Acos(fDot);
        }
        public static float AngleR(Vector4 v1, Vector4 v2)
        {
            return v1.AngleR(v2);
        }

        public float DistanceSquared(Vector4 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
            return dx * dx + dy * dy + dz * dz + dw * dw;
        }
        public float DistanceSquared(ref Vector4 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
            return dx * dx + dy * dy + dz * dz + dw * dw;
        }

        public float Distance(Vector4 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
        }
        public float Distance(ref Vector4 v2)
        {
            float dx = v2.x - x, dy = v2.y - y, dz = v2.z - z, dw = v2.w - w;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
        }


        public static Vector4 operator -(Vector4 v)
        {
            return new Vector4(-v.x, -v.y, -v.z, -v.w);
        }

        public static Vector4 operator *(float f, Vector4 v)
        {
            return new Vector4(f * v.x, f * v.y, f * v.z, f * v.w);
        }
        public static Vector4 operator *(Vector4 v, float f)
        {
            return new Vector4(f * v.x, f * v.y, f * v.z, f * v.w);
        }
        public static Vector4 operator /(Vector4 v, float f)
        {
            return new Vector4(v.x / f, v.y / f, v.z / f, v.w / f);
        }
        public static Vector4 operator /(float f, Vector4 v)
        {
            return new Vector4(f / v.x, f / v.y, f / v.z, f / v.w);
        }

        public static Vector4 operator *(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }
        public static Vector4 operator /(Vector4 a, Vector4 b)
        {
            return new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
        }


        public static Vector4 operator +(Vector4 v0, Vector4 v1)
        {
            return new Vector4(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
        }
        public static Vector4 operator +(Vector4 v0, float f)
        {
            return new Vector4(v0.x + f, v0.y + f, v0.z + f, v0.w + f);
        }

        public static Vector4 operator -(Vector4 v0, Vector4 v1)
        {
            return new Vector4(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
        }
        public static Vector4 operator -(Vector4 v0, float f)
        {
            return new Vector4(v0.x - f, v0.y - f, v0.z - f, v0.w - f);
        }



        public static bool operator ==(Vector4 a, Vector4 b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
        }
        public static bool operator !=(Vector4 a, Vector4 b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);
        }
        public override bool Equals(object obj)
        {
            return this == (Vector4)obj;
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
                hash = (hash * 16777619) ^ w.GetHashCode();
                return hash;
            }
        }
        public int CompareTo(Vector4 other)
        {
            if (x != other.x)
                return x < other.x ? -1 : 1;
            else if (y != other.y)
                return y < other.y ? -1 : 1;
            else if (z != other.z)
                return z < other.z ? -1 : 1;
            else if (w != other.w)
                return w < other.w ? -1 : 1;
            return 0;
        }
        public bool Equals(Vector4 other)
        {
            return (x == other.x && y == other.y && z == other.z && w == other.w);
        }


        public bool EpsilonEqual(Vector4 v2, float epsilon)
        {
            return Math.Abs(x - v2.x) <= epsilon &&
                   Math.Abs(y - v2.y) <= epsilon &&
                   Math.Abs(z - v2.z) <= epsilon &&
                   Math.Abs(w - v2.w) <= epsilon;
        }

        public static implicit operator Vector4(Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 0.0f);
        }

        public static implicit operator Vector3(Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4(Vector2 v)
        {
            return new Vector4(v.x, v.y, 0.0f, 0.0f);
        }

        public static implicit operator Vector2(Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }

        public override string ToString()
        {
            return string.Format("{0:F8} {1:F8} {2:F8} {3:F8}", x, y, z, w);
        }
        public string ToString(string fmt)
        {
            return string.Format("{0} {1} {2} {3}", x.ToString(fmt), y.ToString(fmt), z.ToString(fmt), w.ToString(fmt));
        }




#if G3_USING_UNITY
        public static implicit operator Vector4(Vector4 v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }
        public static implicit operator Vector4(Vector4 v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }
        public static implicit operator Color(Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }
        public static implicit operator Vector4(Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }
#endif

    }
}