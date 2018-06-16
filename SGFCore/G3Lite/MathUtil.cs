using System;
using System.Collections.Generic;


namespace SGF.G3Lite
{
    public static class MathUtil
    {
        public const double Deg2Rad = (Math.PI / 180.0);
        public const double Rad2Deg = (180.0 / Math.PI);
        public const double TwoPI = 2.0 * Math.PI;
        public const double FourPI = 4.0 * Math.PI;
        public const double HalfPI = 0.5 * Math.PI;
        public const double ZeroTolerance = 1e-08;
        public const double Epsilon = 2.2204460492503131e-016;
        public const double SqrtTwo = 1.41421356237309504880168872420969807;
        public const double SqrtTwoInv = 1.0 / SqrtTwo;
        public const double SqrtThree = 1.73205080756887729352744634150587236;

        public const float Deg2Radf = (float)(Math.PI / 180.0);
        public const float Rad2Degf = (float)(180.0 / Math.PI);
        public const float PIf = (float)(Math.PI);
        public const float TwoPIf = 2.0f * PIf;
        public const float HalfPIf = 0.5f * PIf;
        public const float SqrtTwof = 1.41421356237f;

        public const float ZeroTolerancef = 1e-06f;
        public const float Epsilonf = 1.192092896e-07F;


        public static bool IsFinite(double d)
        {
            return double.IsInfinity(d) == false && double.IsNaN(d) == false;
        }
        public static bool IsFinite(float d)
        {
            return float.IsInfinity(d) == false && float.IsNaN(d) == false;
        }


        public static bool EpsilonEqual(double a, double b, double epsilon = MathUtil.Epsilon)
        {
            return Math.Abs(a - b) <= epsilon;
        }
        public static bool EpsilonEqual(float a, float b, float epsilon = MathUtil.Epsilonf)
        {
            return (float)Math.Abs(a - b) <= epsilon;
        }

        // ugh C# generics so limiting...
        public static T Clamp<T>(T f, T low, T high) where T : IComparable
        {
            if (f.CompareTo(low) < 0) return low;
            else if (f.CompareTo(high) > 0) return high;
            else return f;
        }
        public static float Clamp(float f, float low, float high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }
        public static double Clamp(double f, double low, double high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }
        public static int Clamp(int f, int low, int high)
        {
            return (f < low) ? low : (f > high) ? high : f;
        }

        public static int ModuloClamp(int f, int N)
        {
            while (f < 0)
                f += N;
            return f % N;
        }

        // fMinMaxValue may be signed
        public static float RangeClamp(float fValue, float fMinMaxValue)
        {
            return Clamp(fValue, -Math.Abs(fMinMaxValue), Math.Abs(fMinMaxValue));
        }
        public static double RangeClamp(double fValue, double fMinMaxValue)
        {
            return Clamp(fValue, -Math.Abs(fMinMaxValue), Math.Abs(fMinMaxValue));
        }


        public static float SignedClamp(float f, float fMax)
        {
            return Clamp(Math.Abs(f), 0, fMax) * Math.Sign(f);
        }
        public static double SignedClamp(double f, double fMax)
        {
            return Clamp(Math.Abs(f), 0, fMax) * Math.Sign(f);
        }

        public static float SignedClamp(float f, float fMin, float fMax)
        {
            return Clamp(Math.Abs(f), fMin, fMax) * Math.Sign(f);
        }
        public static double SignedClamp(double f, double fMin, double fMax)
        {
            return Clamp(Math.Abs(f), fMin, fMax) * Math.Sign(f);
        }


        public static bool InRange(float f, float low, float high)
        {
            return f >= low && f <= high;
        }
        public static bool InRange(double f, double low, double high)
        {
            return f >= low && f <= high;
        }
        public static bool InRange(int f, int low, int high)
        {
            return f >= low && f <= high;
        }


        // clamps theta to angle interval [min,max]. should work for any theta,
        // regardless of cycles, however min & max values should be in range
        // [-360,360] and min < max
        public static double ClampAngleDeg(double theta, double min, double max)
        {
            // convert interval to center/extent - [c-e,c+e]
            double c = (min + max) * 0.5;
            double e = max - c;

            // get rid of extra rotations
            theta = theta % 360;

            // shift to origin, then convert theta to +- 180
            theta -= c;
            if (theta < -180)
                theta += 360;
            else if (theta > 180)
                theta -= 360;

            // clamp to extent
            if (theta < -e)
                theta = -e;
            else if (theta > e)
                theta = e;

            // shift back
            return theta + c;
        }



