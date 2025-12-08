using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IndirectCommandData
    {
        public uint count;          // 인덱스 개수 (IndexCount와 동일)
        public uint instanceCount;  // 인스턴스 개수
        public uint firstIndex;     // 인덱스 배열의 시작 오프셋
        public uint baseVertex;      // VBO의 시작 정점 오프셋
        public uint baseInstance;   // 인스턴스 버퍼의 시작 오프셋
    }

    // GPU에 여러 DrawCommand를 통째로 넣고
    // - MultiDrawElementsIndirect(), 또는, MultiDrawArraysIndirect() 로 한 번에 렌더링.
    // - CPU의 Execute 호출을 최소화하므로 고성능으로 렌더링
    // - SceneGraph에서 RendererPass로 전달되는 최종 렌더명령 목록
    public sealed class IndirectCommandBuffer : MutableBuffer<IndirectCommandData> 
    {
        private readonly object _lockObj = new();
        public ReadOnlySpan<IndirectCommandData> Commands
        {
            get
            {
                if (!KeepCpuCopy || _cpuData == null)
                    return ReadOnlySpan<IndirectCommandData>.Empty;

                return new ReadOnlySpan<IndirectCommandData>(_cpuData, 0, Count);
            }
        }

        public IndirectCommandBuffer(ReadOnlySpan<IndirectCommandData> commandsData, BufferUsageHint usage, bool keepCpuData = true)
            : base(commandsData.ToArray(), BufferTarget.DrawIndirectBuffer, usage, keepCpuData)
        {
            GLUtil.EnsureContextActive();
            ValidateCommandStruct();
        }
        //public IndirectCommandBuffer(int capacity, BufferUsageHint usage, bool keepCpuData = true)
        //    : base(new IndirectCommandData[capacity], BufferTarget.DrawIndirectBuffer, usage, keepCpuData)
        //{
        //    GLUtil.EnsureContextActive();
        //    ValidateCommandStruct();
        //}
        public void UpdateData(int startIndex, ReadOnlySpan<IndirectCommandData> newData)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(IndirectCommandBuffer));//<IndirectCommandData>));
            if (!KeepCpuCopy) throw new InvalidOperationException("CPU commandsData is not maintained. Set KeepCpuCopy to true in the constructor.");
            if (startIndex < 0 || newData.Length == 0 || startIndex + newData.Length > Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Command data range is out of bounds.");
            GLUtil.EnsureContextActive();
            lock (_lockObj)
            {
                if (_cpuData != null)
                {
                    var span = _cpuData.AsSpan();
                    newData.CopyTo(span.Slice(startIndex, newData.Length));
                }
                SyncToGpuSub(newData, startIndex * Unsafe.SizeOf<IndirectCommandData>());
            }
            SyncToGpuSub(newData, startIndex * Unsafe.SizeOf<IndirectCommandData>());
            GLUtil.ErrorCheck();
        }
        public override void Bind()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(IndirectCommandBuffer));//<IndirectCommandData>));
            }
            if (!IsContextLikelyActive())
            {
                throw new InvalidOperationException("Cannot bind command _instancedGeometry without an active OpenGL context.");
            }
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, Handle);
            GLUtil.ErrorCheck();
        }
        public override void Unbind()
        {
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, 0);
        }
        private void ValidateCommandStruct()
        {
            int expectedSize = 
                typeof(IndirectCommandData) == typeof(IndirectCommandData) ? 20 : 
                typeof(IndirectCommandData) == typeof(ArraysIndirectCommand) ? 16 :
                -1;
            if (expectedSize != -1 && Unsafe.SizeOf<IndirectCommandData>() != expectedSize)
            {
                Console.WriteLine($"Warning: Type '{typeof(IndirectCommandData).Name}' size ({Unsafe.SizeOf<IndirectCommandData>()} bytes) does not match expected draw command size ({expectedSize} bytes).");
            }
        }

        private static bool IsContextLikelyActive()
        {
            try
            {
                GL.GetError();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}