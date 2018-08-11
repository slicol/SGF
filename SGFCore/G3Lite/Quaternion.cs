using System;

namespace SGF.G3Lite
{
    // mostly ported from WildMagic5 Wm5Quaternion, from geometrictools.com
    public struct Quaternion : IComparable<Quaternion>, IEquatable<Quaternion>
    {
        // note: in Wm5 version, this is a 4-element array stored in order (w,x,y,z).
        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public Quaternion(float[] v2) { x = v2[0]; y = v2[1]; z = v2[2]; w = v2[3]; }
        public Quaternion(Quaternion q2) { x = q2.x; y = q2.y; z = q2.z; w = q2.w; }

        public Quaternion(Vector3 axis, float AngleDeg)
        {
            x = y = z = 0; w = 1;
            SetAxisAngleD(axis, AngleDeg);
        }
        public Quaternion(Vector3 vFrom, Vector3 vTo)
        {
            x = y = z = 0; w = 1;
            SetFromTo(vFrom, vTo);
        }
        public Quaternion(Quaternion p, Quaternion q, float t)
        {
            x = y = z = 0; w = 1;
            SetToSlerp(p, q, t);
        }
        public Quaternion(Matrix3 mat)
        {
            x = y = z = 0; w = 1;
            SetFromRotationMatrix(mat);
        }

        static public readonly Quaternion Zero = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        static public readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

        public float this[int key]
        {
            get { if (key == 0) return x; else if (key == 1) return y; else if (key == 2) return z; else return w; }
            set { if (key == 0) x = value; else if (key == 1) y = value; else if (key == 2) z = value; else w = value; }

        }


        public float LengthSquared
        {
            get { return x * x + y * y + z * z + w * w; }
        }
        public float Length
        {
            get { return (float)Math.Sqrt(x * x + y * y + z * z + w * w); }
        }

        public float Normalize(float epsilon = 0)
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
        public Quaternion Normalized
        {
            get { Quaternion q = new Quaternion(this); q.Normalize(); return q; }
        }

        public float Dot(Quaternion q2)
        {
            return x * q2.x + y * q2.y + z * q2.z + w * q2.w;
        }




        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            float w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
            float x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
            float y = a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z;
            float z = a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x;
            return new Quaternion(x, y, z, w);
        }



        public static Quaternion operator -(Quaternion q1, Quaternion q2)
        {
            return new Quaternion(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);
        }

