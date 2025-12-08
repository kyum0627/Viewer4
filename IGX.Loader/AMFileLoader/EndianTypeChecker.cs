namespace IGX.Loader.AMFileLoader
{
    public static class EndianTypeChecker
    {
        // 파일 경로를 받아서 엔디언 타입을 확인하는 메서드
        public static string CheckEndianType(byte[] bytes)
        {
            // 엔디언 타입 확인
            bool isLittleEndian = IsLittleEndian(bytes);
            return isLittleEndian ? "Little Endian" : "Big Endian";
        }

        // 엔디언이 리틀 엔디언인지 확인하는 메서드
        private static bool IsLittleEndian(byte[] bytes)
        {
            // 예시로 첫 4바이트를 검사하여 리틀 엔디언인지 확인
            // 예를 들어, 0x01 0x00 0x00 0x00 가 리틀 엔디언이면, 이는 1을 나타냄
            if (bytes.Length < 4)
            {
                throw new ArgumentException("Invalid byte array length.");
            }

            // 리틀 엔디언 검사 (0x01 0x00 0x00 0x00 순서로)
            return bytes[0] == 0x01;
        }
    }
}
