using System;

namespace IGX.Geometry.Common
{
    /// <summary>
    /// 기하학적 도형의 특정 속성을 계산하는 유틸리티 클래스.
    /// Arc, 원뿔, 원기둥 등의 기본 도형을 부드럽게(?) 표현하기 위해 사용
    /// See: https://en.wikipedia.org/wiki/Sagitta_(geometry)
    /// </summary>
    public static class Sagitta
    {
        private const int MinSamples = 3;
        private const int MaxSamples = 100;

        /// <summary>
        /// 원기둥이나 기타 원형 도형을 세밀하게 표현하기 위해 필요한 세그먼트 수와 최대 크기를 계산
        /// </summary>
        /// <param name="arc">원호의 길이</param>
        /// <param name="radius">도형의 반지름</param>
        /// <param name="scale">도형의 크기에 대한 스케일링 비율</param>
        /// <param name="segments">도형을 표현하는 데 필요한 세그먼트 수</param>
        /// <returns></returns>
        public static float BasedError(double arc, float radius, float scale, int segments)
        {
            double LengthOfSagitta = scale * radius * (1.0f - Math.Cos(arc / segments)); //.Length of sagitta
            return (float)LengthOfSagitta;
        }

        /// <summary>
        /// 허용 오차 내에서 도형을 표현하는 데 필요한 세그먼트 수를 계산
        /// </summary>
        /// <param name="arc">원호 길이(각도, radian)</param>
        /// <param name="radius">반지름</param>
        /// <param name="scale">스케일</param>
        /// <param name="tolerance">세밀한 정도. 작을수록 세밀</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static uint SegmentCount(double arc, float radius, float scale, float tolerance)
        {
            float maximumSagitta = tolerance;
            double samples = arc / Math.Acos(Math.Max(-1.0f, 1.0f - (maximumSagitta / (scale * radius))));
            if (double.IsNaN(samples))
            {
                throw new Exception(
                    $"Segment Sample 수 계산 불가: ({nameof(scale)}: {scale}, {nameof(arc)}: {arc}, {nameof(radius)}: {radius}, {nameof(tolerance)}: {tolerance} )"
                );
            }
            // 최종 샘플 수 계산
            uint sampleCount = (uint)Math.Min(MaxSamples, Math.Max(MinSamples, Math.Ceiling(samples)));

            if (sampleCount > 32)
            {
                sampleCount = 32;
            }

            return ((sampleCount / 4) + 1) * 4;
        }

        /// <summary>
        /// 반지름을 기반으로 도형의 허용 오차를 계산
        /// </summary>
        /// <param name="radius">도형의 반지름</param>
        /// <returns>허용 오차</returns>
        public static float CalculateSagittaTolerance(float radius)
        {
            if (radius == 0) // 반경이 지정되지 않은 경우 default value
            {
                return 1;
            }

            float value = (radius * 0.04f) + 0.02f; // 오차 계산
            return value;
        }
    }
}
