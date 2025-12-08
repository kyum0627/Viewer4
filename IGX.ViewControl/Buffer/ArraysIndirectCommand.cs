using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ArraysIndirectCommand
    {
        public uint count;
        public uint instanceCount;
        public uint first;
        public uint baseInstance;
    }
}
