//using igxGlobalSettingsManager;
namespace IGX.Structure
{
    /// <summary>
    /// 용접선 산출에 사용되는 종류별 default 허용 오차 , 너무 작아도 너무 커도 안됨
    /// </summary>
    public static class DefaultWeldTolerance
    {
        public const float Butt = 5.0f; // 모서리끼리 용접
        public const float Overlap = 3.0f; // 한 부재의 face가 다른 부재의 face위에 용접 //20240403 hschoi gap이 2mm이상인 경우가 발생하여 tolerance를 3으로 높임
        public const float Fillet = 10.0f; // 한 부재의 edge가 다른 부재의 face위에 용접
        public const float Penetrate = 10.0f; // web floor와 longitudinals 처럼 두 부재가 상호 관통 관계인 경우
        public const float IgnoredWeldLength = 5.0f;
    }
}