        // clamps theta to angle interval [min,max]. should work for any theta,
        // regardless of cycles, however min & max values should be in range
        // [-2_PI,2_PI] and min < max
        public static double ClampAngleRad(double theta, double min, double max)
        {
            // convert interval to center/extent - [c-e,c+e]
            double c = (min + max) * 0.5;
            double e = max - c;

            // get rid of extra rotations
            theta = theta % TwoPI;

            // shift to origin, then convert theta to +- 180
            theta -= c;
            if (theta < -Math.PI)
                theta += TwoPI;
            else if (theta > Math.PI)
                theta -= TwoPI;

            // clamp to extent
            if (theta < -e)
                theta = -e;
            else if (theta > e)
                theta = e;

            // shift back
            return theta + c;
        }



        // for ((i++) % N)-type loops, but where we might be using (i--)
        public static int WrapSignedIndex(int val, int mod)
        {
            while (val < 0)
                val += mod;
            return val % mod;
        }


        // compute min and max of a,b,c with max 3 comparisons (sometimes 2)
        public static void MinMax(double a, double b, double c, out double min, out double max)
        {
            if (a < b)
            {
                if (a < c)
                {
                    min = a; max = Math.Max(b, c);
                }
                else
                {
                    min = c; max = b;
                }
            }
            else
            {
                if (a > c)
                {
                    max = a; min = Math.Min(b, c);
                }
                else
                {
                    min = b; max = c;
                }
            }
        }


        public static double Min(double a, double b, double c)
        {
            return Math.Min(a, Math.Min(b, c));
        }
        public static float Min(float a, float b, float c)
        {
            return Math.Min(a, Math.Min(b, c));
        }
        public static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }
        public static double Max(double a, double b, double c)
        {
            return Math.Max(a, Math.Max(b, c));
        }
        public static float Max(float a, float b, float c)
        {
            return Math.Max(a, Math.Max(b, c));
        }
        public static int Max(int a, int b, int c)
        {
            return Math.Max(a, Math.Max(b, c));
        }



        // there are fast approximations to this...
        public static double InvSqrt(double f)
        {
            return f / Math.Sqrt(f);
        }


        // normal Atan2 returns in range [-pi,pi], this shifts to [0,2pi]
        public static double Atan2Positive(double y, double x)
        {
            double theta = Math.Atan2(y, x);
            if (theta < 0)
                theta = (2 * Math.PI) + theta;
            return theta;
        }


        public static float PlaneAngleD(Vector3 a, Vector3 b, int nPlaneNormalIdx = 1)
        {
            a[nPlaneNormalIdx] = b[nPlaneNormalIdx] = 0.0f;
            a.Normalize();
            b.Normalize();
            return Vector3.AngleD(a, b);
        }



        public static float PlaneAngleSignedD(Vector3 vFrom, Vector3 vTo, int nPlaneNormalIdx = 1)
        {
            vFrom[nPlaneNormalIdx] = vTo[nPlaneNormalIdx] = 0.0f;
            vFrom.Normalize();
            vTo.Normalize();
            Vector3 c = vFrom.Cross(vTo);
            if (c.sqrMagnitude < MathUtil.ZeroTolerancef)
            {        // vectors are parallel
                return vFrom.Dot(vTo) < 0 ? 180.0f : 0;
            }
            float fSign = Math.Sign(c[nPlaneNormalIdx]);
            float fAngle = fSign * Vector3.AngleD(vFrom, vTo);
            return fAngle;
        }


        public static float PlaneAngleSignedD(Vector3 vFrom, Vector3 vTo, Vector3 planeN)
        {
            vFrom = vFrom - Vector3.Dot(vFrom, planeN) * planeN;
            vTo = vTo - Vector3.Dot(vTo, planeN) * planeN;
            vFrom.Normalize();
            vTo.Normalize();
            Vector3 c = Vector3.Cross(vFrom, vTo);
            if (c.sqrMagnitude < MathUtil.ZeroTolerancef)
            {        // vectors are parallel
                return vFrom.Dot(vTo) < 0 ? 180.0f : 0;
            }
            float fSign = Math.Sign(Vector3.Dot(c, planeN));
            float fAngle = fSign * Vector3.AngleD(vFrom, vTo);
            return fAngle;
        }



        public static float PlaneAngleSignedD(Vector2 vFrom, Vector2 vTo)
        {
            vFrom.Normalize();
            vTo.Normalize();
            float fSign = Math.Sign(vFrom.Cross(vTo));
            float fAngle = fSign * Vector2.AngleD(vFrom, vTo);
            return fAngle;
        }





