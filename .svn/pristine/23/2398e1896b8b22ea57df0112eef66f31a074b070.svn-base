namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 버퍼 업데이트 범위 검증 유틸리티
    /// 모든 버퍼 클래스에서 일관된 범위 검증 제공
    /// </summary>
    internal static class BufferValidation
    {
        /// <summary>
        /// 버퍼 업데이트 범위 검증
        /// </summary>
        /// <param name="startIndex">시작 인덱스</param>
        /// <param name="dataLength">데이터 길이</param>
        /// <param name="bufferSize">버퍼 크기</param>
        /// <param name="paramName">파라미터 이름</param>
        /// <exception cref="ArgumentOutOfRangeException">범위가 유효하지 않을 때</exception>
        public static void ValidateUpdateRange(int startIndex, int dataLength, int bufferSize, string paramName)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(paramName, 
                    $"시작 인덱스 {startIndex}는 0 이상이어야 합니다.");
            
            if (dataLength < 0)
                throw new ArgumentOutOfRangeException(nameof(dataLength), 
                    $"데이터 길이 {dataLength}는 0 이상이어야 합니다.");
            
            if (startIndex + dataLength > bufferSize)
                throw new ArgumentOutOfRangeException(paramName, 
                    $"범위 [{startIndex}, {startIndex + dataLength})가 버퍼 크기 {bufferSize}를 초과합니다.");
        }

        /// <summary>
        /// 단일 인덱스 검증
        /// </summary>
        /// <param name="index">인덱스</param>
        /// <param name="bufferSize">버퍼 크기</param>
        /// <param name="paramName">파라미터 이름</param>
        /// <exception cref="ArgumentOutOfRangeException">인덱스가 유효하지 않을 때</exception>
        public static void ValidateIndex(int index, int bufferSize, string paramName)
        {
            if (index < 0 || index >= bufferSize)
                throw new ArgumentOutOfRangeException(paramName, 
                    $"인덱스 {index}가 범위 [0, {bufferSize})를 벗어났습니다.");
        }

        /// <summary>
        /// 버퍼 크기 검증
        /// </summary>
        /// <param name="newSize">새 크기</param>
        /// <param name="currentSize">현재 크기</param>
        /// <param name="paramName">파라미터 이름</param>
        /// <exception cref="ArgumentException">크기가 유효하지 않을 때</exception>
        public static void ValidateBufferSize(int newSize, int currentSize, string paramName)
        {
            if (newSize < 0)
                throw new ArgumentException($"버퍼 크기 {newSize}는 0 이상이어야 합니다.", paramName);
            
            if (newSize < currentSize)
                throw new ArgumentException(
                    $"새 크기 {newSize}는 현재 크기 {currentSize}보다 작을 수 없습니다.", 
                    paramName);
        }

        /// <summary>
        /// OpenGL 컨텍스트 활성화 검증
        /// </summary>
        /// <param name="operationName">작업 이름</param>
        /// <exception cref="InvalidOperationException">컨텍스트가 비활성화되었을 때</exception>
        public static void EnsureGLContextActive(string operationName)
        {
            if (!GLUtil.IsContextActive())
                throw new InvalidOperationException(
                    $"OpenGL 컨텍스트가 활성화되지 않았습니다. '{operationName}' 작업을 수행할 수 없습니다.");
        }

        /// <summary>
        /// VAO 바인딩 상태 검증
        /// </summary>
        /// <exception cref="InvalidOperationException">VAO가 바인딩되지 않았을 때</exception>
        public static void EnsureVAOBound()
        {
            if (OpenTK.Graphics.OpenGL4.GL.GetInteger(OpenTK.Graphics.OpenGL4.GetPName.VertexArrayBinding) == 0)
                throw new InvalidOperationException(
                    "VAO가 바인딩되지 않았습니다. ElementBuffer는 VAO 없이 바인딩할 수 없습니다.");
        }
    }
}
