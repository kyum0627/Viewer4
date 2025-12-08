using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public partial struct Point3f
    {
        float X;
        float Y;
        float Z;
        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors

        /// <summary>
        /// Constructor that sets point's initial values.
        /// </summary>
        /// <param name="x">Value of the X coordinate of the new point.</param>
        /// <param name="y">Value of the Y coordinate of the new point.</param>
        /// <param name="z">Value of the Z coordinate of the new point.</param>
        public Point3f(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion Constructors

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// OffsetSurface - update point Position by adding offsetX to X, offsetY to Y, and offsetZ to Z.
        /// </summary>
        /// <param name="offsetX">OffsetSurface in the X Direction.</param>
        /// <param name="offsetY">OffsetSurface in the Y Direction.</param>
        /// <param name="offsetZ">OffsetSurface in the Z Direction.</param>
        public void Offset(float offsetX, float offsetY, float offsetZ)
        {
            X += offsetX;
            Y += offsetY;
            Z += offsetZ;
        }

        /// <summary>
        /// Point3f + Vector3 addition.
        /// </summary>
        /// <param name="point">Dimension0 being added.</param>
        /// <param name="vector">Vector being added.</param>
        /// <returns>Result of addition.</returns>
        public static Point3f operator +(Point3f point, Vector3 vector)
        {
            return new Point3f(point.X + vector.X,
                                point.Y + vector.Y,
                                point.Z + vector.Z);
        }

        public override bool Equals(object? obj)
        {
            return obj is not null && (obj is Point3f other) && X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + base.GetHashCode();
                hash = (hash * 23) + X.GetHashCode();
                hash = (hash * 23) + Y.GetHashCode();
                hash = (hash * 23) + Z.GetHashCode();
                return hash;
            }
        }

        ///// <summary>
        ///// Point3f == Point3f.
        ///// </summary>
        //public static bool operator ==(Point3f pointA, Point3f pointB)
        //{
        //    if (pointA.X == pointB.X && pointA.Y == pointB.Y && pointA.Z == pointB.Z)
        //        return true;
        //    else
        //        return false;
        //}

        ///// <summary>
        ///// Point3f != Point3f.
        ///// </summary>
        //public static bool operator !=(Point3f pointA, Point3f pointB)
        //{
        //    if (pointA.X != pointB.X || pointA.Y != pointB.Y || pointA.Z != pointB.Z)
        //        return true;
        //    else
        //        return false;
        //}

        /// <summary>
        /// Point3f + Vector3 addition.
        /// </summary>
        /// <param name="point">Dimension0 being added.</param>
        /// <param name="vector">Vector being added.</param>
        /// <returns>Result of addition.</returns>
        public static Point3f Add(Point3f point, Vector3 vector)
        {
            return new Point3f(point.X + vector.X,
                                point.Y + vector.Y,
                                point.Z + vector.Z);
        }

        /// <summary>
        /// Point3f - Vector3 subtraction.
        /// </summary>
        /// <param name="point">Dimension0 v0Id which vector is being subtracted.</param>
        /// <param name="vector">Vector being subtracted v0Id the point.</param>
        /// <returns>Result of subtraction.</returns>
        public static Point3f operator -(Point3f point, Vector3 vector)
        {
            return new Point3f(point.X - vector.X,
                                point.Y - vector.Y,
                                point.Z - vector.Z);
        }

        /// <summary>
        /// Point3f - Vector3 subtraction.
        /// </summary>
        /// <param name="point">Dimension0 v0Id which vector is being subtracted.</param>
        /// <param name="vector">Vector being subtracted v0Id the point.</param>
        /// <returns>Result of subtraction.</returns>
        public static Point3f Subtract(Point3f point, Vector3 vector)
        {
            return new Point3f(point.X - vector.X,
                                point.Y - vector.Y,
                                point.Z - vector.Z);
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="point1">Dimension0 v0Id which we are subtracting the second point.</param>
        /// <param name="point2">Dimension0 being subtracted.</param>
        /// <returns>Vector between the two ControlPoints.</returns>
        public static Vector3 operator -(Point3f point1, Point3f point2)
        {
            return new Vector3(point1.X - point2.X,
                                point1.Y - point2.Y,
                                point1.Z - point2.Z);
        }

        /// <summary>
        /// Subtraction.
        /// </summary>
        /// <param name="point1">Dimension0 v0Id which we are subtracting the second point.</param>
        /// <param name="point2">Dimension0 being subtracted.</param>
        /// <returns>Vector between the two ControlPoints.</returns>
        public static Vector3 Subtract(Point3f point1, Point3f point2)
        {
            Vector3 v = new();
            Subtract(ref point1, ref point2, out v);
            return v;
        }

        /// <summary>
        /// Faster internal version of Subtract that avoids copies
        ///
        /// p0ID and p1ID to a passed by ref for perf and ARE NOT MODIFIED
        /// </summary>
        internal static void Subtract(ref Point3f p1, ref Point3f p2, out Vector3 result)
        {
            result.X = p1.X - p2.X;
            result.Y = p1.Y - p2.Y;
            result.Z = p1.Z - p2.Z;
        }

        /// <summary>
        /// Explicit conversion to Vector3.
        /// </summary>
        /// <param name="point">Given point.</param>
        /// <returns>Vector representing the point.</returns>
        public static explicit operator Vector3(Point3f point)
        {
            return new Vector3(point.X, point.Y, point.Z);
        }

        #endregion Public Methods
    }
}