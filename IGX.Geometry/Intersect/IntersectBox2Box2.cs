using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using IGX.Geometry.Common;
using IGX.Geometry.ConvexHull;

namespace IGX.Geometry.Intersect
{
    public class IntersectBox2Box2
    {
        public bool intersect;
        public List<Vector2> polygon = new(); // intersect가 true이면 convex polygon 내에서 intersect

        public bool Test(OOBB2 box0, OOBB2 box1)
        {
            Vector2[] a0 = new Vector2[2] { box0.axisX, box0.axisY };
            Vector2[] a1 = new Vector2[2] { box1.axisX, box1.axisY };
            Vector2 e0 = box0.extent;
            Vector2 e1 = box1.extent;

            // difference of box centers, D = C1-C0.
            Vector2 D = box1.center - box0.center;

            float[,] absA0dA1 = new float[2, 2];
            float rSum;

            //int separating;

            // Test box0.axis[0].
            absA0dA1[0, 0] = Math.Abs(Vector2.Dot(a0[0], a1[0]));
            absA0dA1[0, 1] = Math.Abs(Vector2.Dot(a0[0], a1[1]));
            rSum = e0[0] + (e1[0] * absA0dA1[0, 0]) + (e1[1] * absA0dA1[0, 1]);
            if (Math.Abs(Vector2.Dot(a0[0], D)) > rSum)
            {
                intersect = false;
                //separating = 0;
                return intersect;
            }

            // Test axis box0.axis[1].
            absA0dA1[1, 0] = Math.Abs(Vector2.Dot(a0[1], a1[0]));
            absA0dA1[1, 1] = Math.Abs(Vector2.Dot(a0[1], a1[1]));
            rSum = e0[1] + (e1[0] * absA0dA1[1, 0]) + (e1[1] * absA0dA1[1, 1]);
            if (Math.Abs(Vector2.Dot(a0[1], D)) > rSum)
            {
                intersect = false;
                //separating = 1;
                return intersect;
            }

            // Test axis box1.axis[0].
            rSum = e1[0] + (e0[0] * absA0dA1[0, 0]) + (e0[1] * absA0dA1[1, 0]);
            if (Math.Abs(Vector2.Dot(a1[0], D)) > rSum)
            {
                intersect = false;
                //separating = 2;
                return intersect;
            }

            // Test axis box1.axis[1].
            rSum = e1[1] + (e0[0] * absA0dA1[0, 1]) + (e0[1] * absA0dA1[1, 1]);
            if (Math.Abs(Vector2.Dot(a1[1], D)) > rSum)
            {
                intersect = false;
                //separating = 3;
                return intersect;
            }

            intersect = true;
            return intersect;
        }

        public bool Compute(OOBB2 box0, OOBB2 box1)
        {
            intersect = true;

            // box0의 intersection polygon 초기화, CCW order
            Vector2[] vertex = new Vector2[4];
            box0.ComputeVertices(ref vertex);
            polygon.Add(vertex[0]);  // C - e0 * U0 - e1 * U1
            polygon.Add(vertex[1]);  // C + e0 * U0 - e1 * U1
            polygon.Add(vertex[3]);  // C + e0 * U0 + e1 * U1
            polygon.Add(vertex[2]);  // C - e0 * U0 + e1 * U1

            // box1의 edge로 polygon을 clipping
            // line norID points는 box1 내부
            // line origin은 boox1을 CCW로 순회할 때 edge의 first vertex
            box1.ComputeVertices(ref vertex);
            Vector2[] normal = new Vector2[4]
            {
            box1.axisY, -box1.axisX, box1.axisX, -box1.axisY
            };

            for (int i = 0; i < 4; ++i)
            {
                HalfSpace2 hs = new(vertex[i], normal[i]);
                ClipPoly2ByHalfspace2.Result res;
                res = ClipPoly2ByHalfspace2.Compute(hs, polygon);

                if (!res.intersect)
                {
                    // no intersect
                    intersect = false;
                    polygon.Clear();
                    break;
                }
            }
            return intersect;
        }
    }
}