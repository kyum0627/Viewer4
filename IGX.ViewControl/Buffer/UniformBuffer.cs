using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace IGX.ViewControl.Buffer
{
    public class UniformBuffer<TUniforms> : MutableBuffer<TUniforms> where TUniforms : unmanaged
    {
        public UniformBuffer(ReadOnlySpan<TUniforms> data, BufferUsageHint usageHint, bool keepCpuData = false)
            : base(data, BufferTarget.UniformBuffer, usageHint, keepCpuData)
        {
            GLUtil.EnsureContextActive();
        }

        public void BindBase(int bindingIndex)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UniformBuffer<TUniforms>));
            if (bindingIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Binding index must be non-negative.");
            GLUtil.EnsureContextActive();

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingIndex, Handle);
            GLUtil.ErrorCheck();
        }
        public void UnbindBase(int bindingIndex)
        {
            if (bindingIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Binding index must be non-negative.");
            GLUtil.EnsureContextActive();

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingIndex, 0);
        }
        public void UpdateUniforms(int startIndex, ReadOnlySpan<TUniforms> newData)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(UniformBuffer<TUniforms>));
            if (startIndex < 0 || newData.Length == 0 || startIndex + newData.Length > Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Uniform data range is out of bounds.");
            GLUtil.EnsureContextActive();

            if (KeepCpuCopy)
            {
                var cpuData = CpuData;
                newData.CopyTo(cpuData.Slice(startIndex));  // CPU 업데이트 (KeepCpuCopy=true 시)
            }
            nint byteOffset = startIndex * Unsafe.SizeOf<TUniforms>();
            SyncToGpuSub(newData, byteOffset);

            GLUtil.ErrorCheck();
        }
    }
}