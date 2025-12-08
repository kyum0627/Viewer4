using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    public class ImmutableBuffer<T> : IDisposable where T : struct
    {
        private static readonly ConcurrentQueue<int> _pendingDeletionHandles = new();
        private readonly object _lockObject = new();
        private T[]? _cpuData;

        public int Handle { get; private set; }
        public int SizeInBytes { get; private set; }
        public bool IsDisposed { get; private set; }
        public BufferTarget Target { get; }
        public BufferStorageFlags Flags { get; }
        public bool KeepCpuData => _cpuData != null;
        public Span<T> CpuData
        {
            get
            {
                if (_cpuData == null)
                    throw new InvalidOperationException("CPU data is not maintained.");

                lock (_lockObject)
                {
                    return _cpuData.AsSpan();
                }
            }
        }
        public ImmutableBuffer(BufferTarget target, T[] data, BufferStorageFlags flags, bool keepCpuData = false)
        {
            GLUtil.EnsureContextActive();

            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data), "Initial data array cannot be null or empty.");
            }

            Target = target;
            Flags = flags;
            int elementCount = data.Length;
            SizeInBytes = data.Length * Marshal.SizeOf<T>();
            try
            {
                Handle = GL.GenBuffer();

                GL.NamedBufferStorage(Handle, SizeInBytes, data, flags);
                GLUtil.ErrorCheck();

                if (keepCpuData)
                {
                    // CPU 데이터 유지 (lock으로 보호)
                    lock (_lockObject)
                    {
                        _cpuData = new T[data.Length];
                        data.CopyTo(_cpuData.AsSpan());
                    }
                }
            }
            catch (Exception ex)
            {
                // 생성 실패 시 핸들 즉시 정리 (Dispose 대신 GL.DeleteBuffer를 바로 호출할 수도 있지만,
                // 비동기 삭제 패턴 유지를 위해 큐에 넣고 Handle = 0 처리)
                if (Handle != 0)
                {
                    _pendingDeletionHandles.Enqueue(Handle);
                    Handle = 0;
                }
                throw new InvalidOperationException("Failed to initialize ImmutableBuffer.", ex);
            }
        }

        public void UpdateData(ReadOnlySpan<T> data, int offsetInElements = 0)
        {
            GLUtil.EnsureContextActive();
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ImmutableBuffer<T>), "Cannot update a disposed _instancedGeometry.");
            }
            if ((Flags & BufferStorageFlags.DynamicStorageBit) == 0)
            {
                throw new InvalidOperationException("This immutable buffer does not have write access. It was not created with BufferStorageFlags.DynamicStorageBit.");
            }
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "SyncToGpuSub data array cannot be null.");
            }
            if (offsetInElements < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offsetInElements), "Offset cannot be negative.");
            }

            int elementSize = Marshal.SizeOf<T>();
            int byteOffset = offsetInElements * elementSize;
            int dataSizeBytes = data.Length * elementSize;

            if (byteOffset + dataSizeBytes > SizeInBytes)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(data),
                    $"Update data exceeds buffer size. Buffer size: {SizeInBytes} bytes, " +
                    $"Update range: {byteOffset} to {byteOffset + dataSizeBytes} bytes.");
            }
            try
            {
                var temp = data.ToArray();
                GL.NamedBufferSubData(Handle, byteOffset, dataSizeBytes, temp);
                GLUtil.ErrorCheck();

                if (KeepCpuData)
                {
                    // CPU 데이터 업데이트는 lock으로 보호
                    lock (_lockObject)
                    {
                        data.CopyTo(_cpuData!.AsSpan(offsetInElements));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update ImmutableBuffer data.", ex);
            }
        }

        public void Bind()
        {
            GLUtil.EnsureContextActive();
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ImmutableBuffer<T>));
            }
            if (Handle == 0)
            {
                Console.WriteLine("Warning: Attempting to bind an invalid _instancedGeometry handle.");
                return;
            }
            GL.BindBuffer(Target, Handle);
        }
        public void Unbind()
        {
            GLUtil.EnsureContextActive();
            GL.BindBuffer(Target, 0);
        }
        public void Dispose()
        {
            lock (_lockObject)
            {
                if (IsDisposed)
                {
                    return;
                }

                if (Handle != 0)
                {
                    _pendingDeletionHandles.Enqueue(Handle);
                    Handle = 0;
                }
                _cpuData = null;
                IsDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public static void ProcessPendingDeletions()
        {
            while (_pendingDeletionHandles.TryDequeue(out int handle))
            {
                try
                {
                    if (handle != 0)
                    {
                        GL.DeleteBuffer(handle);
                        Console.WriteLine($"Buffer {handle} deleted.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete _instancedGeometry {handle}: {ex.Message}");
                }
            }
        }
    }
}