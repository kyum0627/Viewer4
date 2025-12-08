using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL API 오류 예외
    /// OpenGL 호출 실패 시 발생하며 ErrorCode를 포함
    /// </summary>
    public class GLStateException : Exception
    {
        /// <summary>
        /// OpenGL 오류 코드
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// 오류 코드와 메시지로 예외 생성
        /// </summary>
        public GLStateException(ErrorCode errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 오류 코드, 메시지, 내부 예외로 예외 생성
        /// </summary>
        public GLStateException(ErrorCode errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public GLStateException() : base()
        {
        }

        /// <summary>
        /// 메시지로 예외 생성
        /// </summary>
        public GLStateException(string? message) : base(message)
        {
        }

        /// <summary>
        /// 메시지와 내부 예외로 예외 생성
        /// </summary>
        public GLStateException(string? message, Exception? innerException) 
            : base(message, innerException)
        {
        }
    }
}
