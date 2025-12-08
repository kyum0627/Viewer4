using IGX.ViewControl.Buffer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render.Materials
{
    #region --- Shader2: OpenTK 기반 Shader 관리 ---
    public class Shader2 : IDisposable
    {
        public int ProgramID { get; private set; }
        public IReadOnlyDictionary<string, int> UniformLocations { get; private set; } = new Dictionary<string, int>();
        private bool disposed = false;

        public Shader2(string vertexPathOrSource, string fragmentPathOrSource, bool isFile = true)
        {
            string vertexSource = isFile ? File.ReadAllText(vertexPathOrSource) : vertexPathOrSource;
            string fragmentSource = isFile ? File.ReadAllText(fragmentPathOrSource) : fragmentPathOrSource;

            int vertex = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, vertex);
            GL.AttachShader(ProgramID, fragment);
            GL.LinkProgram(ProgramID);

            GL.DetachShader(ProgramID, vertex);
            GL.DetachShader(ProgramID, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            CacheUniforms();
        }

        private void CacheUniforms()
        {
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out int count);
            var uniforms = new Dictionary<string, int>();
            for (int i = 0; i < count; i++)
            {
                string name = GL.GetActiveUniform(ProgramID, i, out int size, out ActiveUniformType type);
                if (name.EndsWith("[0]")) name = name.Replace("[0]", "");
                int location = GL.GetUniformLocation(ProgramID, name);
                uniforms[name] = location;
            }
            UniformLocations = uniforms;
        }

        private static int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status != (int)All.True)
                throw new Exception($"Shader compile error ({type}): {GL.GetShaderInfoLog(shader)}");
            return shader;
        }

        public void Use() => GL.UseProgram(ProgramID);

        public void Dispose()
        {
            if (!disposed)
            {
                if (GL.IsProgram(ProgramID)) GL.DeleteProgram(ProgramID);
                disposed = true;
            }
        }
    }
    #endregion

    #region --- MaterialPropertyBlock: Unity 스타일 Property 캐시 ---
    public class MaterialPropertyBlock
    {
        public Dictionary<string, float> Floats = new();
        public Dictionary<string, Vector2> Vector2s = new();
        public Dictionary<string, Vector3> Vector3s = new();
        public Dictionary<string, Vector4> Vector4s = new();

        public Dictionary<string, Matrix4> Matrices = new();
        public Dictionary<string, TextureBuffer> Textures = new();

        public void SetFloat(string name, float value) => Floats[name] = value;
        public void SetVector2(string name, Vector2 vec) => Vector2s[name] = vec;
        public void SetVector3(string name, Vector3 vec) => Vector3s[name] = vec;
        public void SetVector4(string name, Vector4 vec) => Vector4s[name] = vec;
        public void SetMatrix(string name, Matrix4 m) => Matrices[name] = m;
        public void SetTexture(string name, TextureBuffer t) => Textures[name] = t;
    }
    #endregion

    #region --- ShaderWrapper: Material + Shader2 관리 ---
    public class ShaderWrapper
    {
        private readonly Shader2 _shader;
        private readonly MaterialPropertyBlock _properties = new();

        public ShaderWrapper(Shader2 shader)
        {
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
        }

        // Property API
        public void SetFloat(string name, float value) => _properties.SetFloat(name, value);
        public void SetVector3(string name, Vector3 vec) => _properties.SetVector3(name, vec);
        public void SetVector4(string name, Vector4 vec) => _properties.SetVector4(name, vec);
        public void SetMatrix(string name, Matrix4 mat) => _properties.SetMatrix(name, mat);
        public void SetTexture(string name, TextureBuffer tex) => _properties.SetTexture(name, tex);

        // Shader2에 Property 적용
        public void ApplyProperties()
        {
            _shader.Use();

            foreach (var f in _properties.Floats)
                GL.Uniform1(_shader.UniformLocations[f.Key], f.Value);

            foreach (var v in _properties.Vector2s)
                GL.Uniform2(_shader.UniformLocations[v.Key], v.Value);

            foreach (var v in _properties.Vector3s)
                GL.Uniform3(_shader.UniformLocations[v.Key], v.Value);

            foreach (var v in _properties.Vector4s)
                GL.Uniform4(_shader.UniformLocations[v.Key], v.Value);

            foreach (var m in _properties.Matrices)
            {
                var mm = m.Value;
                GL.UniformMatrix4(_shader.UniformLocations[m.Key], false, ref mm);
            }

            foreach (var kvp in _properties.Textures)
            {
                string uniformName = kvp.Key;
                TextureBuffer tex = kvp.Value;

                if (!tex.IsValid)
                    continue;

                int unitIndex = 0; // 필요하면 외부에서 관리
                GL.ActiveTexture(TextureUnit.Texture0 + unitIndex);
                GL.BindTexture(TextureTarget.Texture2D, tex.Handle);

                if (_shader.UniformLocations.TryGetValue(uniformName, out int location))
                {
                    GL.Uniform1(location, unitIndex);
                }
            }
        }

        public Shader2 Shader => _shader;
    }
    #endregion

    #region --- ShaderBinder: 카메라/조명 바인딩 ---
    public class ShaderBinder
    {
        private readonly ShaderWrapper _shader;

        public ShaderBinder(ShaderWrapper shader) => _shader = shader;

        public void BindCamera(MyCamera cam)
        {
            _shader.SetMatrix("uView", cam.ViewMatrix);
            _shader.SetMatrix("uProjection", cam.ProjectionMatrix);
        }

        public void BindModel(Matrix4 modelMatrix)
        {
            _shader.SetMatrix("uModel", modelMatrix);
        }

        public void BindLight(ILight light)
        {
            _shader.SetVector3("uLightDirection", light.Direction);
            _shader.SetVector3("uLightColor", light.Color);
            _shader.SetFloat("uAmbientStrength", 0.1f);
            _shader.SetFloat("uSpecularStrength", 0.5f);
            _shader.SetFloat("uShininess", 32f);
        }
    }
    #endregion

    #region --- MeshRendererWrapper: Mesh + ShaderWrapper 렌더링 ---
    //public class MeshRendererWrapper
    //{
    //    private readonly Mesh _mesh;
    //    private readonly ShaderWrapper PassShader;

    //    public MeshRendererWrapper(Mesh _clipSystem, ShaderWrapper PassShader)
    //    {
    //        _mesh = _clipSystem;
    //        PassShader = PassShader;
    //    }

    //    public void Render(Matrix4 modelMatrix)
    //    {
    //        PassShader.ApplyProperties();
    //        Graphics.DrawMesh(_mesh, modelMatrix, PassShader.Shader);
    //    }
    //}
    #endregion
}