        public static float Lerp(float a, float b, float t)
        {
            return (1.0f - t) * a + (t) * b;
        }
        public static double Lerp(double a, double b, double t)
        {
            return (1.0 - t) * a + (t) * b;
        }

        public static float SmoothStep(float a, float b, float t)
        {
            t = t * t * (3.0f - 2.0f * t);
            return (1.0f - t) * a + (t) * b;
        }
        public static double SmoothStep(double a, double b, double t)
        {
            t = t * t * (3.0 - 2.0 * t);
            return (1.0 - t) * a + (t) * b;
        }


        public static float SmoothInterp(float a, float b, float t)
        {
            float tt = WyvillRise01(t);
            return (1.0f - tt) * a + (tt) * b;
        }
        public static double SmoothInterp(double a, double b, double t)
        {
            double tt = WyvillRise01(t);
            return (1.0 - tt) * a + (tt) * b;
        }

        //! if yshift is 0, function approaches y=1 at xZero from y=0. 
        //! speed (> 0) controls how fast it gets there
        //! yshift pushes the whole graph upwards (so that it actually crosses y=1 at some point)
        public static float SmoothRise0To1(float fX, float yshift, float xZero, float speed)
        {
            double denom = Math.Pow((fX - (xZero - 1)), speed);
            float fY = (float)((1 + yshift) + (1 / -denom));
            return Clamp(fY, 0, 1);
        }

        public static float WyvillRise01(float fX)
        {
            float d = MathUtil.Clamp(1.0f - fX * fX, 0.0f, 1.0f);
            return 1 - (d * d * d);
        }
        public static double WyvillRise01(double fX)
        {
            double d = MathUtil.Clamp(1.0 - fX * fX, 0.0, 1.0);
            return 1 - (d * d * d);
        }

        public static float WyvillFalloff01(float fX)
        {
            float d = 1 - fX * fX;
            return (d >= 0) ? (d * d * d) : 0;
        }
        public static double WyvillFalloff01(double fX)
        {
            double d = 1 - fX * fX;
            return (d >= 0) ? (d * d * d) : 0;
        }


        public static float WyvillFalloff(float fD, float fInnerRad, float fOuterRad)
        {
            if (fD > fOuterRad)
            {
                return 0;
            }
            else if (fD > fInnerRad)
            {
                fD -= fInnerRad;
                fD /= (fOuterRad - fInnerRad);
                fD = Math.Max(0, Math.Min(1, fD));
                float fVal = (1.0f - fD * fD);
                return fVal * fVal * fVal;
            }
            else
                return 1.0f;
        }
        public static double WyvillFalloff(double fD, double fInnerRad, double fOuterRad)
        {
            if (fD > fOuterRad)
            {
                return 0;
            }
            else if (fD > fInnerRad)
            {
                fD -= fInnerRad;
                fD /= (fOuterRad - fInnerRad);
                fD = Math.Max(0, Math.Min(1, fD));
                double fVal = (1.0f - fD * fD);
                return fVal * fVal * fVal;
            }
            else
                return 1.0;
        }



        // lerps from [0,1] for x in range [deadzone,R]
        public static float LinearRampT(float R, float deadzoneR, float x)
        {
            float sign = Math.Sign(x);
            x = Math.Abs(x);
            if (x < deadzoneR)
                return 0.0f;
            else if (x > R)
                return sign * 1.0f;
            else
            {
                x = Math.Min(x, R);
                float d = (x - deadzoneR) / (R - deadzoneR);
                return sign * d;
            }
        }


        public static bool SolveQuadratic(double a, double b, double c, out double minT, out double maxT)
        {
            minT = maxT = 0;
            if (a == 0 && b == 0)   // function is constant...
                return true;

            double discrim = b * b - 4.0 * a * c;
            if (discrim < 0)
                return false;    // no solution

            // a bit odd but numerically better (says NRIC)
            double t = -0.5 * (b + Math.Sign(b) * Math.Sqrt(discrim));
            minT = t / a;
            maxT = c / t;
            if (minT > maxT)
            {
                a = minT; minT = maxT; maxT = a;   // swap
            }

            return true;
        }




        static readonly int[] powers_of_10 = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000 };
        public static int PowerOf10(int n)
        {
            return powers_of_10[n];
        }


        /// <summary>
        /// Iterate from 0 to (nMax-1) using prime-modulo, so we see every index once, but not in-order
        /// </summary>
        public static IEnumerable<int> ModuloIteration(int nMaxExclusive, int nPrime = 31337)
        {
            int i = 0;
            bool done = false;
            while (done == false)
            {
                yield return i;
                i = (i + nPrime) % nMaxExclusive;
                done = (i == 0);
            }
        }





    }
}