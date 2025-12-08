using System;
using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectTriangle2Halfspace3
    {
        public struct Result
        {
            public bool intersect;
            public int numPoints;
            public Vector3[] point;
        };

        public Result Test(HalfSpace3 halfspace, Triangle3f triangle)
        {
            Result result = new();
            float[] s = new float[3];
            for (int i = 0; i < 3; ++i)
            {
                s[i] = Vector3.Dot(halfspace.normal, triangle[i]) - halfspace.constant;
            }
            result.intersect = Math.Max(Math.Max(s[0], s[1]), s[2]) >= 0;
            return result;
        }

        public Result Compute(HalfSpace3 halfspace, Triangle3f triangle)
        {
            //   nID pID z  intersection
            //   ---------------------------------
            //   0 3 0  triangle (original)
            //   0 2 1  triangle (original)
            //   0 1 2  triangle (original)
            //   0 0 3  triangle (original)
            //   1 2 0  quad (2 halfedgeSet clipped)
            //   1 1 1  triangle (1 hedge clipped)
            //   1 0 2  hedge
            //   2 1 0  triangle (2 halfedgeSet clipped)
            //   2 0 1  vertex
            //   3 0 0  none
            float[] s = new float[3];
            int numPositive = 0, numNegative = 0, numZero = 0;

            Result result = new();
            result.point = new Vector3[4];

            for (int i = 0; i < 3; ++i)
            {
                s[i] = Vector3.Dot(halfspace.normal, triangle[i]) - halfspace.constant;
                if (s[i] > 0)
                {
                    ++numPositive;
                }
                else if (s[i] < 0)
                {
                    ++numNegative;
                }
                else
                {
                    ++numZero;
                }
            }

            if (numNegative == 0)
            {
                // The triangle is in the halfspace.
                result.intersect = true;
                result.numPoints = 3;
                result.point[0] = triangle[0];
                result.point[1] = triangle[1];
                result.point[2] = triangle[2];
            }
            else if (numNegative == 1)
            {
                result.intersect = true;
                if (numPositive == 2)
                {
                    // The portion of the triangle in the halfspace is a
                    // quadrilateral.
                    result.numPoints = 4;
                    for (int i0 = 0; i0 < 3; ++i0)
                    {
                        if (s[i0] < 0)
                        {
                            int i1 = (i0 + 1) % 3, i2 = (i0 + 2) % 3;
                            result.point[0] = triangle[i1];
                            result.point[1] = triangle[i2];
                            float t2 = s[i2] / (s[i2] - s[i0]);
                            float t0 = s[i0] / (s[i0] - s[i1]);
                            result.point[2] = triangle[i2] + (t2 * (triangle[i0] - triangle[i2]));
                            result.point[3] = triangle[i0] + (t0 * (triangle[i1] - triangle[i0]));
                            break;
                        }
                    }
                }
                else if (numPositive == 1)
                {
                    // The portion of the triangle in the halfspace is a
                    // triangle with one vertex on the plane.
                    result.numPoints = 3;
                    for (int i0 = 0; i0 < 3; ++i0)
                    {
                        if (s[i0] == 0)
                        {
                            int i1 = (i0 + 1) % 3, i2 = (i0 + 2) % 3;
                            result.point[0] = triangle[i0];
                            float t1 = s[i1] / (s[i1] - s[i2]);
                            Vector3 p = triangle[i1] + (t1 *
                                (triangle[i2] - triangle[i1]));
                            if (s[i1] > 0)
                            {
                                result.point[1] = triangle[i1];
                                result.point[2] = p;
                            }
                            else
                            {
                                result.point[1] = p;
                                result.point[2] = triangle[i2];
                            }
                            break;
                        }
                    }
                }
                else
                {
                    // Only an hedge of the triangle is in the halfspace.
                    result.numPoints = 0;
                    for (int i = 0; i < 3; ++i)
                    {
                        if (s[i] == 0)
                        {
                            result.point[result.numPoints++] = triangle[i];
                        }
                    }
                }
            }
            else if (numNegative == 2)
            {
                result.intersect = true;
                if (numPositive == 1)
                {
                    // The portion of the triangle in the halfspace is a
                    // triangle.
                    result.numPoints = 3;
                    for (int i0 = 0; i0 < 3; ++i0)
                    {
                        if (s[i0] > 0)
                        {
                            int i1 = (i0 + 1) % 3, i2 = (i0 + 2) % 3;
                            result.point[0] = triangle[i0];
                            float t0 = s[i0] / (s[i0] - s[i1]);
                            float t2 = s[i2] / (s[i2] - s[i0]);
                            result.point[1] = triangle[i0] + (t0 *
                                (triangle[i1] - triangle[i0]));
                            result.point[2] = triangle[i2] + (t2 *
                                (triangle[i0] - triangle[i2]));
                            break;
                        }
                    }
                }
                else
                {
                    // Only a vertex of the triangle is in the halfspace.
                    result.numPoints = 1;
                    for (int i = 0; i < 3; ++i)
                    {
                        if (s[i] == 0)
                        {
                            result.point[0] = triangle[i];
                            break;
                        }
                    }
                }
            }
            else  // numNegative == 3
            {
                // The triangle is outside the halfspace. (numNegative == 3)
                result.intersect = false;
                result.numPoints = 0;
            }

            return result;
        }
    }
}
