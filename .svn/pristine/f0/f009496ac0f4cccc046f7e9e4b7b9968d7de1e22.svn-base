using IGX.ViewControl.Render;
using OpenTK.Mathematics;

namespace IGX.ViewControl
{
    public class LightingProvider : ILight
    {
        public Vector3 Direction { get; set; } = new Vector3(1, -2, 4).Normalized();
        public Vector3 Color { get; set; } = Vector3.One;
        public Vector3 Ambient { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
        public Vector3 Diffuse { get; set; } = Vector3.One;
        public Vector3 Specular { get; set; } = Vector3.One;
        public float AmbientStrength { get; set; } = RendererConstants.AmbientStrength;
        public float SpecularStrength { get; set; } = RendererConstants.SpecularStrength;
        public float Shininess { get; set; } = RendererConstants.Shininess;
        public void SetLightingUniforms(Shader shader)
        {
            shader.SetUniformIfExist("light.direction", Direction);
            shader.SetUniformIfExist("light.color", Color);
            shader.SetUniformIfExist("light.ambient", Ambient);
            shader.SetUniformIfExist("light.diffuse", Diffuse);
            shader.SetUniformIfExist("light.specular", Specular);
            shader.SetUniformIfExist("light.ambientStrength", AmbientStrength);
            shader.SetUniformIfExist("light.specularStrength", SpecularStrength);
            shader.SetUniformIfExist("light.shininess", Shininess);
        }
    }
}