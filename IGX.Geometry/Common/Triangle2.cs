using System;
using OpenTK.Mathematics;

namespace IGX.Geometry.Common
{
    public struct Triangle2
    {
        public Vector2 A, B, C;
        public Triangle2(Vector2 A, Vector2 B, Vector2 C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }
        public float Area()
        {
            return Math.Abs(SignedArea);
        }
        public Vector2 this[int key]
        {
            get
            {
                return key == 0 ? A : key == 1 ? B : C;
            }
            set
            {
                if (key == 0)
                {
                    A = value;
                }
                else if (key == 1)
                {
                    B = value;
                }
                else
                {
                    C = value;
                }
            }
        }
        public float MaxX()
        {
            float x = Math.Max(A.X, B.X);
            return Math.Max(x, C.X);
        }
        public float MinX()
        {
            float x = Math.Min(A.X, B.X);
            return Math.Min(x, C.X);
        }
        public float MaxY()
        {
            float y = Math.Max(A.Y, B.Y);
            return Math.Max(y, C.Y);
        }
        public float MinY()
        {
            float y = Math.Min(A.Y, B.Y);
            return Math.Min(y, C.Y);
        }
        public void ChangeOrientation()
        {
            Vector2 tmp = A;
            A = C;
            C = tmp;
        }

        public bool ContainsPoint(Vector2 point)
        {
            return ContainsPoint(
                    A.X, A.Y,
                    B.X, B.Y,
                    C.X, C.Y,
                    point.X, point.Y
                );
        }

        private bool ContainsPoint(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
        {
            return ((cx - px) * (ay - py)) >= ((ax - px) * (cy - py)) &&
                   ((ax - px) * (by - py)) >= ((bx - px) * (ay - py)) &&
                   ((bx - px) * (cy - py)) >= ((cx - px) * (by - py));
        }

        /// <summary>
        /// http://geomalgorithms.com/a01-_area.html
        /// </summary>
        public float SignedArea
        {
            get
            {
                float area = 0;
                Vector2 a = B - A;
                Vector2 b = C - B;

                area = (a.Y * b.X) - (a.X * b.Y);
                return area * 0.5f;
            }
        }

        //Find the opposite hedge to a vertex
        public Segment2f FindOppositeEdgeToVertex(Vector2 p)
        {
            if (p.Equals(A))
            {
                return new Segment2f(B, C);
            }
            else
            {
                return p.Equals(B) ? new Segment2f(C, A) : new Segment2f(A, B);
            }
        }

        //Check if an hedge is a part of this triangle
        public bool IsEdgePartOfTriangle(Segment2f e)
        {
            if ((e.P0.Equals(A) && e.P0.Equals(B)) || (e.P0.Equals(B) && e.P0.Equals(A)))
            {
                return true;
            }
            if ((e.P0.Equals(B) && e.P0.Equals(C)) || (e.P0.Equals(C) && e.P0.Equals(B)))
            {
                return true;
            }
            return (e.P0.Equals(C) && e.P0.Equals(A)) || (e.P0.Equals(A) && e.P0.Equals(C));
        }
    }
}
