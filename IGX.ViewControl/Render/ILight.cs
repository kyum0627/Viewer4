using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    public interface ILight
    {
        Vector3 Direction { get; set; }
        Vector3 Color { get; set; }
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public float AmbientStrength { get; set; }
        public float SpecularStrength { get; set; }
        public float Shininess { get; set; }
        void SetLightingUniforms(Shader shader);
    }
}