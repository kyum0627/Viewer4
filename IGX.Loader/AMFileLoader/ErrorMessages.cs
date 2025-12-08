namespace IGX.Geometry.Common
{
    public static class ErrorMessages
    {
        // 파일 I/O 관련 에러
        public const string FileNotFound = "File not found: {0}";
        public const string InvalidFileHeader = "Unable to read first 4 bytes of file: {0}";
        public const string LoadError = "Error loading file: {0}";
        public const string FileAccessDenied = "Access to file denied: {0}";
        public const string FileReadError = "Error reading file: {0}. Details: {1}";
        public const string EndOfFileReached = "Unexpected end of file reached: {0}";

        // 스트림 처리 관련 에러
        public const string StreamNull = "Stream is null or invalid.";
        public const string StreamReadError = "Error reading from stream at position {0}/{1}. Details: {2}";
        public const string InsufficientBytes = "Insufficient bytes read. Expected: {0}, Actual: {1} at position {2}";

        // 데이터 파싱 관련 에러
        public const string InvalidChunkType = "Invalid chunk type: {0} at position {1}";
        public const string CorruptedChunkData = "Corrupted chunk data at position {0}. Expected length: {1}";
        public const string InvalidPrimitiveType = "Invalid primitive type: {0}";
        public const string ParsingError = "Error parsing data: {0}. Details: {1}";

        // 유효성 검사 관련 에러
        public const string InvalidEncoding = "Invalid encoding detected in file: {0}";
        public const string InvalidVersion = "Unsupported version: {0} for chunk: {1}";
        public const string InvalidColorData = "Invalid color data at position: {0}";

        // 비동기 처리 관련 에러
        public const string AsyncOperationFailed = "Asynchronous operation failed: {0}";
        public const string TessellationError = "Error during tessellation of primitive: {0}. Details: {1}";

        // 일반 에러
        public const string GeneralLoadError = "Error loading file: {0}. Details: {1}";
        public const string UnexpectedError = "Unexpected error occurred: {0}";
    }
}
