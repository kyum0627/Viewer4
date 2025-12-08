using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.GLDataStructure
{
    public record VertexAttributeDesc
    {
        public string? FieldName { get; init; } = string.Empty;
        public int Location { get; init; }
        public int Size { get; init; }
        public All GLType { get; init; }
        public bool Normalized { get; init; }
        public int Offset { get; init; }
        public int Divisor { get; init; } = 1;
        public bool IsMatrix4 { get; init; }
    }
}
