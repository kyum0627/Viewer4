using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

namespace IGX.ViewControl.Buffer
{
    public class ShaderStorageBuffer<TSSBO> : MutableBuffer<TSSBO> where TSSBO : unmanaged
    {
        public ShaderStorageBuffer(ReadOnlySpan<TSSBO> data, BufferUsageHint usageHint, bool keepCpuData = false)
            : base(data.ToArray(), BufferTarget.ShaderStorageBuffer, usageHint, keepCpuData)
        {
            GLUtil.EnsureContextActive();
            GLUtil.ErrorCheck();
        }

        public void BindBase(int bindingIndex)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ShaderStorageBuffer<TSSBO>));
            GLUtil.EnsureContextActive();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingIndex, Handle);
        }
        public void UnbindBase(int bindingIndex)
        {
            if (bindingIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(bindingIndex), "Binding index must be non-negative.");
            GLUtil.EnsureContextActive();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingIndex, 0);
        }

        public void UpdateInstances(int startIndex, ReadOnlySpan<TSSBO> newData)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(ShaderStorageBuffer<TSSBO>));
            if (!KeepCpuCopy) throw new InvalidOperationException("CPU data is not maintained. Set KeepCpuCopy to true in the constructor.");
            if (startIndex < 0 || newData.Length == 0 || startIndex + newData.Length > Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Storage data range is out of bounds.");
            GLUtil.EnsureContextActive();

            var cpuData = CpuData;
            newData.CopyTo(cpuData.Slice(startIndex));  // CPU 업데이트
            nint byteOffset = startIndex * Unsafe.SizeOf<TSSBO>();
            SyncToGpuSub(newData, byteOffset);

            GLUtil.ErrorCheck();
        }
    }
}