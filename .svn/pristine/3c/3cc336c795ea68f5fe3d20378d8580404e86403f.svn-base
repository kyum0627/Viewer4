using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    public interface IBuffer : IDisposable
    {
        int Handle { get; }
        bool IsValid { get; }
        int SizeInBytes { get; }
        void Bind();
        void Unbind();
    }

    /// <summary>
    /// GPU 버퍼 생성, 데이터 업로드, 갱신, 삭제를 책임지는 최소 단위로“OpenGL Buffer Object 자체”에 해당
    /// GPU 메모리만 관리
    /// CPU에서 접근이 필요할 때마다 GL.GetBufferSubData를 호출해야 함 (매우 느림)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GLBuffer<T> : IBuffer where T : unmanaged
    {
        private const int InvalidHandle = 0;
        private bool _isDisposed;
        private readonly BufferUsageHint _usage;
        private int _sizeInBytes;
        public BufferTarget BufferTarget { get; }
        public BufferUsageHint BufferUsageHint => _usage;
        public int Count { get; protected set; }
        public int Handle { get; private set; }
        public bool IsDisposed => _isDisposed;
        public bool IsValid => Handle != InvalidHandle && !_isDisposed;
        public int SizeInBytes => _sizeInBytes;
        protected GLBuffer(BufferTarget target, BufferUsageHint usage)
        {
            BufferTarget = target;
            _usage = usage;
            GL.GenBuffers(1, out int handle);
            Handle = handle;
        }

        public virtual unsafe void SyncToGpuAll(ReadOnlySpan<T> data)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(GLBuffer<T>));
            GLUtil.EnsureContextActive(); // 추가: 컨텍스트 확인
            Bind();
            try
            {
                _sizeInBytes = data.Length * Unsafe.SizeOf<T>();
                Count = data.Length;
                var dataPtr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(data));
                GL.BufferData(BufferTarget, new nint(_sizeInBytes), (nint)dataPtr, _usage);
                GLUtil.ErrorCheck("BufferData"); // 추가
            }
            finally
            {
                Unbind();
            }
        }

        public virtual unsafe void SyncToGpuSub(ReadOnlySpan<T> data, nint offsetInBytes)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(GLBuffer<T>));

            var subDataSizeInBytes = data.Length * Unsafe.SizeOf<T>();

            GLUtil.EnsureContextActive();
            Bind();
            try
            {
                var dataPtr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(data));
                GL.BufferSubData(BufferTarget, offsetInBytes, new nint(subDataSizeInBytes), (nint)dataPtr);
                GLUtil.ErrorCheck("BufferSubData");
            }
            finally
            {
                Unbind();
            }
        }
        public virtual unsafe void GetData(Span<T> destination)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(GLBuffer<T>));
            var dataSizeToRead = Count * Unsafe.SizeOf<T>();
            if (destination.Length < Count)
            {
                throw new ArgumentException("Destination span is too small.", nameof(destination));
            }
            GLUtil.EnsureContextActive();
            Bind();
            try
            {
                fixed (T* ptr = destination)
                {
                    GL.GetBufferSubData(BufferTarget, nint.Zero, new nint(dataSizeToRead), (nint)ptr);
                }
                GLUtil.ErrorCheck("GetBufferSubData"); // 에러 체크 추가
            }
            finally
            {
                Unbind();
            }
        }

        public virtual ReadOnlySpan<T> GetCpuDataSnapshot()
        {
            if (!IsValid) throw new InvalidOperationException("Buffer is invalid or disposed.");
            if (Count == 0) return ReadOnlySpan<T>.Empty;
            var data = new T[Count];
            GetData(data.AsSpan());
            return data.AsSpan(0, Count);
        }

        public virtual void Bind()
        {
            if (BufferTarget == BufferTarget.ElementArrayBuffer && GL.GetInteger(GetPName.VertexArrayBinding) == 0)
                throw new InvalidOperationException("Cannot bind an ElementArrayBuffer without an active VAO.");
            GL.BindBuffer(BufferTarget, Handle);
        }
        public virtual void Unbind() => GL.BindBuffer(BufferTarget, 0);
        public virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (Handle != InvalidHandle)
            {
                if (GLUtil.IsContextActive())
                {
                    GLResourceManager.EnqueueForDeletion(Handle);
                }
                else
                {
                    Console.WriteLine($"Warning: Buffer {Handle} could not be enqueued for deletion due to missing OpenGL context.");
                }
                Handle = InvalidHandle;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}