        public static Vector3 operator *(Quaternion q, Vector3 v)
        {
            //return q.ToRotationMatrix() * v;
            // inline-expansion of above:
            float twoX = 2 * q.x; float twoY = 2 * q.y; float twoZ = 2 * q.z;
            float twoWX = twoX * q.w; float twoWY = twoY * q.w; float twoWZ = twoZ * q.w;
            float twoXX = twoX * q.x; float twoXY = twoY * q.x; float twoXZ = twoZ * q.x;
            float twoYY = twoY * q.y; float twoYZ = twoZ * q.y; float twoZZ = twoZ * q.z;
            return new Vector3(
                v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
                v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
                v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY))); ;
        }





        /// <summary> Inverse() * v </summary>
        public Vector3 InverseMultiply(ref Vector3 v)
        {
            float norm = LengthSquared;
            if (norm > 0)
            {
                float invNorm = 1.0f / norm;
                float qx = -x * invNorm, qy = -y * invNorm, qz = -z * invNorm, qw = w * invNorm;
                float twoX = 2 * qx; float twoY = 2 * qy; float twoZ = 2 * qz;
                float twoWX = twoX * qw; float twoWY = twoY * qw; float twoWZ = twoZ * qw;
                float twoXX = twoX * qx; float twoXY = twoY * qx; float twoXZ = twoZ * qx;
                float twoYY = twoY * qy; float twoYZ = twoZ * qy; float twoZZ = twoZ * qz;
                return new Vector3(
                    v.x * (1 - (twoYY + twoZZ)) + v.y * (twoXY - twoWZ) + v.z * (twoXZ + twoWY),
                    v.x * (twoXY + twoWZ) + v.y * (1 - (twoXX + twoZZ)) + v.z * (twoYZ - twoWX),
                    v.x * (twoXZ - twoWY) + v.y * (twoYZ + twoWX) + v.z * (1 - (twoXX + twoYY)));
            }
            else
                return Vector3.zero;
        }





        // these multiply quaternion by (1,0,0), (0,1,0), (0,0,1), respectively.
        // faster than full multiply, because of all the zeros
        public Vector3 AxisX
        {
            get
            {
                float twoY = 2 * y; float twoZ = 2 * z;
                float twoWY = twoY * w; float twoWZ = twoZ * w;
                float twoXY = twoY * x; float twoXZ = twoZ * x;
                float twoYY = twoY * y; float twoZZ = twoZ * z;
                return new Vector3(1 - (twoYY + twoZZ), twoXY + twoWZ, twoXZ - twoWY);
            }
        }
        public Vector3 AxisY
        {
            get
            {
                float twoX = 2 * x; float twoY = 2 * y; float twoZ = 2 * z;
                float twoWX = twoX * w; float twoWZ = twoZ * w; float twoXX = twoX * x;
                float twoXY = twoY * x; float twoYZ = twoZ * y; float twoZZ = twoZ * z;
                return new Vector3(twoXY - twoWZ, 1 - (twoXX + twoZZ), twoYZ + twoWX);
            }
        }
        public Vector3 AxisZ
        {
            get
            {
                float twoX = 2 * x; float twoY = 2 * y; float twoZ = 2 * z;
                float twoWX = twoX * w; float twoWY = twoY * w; float twoXX = twoX * x;
                float twoXZ = twoZ * x; float twoYY = twoY * y; float twoYZ = twoZ * y;
                return new Vector3(twoXZ + twoWY, twoYZ - twoWX, 1 - (twoXX + twoYY));
            }
        }



        public Quaternion Inverse()
        {
            float norm = LengthSquared;
            if (norm > 0)
            {
                float invNorm = 1.0f / norm;
                return new Quaternion(
                    -x * invNorm, -y * invNorm, -z * invNorm, w * invNorm);
            }
            else
                return Quaternion.Zero;
        }
        public static Quaternion Inverse(Quaternion q)
        {
            return q.Inverse();
        }



        public Matrix3 ToRotationMatrix()
        {
            float twoX = 2 * x; float twoY = 2 * y; float twoZ = 2 * z;
            float twoWX = twoX * w; float twoWY = twoY * w; float twoWZ = twoZ * w;
            float twoXX = twoX * x; float twoXY = twoY * x; float twoXZ = twoZ * x;
            float twoYY = twoY * y; float twoYZ = twoZ * y; float twoZZ = twoZ * z;
            Matrix3 m = Matrix3.Zero;
            m[0, 0] = 1 - (twoYY + twoZZ); m[0, 1] = twoXY - twoWZ; m[0, 2] = twoXZ + twoWY;
            m[1, 0] = twoXY + twoWZ; m[1, 1] = 1 - (twoXX + twoZZ); m[1, 2] = twoYZ - twoWX;
            m[2, 0] = twoXZ - twoWY; m[2, 1] = twoYZ + twoWX; m[2, 2] = 1 - (twoXX + twoYY);
            return m;
        }



        public void SetAxisAngleD(Vector3 axis, float AngleDeg)
        {
            double angle_rad = MathUtil.Deg2Rad * AngleDeg;
            double halfAngle = 0.5 * angle_rad;
            double sn = Math.Sin(halfAngle);
            w = (float)Math.Cos(halfAngle);
            x = (float)(sn * axis.x);
            y = (float)(sn * axis.y);
            z = (float)(sn * axis.z);
        }

        public static Quaternion AngleAxis(float angleDeg, Vector3 axis)
        {
            return new Quaternion(axis, angleDeg);
        }

        public static Quaternion AngleAxisRad(float angleRad, Vector3 axis)
        {
            return new Quaternion(axis, angleRad * MathUtil.Rad2Degf);
        }

        // this function can take non-normalized vectors vFrom and vTo (normalizes internally)
        public void SetFromTo(Vector3 vFrom, Vector3 vTo)
        {
            // [TODO] this page seems to have optimized version:
            //    http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

            // [RMS] not ideal to explicitly normalize here, but if we don't,
            //   output quaternion is not normalized and this causes problems,
            //   eg like drift if we do repeated SetFromTo()
            Vector3 from = vFrom.normalized, to = vTo.normalized;
            Vector3 bisector = (from + to).normalized;
            w = from.Dot(bisector);
            if (w != 0)
            {
                Vector3 cross = from.Cross(bisector);
                x = cross.x;
                y = cross.y;
                z = cross.z;
            }
            else
            {
                float invLength;
                if (Math.Abs(from.x) >= Math.Abs(from.y))
                {
                    // V1.x or V1.z is the largest magnitude component.
                    invLength = (float)(1.0 / Math.Sqrt(from.x * from.x + from.z * from.z));
                    x = -from.z * invLength;
                    y = 0;
                    z = +from.x * invLength;
                }
                else
                {
                    // V1.y or V1.z is the largest magnitude component.
                    invLength = (float)(1.0 / Math.Sqrt(from.y * from.y + from.z * from.z));
                    x = 0;
                    y = +from.z * invLength;
                    z = -from.y * invLength;
                }
            }
            Normalize();   // aaahhh just to be safe...
        }
        public static Quaternion FromTo(Vector3 vFrom, Vector3 vTo)
        {
            return new Quaternion(vFrom, vTo);
        }
        public static Quaternion FromToConstrained(Vector3 vFrom, Vector3 vTo, Vector3 vAround)
        {
            float fAngle = MathUtil.PlaneAngleSignedD(vFrom, vTo, vAround);
            return Quaternion.AngleAxis(fAngle, vAround);
        }


        public void SetToSlerp(Quaternion p, Quaternion q, float t)
        {
            float cs = p.Dot(q);
            float angle = (float)Math.Acos(cs);
            if (Math.Abs(angle) >= MathUtil.ZeroTolerance)
            {
                float sn = (float)Math.Sin(angle);
                float invSn = 1 / sn;
                float tAngle = t * angle;
                float coeff0 = (float)Math.Sin(angle - tAngle) * invSn;
                float coeff1 = (float)Math.Sin(tAngle) * invSn;
                x = coeff0 * p.x + coeff1 * q.x;
                y = coeff0 * p.y + coeff1 * q.y;
                z = coeff0 * p.z + coeff1 * q.z;
                w = coeff0 * p.w + coeff1 * q.w;
            }
            else
            {
                x = p.x;
                y = p.y;
                z = p.z;
                w = p.w;
            }
        }
        public static Quaternion Slerp(Quaternion p, Quaternion q, float t)
        {
            return new Quaternion(p, q, t);
        }



        public void SetFromRotationMatrix(Matrix3 rot)
        {
            // Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
            // article "Quaternion Calculus and Fast Animation".
            Index3 next = new Index3(1, 2, 0);

            float trace = rot[0, 0] + rot[1, 1] + rot[2, 2];
            float root;

            if (trace > 0)
            {
                // |w| > 1/2, may as well choose w > 1/2
                root = (float)Math.Sqrt(trace + (float)1);  // 2w
                w = ((float)0.5) * root;
                root = ((float)0.5) / root;  // 1/(4w)
                x = (rot[2, 1] - rot[1, 2]) * root;
                y = (rot[0, 2] - rot[2, 0]) * root;
                z = (rot[1, 0] - rot[0, 1]) * root;
            }
            else
            {
                // |w| <= 1/2
                int i = 0;
                if (rot[1, 1] > rot[0, 0])
                {
                    i = 1;
                }
                if (rot[2, 2] > rot[i, i])
                {
                    i = 2;
                }
                int j = next[i];
                int k = next[j];

                root = (float)Math.Sqrt(rot[i, i] - rot[j, j] - rot[k, k] + (float)1);

                Vector3 quat = new Vector3(x, y, z);
                quat[i] = ((float)0.5) * root;
                root = ((float)0.5) / root;
                w = (rot[k, j] - rot[j, k]) * root;
                quat[j] = (rot[j, i] + rot[i, j]) * root;
                quat[k] = (rot[k, i] + rot[i, k]) * root;
                x = quat.x; y = quat.y; z = quat.z;
            }

            Normalize();   // we prefer normalized quaternions...
        }


        public static Quaternion Euler(Vector3 euler)
        {
            float pitch = euler.x;
            float roll = euler.y;
            float yaw = euler.z;

            Quaternion q;
            // Abbreviations for the various angular functions
            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);
            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);

            q.w = (float)(cy * cr * cp + sy * sr * sp);
            q.x = (float)(cy * sr * cp - sy * cr * sp);
            q.y = (float)(cy * cr * sp + sy * sr * cp);
            q.z = (float)(sy * cr * cp - cy * sr * sp);
            return q;
        }



        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return (a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w);
        }
        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return (a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w);
        }
        public override bool Equals(object obj)
        {
            return this == (Quaternion)obj;
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
        public int CompareTo(Quaternion other)
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
        public bool Equals(Quaternion other)
        {
            return (x == other.x && y == other.y && z == other.z && w == other.w);
        }





        public bool EpsilonEqual(Quaternion q2, float epsilon)
        {
            return (float)Math.Abs(x - q2.x) <= epsilon &&
                   (float)Math.Abs(y - q2.y) <= epsilon &&
                   (float)Math.Abs(z - q2.z) <= epsilon &&
                   (float)Math.Abs(w - q2.w) <= epsilon;
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
        public static implicit operator Quaternion(Quaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
        public static implicit operator Quaternion(Quaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }
#endif

    }
}

