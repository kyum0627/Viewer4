using OpenTK.Mathematics;
using IGX.Geometry.Common;

namespace IGX.Geometry.Intersect
{
    public class IntersectLine3Plane3
    {
        Line3f line;
        Plane3f plane;

        public IntersectionResult3 result;

        public IntersectLine3Plane3(Line3f l, Plane3f p)
        {
            line = l;
            plane = p;
        }

        public bool Test()
        {
            result = new IntersectionResult3();

            float DdN = Vector3.Dot(line.direction, plane.normal);
            if (DdN != 0)
            {
                // 교차
                result.status = IntersectionResult.INTERSECT;
                result.type = IntersectionType.POINT;
                result.quantity = 1;
            }
            else
            {
                if (Vector3.Dot(line.origin - plane.V0, plane.normal) == 0)
                {
                    result.status = IntersectionResult.INTERSECT;
                    result.type = IntersectionType.LINE;
                    result.quantity = int.MaxValue;
                }

                // 평행
                result.status = IntersectionResult.NOTINTERSECT;
                result.type = IntersectionType.NONE;
                result.quantity = 0;
            }
            return result.status == IntersectionResult.INTERSECT;
        }

        public bool Compute()
        {
            result = new IntersectionResult3();
            DoQuery(line.origin, line.direction, plane, ref result);
            if (result.status == IntersectionResult.INTERSECT)
            {
                result.points[0] = line.origin + (result.parameter1[0] * line.direction);
            }
            return result.status == IntersectionResult.INTERSECT;
        }

        private void DoQuery(Vector3 lineOrigin, Vector3 lineDirection, Plane3f plane, ref IntersectionResult3 result)
        {
            float DdN = Vector3.Dot(lineDirection, plane.normal);
            float distance = Vector3.Dot(lineOrigin, plane.normal) - plane.distance;
            if (DdN != 0)
            {
                // 교차
                result.status = IntersectionResult.INTERSECT;
                result.quantity = 1;
                result.parameter1[0] = -distance / DdN;
            }
            else
            {
                // parallel. 면위에 있는지 검사
                if (distance == 0)
                {
                    // 면위에 있음. t = 0;
                    result.status = IntersectionResult.INTERSECT;
                    result.quantity = int.MaxValue;
                    result.parameter1[0] = 0f;
                }
                else
                {
                    // 평행하나 면 위에 있지 않음
                    result.status = IntersectionResult.NOTINTERSECT;
                    result.quantity = int.MaxValue;
                }
            }
        }
    }
}
