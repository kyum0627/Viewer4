using OpenTK.Graphics.OpenGL4;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace IGX.ViewControl.Buffer
{
    public interface IMutableBuffer<T> where T : struct
    {
        int Handle { get; }
        int Count { get; }
        bool KeepCpuCopy { get; }
        Span<T> CpuData { get; }

        void SyncToGpuSub(ReadOnlySpan<T> data, nint byteOffset = default);// CPU -> GPU 부분 업로드 (offset은 바이트 오프셋)
        void SyncToGpuAll(); // 전체를 GPU에 올림
        void EnsureCpuCapacity(int requiredElements); // CPU 버퍼 크기 보장 (필요하면 재할당)
        void ReallocateGpuBuffer(int newElementCount); // Resize (GPU 재할당 포함)
        void UploadElementToGpu(int index);
    }

    /// <summary>
    /// GPU 버퍼의 CPU 캐시 및 상태 관리자
    /// CPU-side 데이터 보존: cpuData 유지, 필요 시 ArrayPool 재할당
    /// GPU 동기화: SyncToGpuSub(), SyncToGpuSub(), SyncToGpuAll() 등으로 GPU 버퍼 반영
    /// 크기 가변 지원: ReallocateGpuBuffer(), AppendToBuffer(), RemoveCpuAt() 등으로 동적 버퍼 관리
    /// 멀티스레드 안전성: lock(_lock) 으로 동시 접근 방지
    /// 효율적 메모리 관리: ArrayPool<IndirectCommandData>를 통한 재활용
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MutableBuffer<T> : GLBuffer<T>, IMutableBuffer<T> where T : unmanaged
    {
        private readonly object _lock = new();
        private int _cpuDataCapacity;
        private readonly bool _KeepCpuCopy;
        public bool KeepCpuCopy => _KeepCpuCopy;
        public T[]? _cpuData;
        public Span<T> CpuData => _cpuData;

        public MutableBuffer(ReadOnlySpan<T> data, BufferTarget target, BufferUsageHint usageHint, bool keepCpuData = true)
            : base(target, usageHint)
        {
            _KeepCpuCopy = keepCpuData;
            lock (_lock)
            {
                if (_KeepCpuCopy)
                {
                    _cpuDataCapacity = data.Length;
                    _cpuData = ArrayPool<T>.Shared.Rent(_cpuDataCapacity);
                    data.CopyTo(_cpuData);
                }
            }
            base.SyncToGpuAll(data);
        }
        public void SyncToGpuSub(int startIndex, int count)
        {
            if (!_KeepCpuCopy || _cpuData == null)
                throw new InvalidOperationException("CPU _instancedGeometry is not maintained.");
            if (!GLUtil.IsContextActive())
                throw new InvalidOperationException("Cannot sync to GPU without an active OpenGL context.");
            if (startIndex < 0 || startIndex + count > Count)
                throw new ArgumentOutOfRangeException($"Range [{startIndex}, {startIndex + count}) exceeds data CommandCount ({Count}).");

            lock (_lock)
            {
                var offsetByte = startIndex * Unsafe.SizeOf<T>();
                base.SyncToGpuSub(_cpuData.AsSpan(startIndex, count), offsetByte);
            }
        }
        public override unsafe void SyncToGpuSub(ReadOnlySpan<T> data, nint byteOffset = default)
        {
            if (!GLUtil.IsContextActive())
                throw new InvalidOperationException("Cannot update data without an active OpenGL context.");

            lock (_lock)
            {
                var elementSize = Unsafe.SizeOf<T>();
                int elementOffset = (int)(byteOffset / elementSize);

                if (_KeepCpuCopy)
                {
                    var requiredCount = elementOffset + data.Length;
                    EnsureCpuCapacityInternal(requiredCount);

                    if (_cpuData != null && data.Length > 0)
                    {
                        data.CopyTo(_cpuData.AsSpan(elementOffset));
                    }
                }

                base.SyncToGpuSub(data, byteOffset);

                var newCount = elementOffset + data.Length;
                if (newCount > Count)
                {
                    Count = newCount;
                }
            }
        }
        public void SyncToGpuAll()
        {
            if (!_KeepCpuCopy) throw new InvalidOperationException("CPU copy is not maintained.");
            GLUtil.EnsureContextActive();

            lock (_lock)
            {
                if (_cpuData != null)
                {
                    base.SyncToGpuAll(_cpuData.AsSpan(0, Count));
                }
                else if (Count == 0)
                {
                    base.SyncToGpuAll(ReadOnlySpan<T>.Empty);
                }
            }
        }
        public void UpdateCpuElement(int index, T instanceData)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(InstanceBuffer<T>));
            if (!KeepCpuCopy) throw new InvalidOperationException("CPU data is not maintained. Set KeepCpuCopy to true.");
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            // CPU 배열만 갱신합니다.
            CpuData[index] = instanceData;
        }
        public void EnsureCpuCapacity(int requiredElements)
        {
            lock (_lock)
            {
                EnsureCpuCapacityInternal(requiredElements);
            }
        }
        private void EnsureCpuCapacityInternal(int requiredSize)
        {
            if (!_KeepCpuCopy) throw new InvalidOperationException("CPU copy is not maintained.");
            if (requiredSize <= _cpuDataCapacity) return;

            var newCapacity = Math.Max(requiredSize, _cpuDataCapacity == 0 ? 4 : _cpuDataCapacity * 2);
            var newBuffer = ArrayPool<T>.Shared.Rent(newCapacity);

            if (_cpuData != null)
            {
                _cpuData.AsSpan(0, Count).CopyTo(newBuffer.AsSpan(0, Count));
                ArrayPool<T>.Shared.Return(_cpuData, true);
            }

            _cpuData = newBuffer;
            _cpuDataCapacity = newCapacity;
        }
        public void ReallocateGpuBuffer(int newElementCount)
        {
            if (newElementCount < 0) throw new ArgumentOutOfRangeException(nameof(newElementCount));
            if (newElementCount == Count) return;
            if (!GLUtil.IsContextActive()) throw new InvalidOperationException("Cannot resize _instancedGeometry without an active OpenGL context.");

            lock (_lock)
            {
                EnsureCpuCapacity(newElementCount);
                var oldSizeInBytes = SizeInBytes;
                Count = newElementCount;
                if (_cpuData != null && newElementCount < Count)
                {
                    _cpuData.AsSpan(newElementCount, Count - newElementCount).Clear();
                }
                Bind();
                try
                {
                    var newSizeInBytes = newElementCount * Unsafe.SizeOf<T>();
                    GL.BufferData(BufferTarget, new nint(newSizeInBytes), nint.Zero, BufferUsageHint);
                    if (_cpuData != null && newElementCount > 0)
                    {
                        base.SyncToGpuAll(_cpuData.AsSpan(0, newElementCount));
                    }
                    else
                    {
                        base.SyncToGpuAll(ReadOnlySpan<T>.Empty);
                    }
                }
                finally
                {
                    Unbind();
                }
            }
        }

        public void AppendToBuffer(ReadOnlySpan<T> data)
        {
            if (data.IsEmpty) return;
            lock (_lock)
            {
                var originalCount = Count;
                var newCount = originalCount + data.Length;

                if (_KeepCpuCopy)
                {
                    EnsureCpuCapacityInternal(newCount);
                    if (_cpuData != null)
                    {
                        data.CopyTo(_cpuData.AsSpan(originalCount));
                    }
                }

                try
                {
                    ReallocateGpuBuffer(newCount);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException("Failed to add data to GPU buffer. Reallocation failed.", ex);
                }
            }
        }

        public void RemoveCpuAt(int index)
        {
            if (!_KeepCpuCopy) throw new InvalidOperationException("Cannot remove data when CPU copy is not maintained.");
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

            lock (_lock)
            {
                if (_cpuData == null) return; // 이미 Dispose되었거나 초기화되지 않은 경우

                var newCount = Count - 1;

                // CPU 메모리에서 요소 이동 및 정리
                if (index < newCount)
                {
                    _cpuData.AsSpan(index + 1, newCount - index).CopyTo(_cpuData.AsSpan(index));
                }
                _cpuData.AsSpan(newCount).Clear();
                Count = newCount;

                if (GLUtil.IsContextActive())
                {
                    // GPU 버퍼 재할당 및 전체 복사
                    ReallocateGpuBuffer(newCount);
                }
                else
                {
                    Console.WriteLine("Warning: GPU buffer size not updated due to missing OpenGL context. Call ReallocateGpuBuffer() later.");
                }
            }
        }
        public void UploadElementToGpu(int index)
        {
            if (!_KeepCpuCopy) throw new InvalidOperationException("CPU copy is not maintained.");
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (!GLUtil.IsContextActive()) throw new InvalidOperationException("Cannot upload element without an active OpenGL context.");

            lock (_lock)
            {
                if (_cpuData == null) return;

                base.SyncToGpuSub(_cpuData.AsSpan(index, 1), index * Unsafe.SizeOf<T>());
            }
        }
        public void UpdateElement(int index, T newData)
        {
            if (!_KeepCpuCopy) throw new InvalidOperationException("CPU copy is not maintained.");
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));

            lock (_lock)
            {
                if (_cpuData == null) return;

                _cpuData[index] = newData;

                if (GLUtil.IsContextActive())
                {
                    UploadElementToGpu(index);
                }
                else
                {
                    Console.WriteLine("Warning: CPU element updated, but GPU update skipped due to missing OpenGL context. Call SyncToGpuSub() later.");
                }
            }
        }
        public override ReadOnlySpan<T> GetCpuDataSnapshot()
        {
            lock (_lock)
            {
                if (_KeepCpuCopy)
                {
                    return _cpuData != null ? _cpuData.AsSpan(0, Count) : ReadOnlySpan<T>.Empty;
                }
                else
                {
                    return base.GetCpuDataSnapshot();
                }
            }
        }
    
        protected new void Dispose(bool disposing)
        {
            if (disposing && _cpuData != null)
            {
                lock (_lock)
                {
                    if (_cpuData != null)
                    {
                        ArrayPool<T>.Shared.Return(_cpuData, true);
                        _cpuData = null;
                        _cpuDataCapacity = 0;
                    }
                }
            }
            base.Dispose(disposing);
        }
    }
}