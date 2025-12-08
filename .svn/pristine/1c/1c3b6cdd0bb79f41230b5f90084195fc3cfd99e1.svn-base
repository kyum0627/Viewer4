using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;

namespace IGX.ViewControl.Buffer
{
    // 기하정보의 레이아웃 정의와 버퍼 참조점을 저장하며 다음의 역할을 수행
    // - 어떤 VBO가 몇 번 attribute로 읽히는지(예: position=0, normal=1)
    // - 인덱스 버퍼(EBO)가 연결되었는지
    // - Instance data attribute가 있는지
    // - stride/offset/format 등
    // 즉, attribute로 “읽기 방식”을 정의
    public sealed class VertexArrayObject
    {
        public int Handle { get; private set; }
        private bool disposedValue = false;
        private static readonly ConcurrentQueue<int> _pendingDeletionHandles = new();
        public bool IsValid => Handle != 0 && !disposedValue;

        public VertexArrayObject()
        {
            GLUtil.EnsureContextActive();
            Handle = GL.GenVertexArray();
        }

        public void Bind() => GL.BindVertexArray(Handle);
        public void Unbind() => GL.BindVertexArray(0);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (Handle != 0)
                {
                    _pendingDeletionHandles.Enqueue(Handle);
                    Handle = 0;
                }
                disposedValue = true;
            }
        }

        ~VertexArrayObject()
        {
            Dispose(false);
        }

        public static void ProcessPendingDeletions()
        {
            GL.GetError();

            while (_pendingDeletionHandles.TryDequeue(out int handleToDelete))
            {
                if (handleToDelete != 0)
                {
                    GL.DeleteVertexArray(handleToDelete);
                    GLUtil.ErrorCheck($"ProcessPendingDeletions에서 DeleteVertexArray (핸들: {handleToDelete})");
                }
            }
        }
    }
}