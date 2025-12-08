using OpenTK.Mathematics;

// for serialize
using MessagePack;
using MessagePack.Formatters;

namespace IGX.Geometry.DataStructure.MeshDecomposer
{
    public class Vector3Formatter : IMessagePackFormatter<Vector3>
    {
        public void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            options.Security.DepthStep(ref reader);
            reader.ReadArrayHeader();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            reader.Depth--;
            return new Vector3(x, y, z);
        }
    }
}
