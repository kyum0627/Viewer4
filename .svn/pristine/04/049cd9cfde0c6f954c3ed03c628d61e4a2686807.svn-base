using System.Runtime.InteropServices;

namespace IGX.Geometry.Common
{
    public static class Utility
    {
        public static int GetOffset<T>(int index)
        {
            return index * Marshal.SizeOf<T>(); // 인덱스를 기반으로 버퍼 오프셋 계산
        }
    }
}
