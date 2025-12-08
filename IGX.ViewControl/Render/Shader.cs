using IGX.ViewControl.Buffer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace IGX.ViewControl.Render
{

    public class Shader : IDisposable
    {
        public int ProgramID { get; private set; }
        public string Name = string.Empty;
        public IReadOnlyDictionary<string, UniformInfo>? UniformLocations { get; private set; }
        public IReadOnlyDictionary<string, UniformBufferInfo>? UniformBuffers { get; private set; }
        private IMyCamera? _camera;
        private ILight? _lightingProvider;
        private bool disposed = false;
        private static readonly Dictionary<string, CachedShaderSource> _shaderSourceCache = new();
        private static readonly List<int> _pendingDeletionShaderPrograms = new();
        private static readonly object _deletionLock = new();

        // UBO 바인딩 포인트 자동 할당을 위한 정적 카운터
        private static int _nextBindingPoint = 0;
        private static readonly object _bindingPointLock = new();

        private static class ShaderConstants
        {
            public const string ShaderDirectory = "Shaders";
            public const string DefaultVertexShaderFileName = "uni_vert.glsl";
            public const string DefaultFragmentShaderFileName = "all_frag.glsl";
        }
        public Shader(string? name, string vertexSourceOrPath, string fragmentSourceOrPath, bool isFilePath = true)
        {
            string vertexShaderGLSL;
            string fragmentShaderGLSL;

            if(Name != null)
            {
                Name = name;
            }

            if (isFilePath)
            {
                string fullVertexPath = Path.Combine(AppContext.BaseDirectory, ShaderConstants.ShaderDirectory, vertexSourceOrPath);
                string fullFragmentPath = Path.Combine(AppContext.BaseDirectory, ShaderConstants.ShaderDirectory, fragmentSourceOrPath);
                ShaderFilePathValidation(fullVertexPath, fullFragmentPath);
                vertexShaderGLSL = ReadGLSLtext(fullVertexPath);
                fragmentShaderGLSL = ReadGLSLtext(fullFragmentPath);
            }
            else
            {
                vertexShaderGLSL = vertexSourceOrPath;
                fragmentShaderGLSL = fragmentSourceOrPath;
            }

            int vertexShaderHandle = CompileShaderInternal(ShaderType.VertexShader, vertexShaderGLSL);
            int fragmentShaderHandle = CompileShaderInternal(ShaderType.FragmentShader, fragmentShaderGLSL);
            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, vertexShaderHandle);
            GL.AttachShader(ProgramID, fragmentShaderHandle);
            LinkProgramInternal(ProgramID);
            GL.DetachShader(ProgramID, vertexShaderHandle);
            GL.DetachShader(ProgramID, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
            CacheUniforms();
            CacheUniformBuffers();

            GLUtil.ErrorCheck();
        }

        private static int CompileShaderInternal(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);

            if (success == (int)All.False)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                string errorMessage = $"Shader (Type: {type}) 컴파일 오류:\n{infoLog}";
                Debug.WriteLine(errorMessage);
                GL.DeleteShader(shader);
                throw new Exception(errorMessage);
            }
            return shader;
        }

        private static void LinkProgramInternal(int program)
        {
            GL.LinkProgram(program);
            GLUtil.ErrorCheck();

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Shader Program Link ErrorCheck (ID: {program}): {infoLog}");
            }
            Debug.WriteLine($"Program {program} linked successfully.");

            GL.ValidateProgram(program);
            GLUtil.ErrorCheck();

            GL.GetProgram(program, GetProgramParameterName.ValidateStatus, out int validateStatus);
            if (validateStatus == (int)All.False)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                LogWarning($"Program Validation ErrorCheck (ID: {program}): {infoLog}");
            }
            else
            {
                Debug.WriteLine($"Program {program} validated successfully.");
            }
        }

        private void CacheUniforms()
        {
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
            GLUtil.ErrorCheck();

            var cachedUniforms = new Dictionary<string, UniformInfo>();

            for (int i = 0; i < numberOfUniforms; i++)
            {
                string name = GL.GetActiveUniform(ProgramID, i, out int size, out ActiveUniformType type);
                GLUtil.ErrorCheck();

                if (name.EndsWith("[0]"))
                {
                    name = name.Replace("[0]", "");
                }

                int location = GL.GetUniformLocation(ProgramID, name);
                GLUtil.ErrorCheck();

                if (location != -1 && !cachedUniforms.ContainsKey(name))
                {
                    cachedUniforms.Add(name, new UniformInfo(name, location, type, size));
                }
            }
            UniformLocations = cachedUniforms;
        }

        private void CacheUniformBuffers()
        {
            GL.GetProgram(ProgramID, GetProgramParameterName.ActiveUniformBlocks, out int numUniformBlocks);
            GLUtil.ErrorCheck();
            var cachedUniformBuffers = new Dictionary<string, UniformBufferInfo>();
            lock (_bindingPointLock)
            {
                for (int i = 0; i < numUniformBlocks; i++)
                {
                    // 1. 블록 이름의 최대 길이를 조회.
                    GL.GetActiveUniformBlock(ProgramID, i, ActiveUniformBlockParameter.UniformBlockNameLength, out int nameLength);

                    // 2. 이름을 저장할 StringBuilder를 생성.
                    var name = string.Empty;
                    int written;

                    // 3. 5개의 인자를 받는 GL.GetActiveUniformBlockName 오버로드를 사용.
                    GL.GetActiveUniformBlockName(ProgramID, i, nameLength, out written, out name);
                    string blockName = name.ToString();

                    if (string.IsNullOrEmpty(blockName))
                    {
                        Debug.WriteLine($"[경고] Program {ProgramID}: uniform block index {i}에 대한 이름을 가져오지 못함.");
                        continue;
                    }

                    int bindingPoint = _nextBindingPoint++; // 고유한 바인딩 포인트 할당
                    int blockSize = 0;
                    GL.GetActiveUniformBlock(ProgramID, i, ActiveUniformBlockParameter.UniformBlockDataSize, out blockSize);
                    GLUtil.ErrorCheck();
                    GL.UniformBlockBinding(ProgramID, i, bindingPoint);
                    GLUtil.ErrorCheck();
                    cachedUniformBuffers.Add(blockName, new UniformBufferInfo(blockName, i, bindingPoint, blockSize));
                    Debug.WriteLine($"[Shader] Program {ProgramID}: UBO '{blockName}' (Index: {i}) bound to Binding Point {bindingPoint}, Size: {blockSize}");
                }
            }
            UniformBuffers = cachedUniformBuffers;
        }

        public int GetUniformBlockBindingPoint(string blockName)
        {
            if (UniformBuffers != null && UniformBuffers.TryGetValue(blockName, out UniformBufferInfo info))
            {
                return info.BindingPoint;
            }
            return -1;
        }

        private static void ShaderFilePathValidation(params string[] paths)
        {
            if (paths != null)
            {
                foreach (string path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path)) continue;
                    if (!File.Exists(path))
                    {
                        throw new FileNotFoundException($"셰이더 파일이 없음: {path}");
                    }
                }
            }
        }

        public void BindUniforms(IMyCamera camera, ILight lighting)
        {
            if (camera == null || lighting == null || UniformLocations == null) return;
            // 카메라 uniform (참조로 즉시 사용)
            SetUniformIfExist("uView", camera.ViewMatrix);
            SetUniformIfExist("uProjection", camera.ProjectionMatrix);
            SetUniformIfExist("uViewPosition", camera.Position);
            // 조명 uniform
            SetUniformIfExist("uLightDirection", lighting.Direction);
            SetUniformIfExist("uLightColor", lighting.Color);
            SetUniformIfExist("uAmbientStrength", RendererConstants.AmbientStrength);
            SetUniformIfExist("uSpecularStrength", RendererConstants.SpecularStrength);
            SetUniformIfExist("uShininess", RendererConstants.Shininess);

            GLUtil.ErrorCheck();
        }
        private static string ReadGLSLtext(string path)
        {
            if (_shaderSourceCache.TryGetValue(path, out CachedShaderSource? cachedSource) && !cachedSource.IsStale(File.GetLastWriteTimeUtc(path)))
            {
                Debug.WriteLine($"[Shader Cache] '{path}' 캐시된 소스 로드 (변경 없음)");
                return cachedSource.Source;
            }

            Debug.WriteLine($"[Shader Cache] '{path}' 파일에서 소스 로드 (새로 로드 또는 변경됨)");
            string source = ReadShaderSourceFromFile(path);
            _shaderSourceCache[path] = new CachedShaderSource(source, File.GetLastWriteTimeUtc(path));
            return source;
        }

        private static string ReadShaderSourceFromFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException ex)
            {
                throw new Exception($"셰이더 소스 코드 읽기 실패: {ex.Message} ({path})", ex);
            }
        }
        //public void Use(IDrawBuffer renderer)
        //{
        //    if (disposed) throw new ObjectDisposedException("Shader", "셰이더 객체가 이미 해제됨.");
        //    if (ProgramID == 0) throw new InvalidOperationException("셰이더 프로그램이 초기화되지 않았음.");
        //    int previousProgram = GL.GetInteger(GetPName.CurrentProgram);
        //    GL.UseProgram(ProgramID);
        //    BindUniforms(_camera!, _lightingProvider!);
        //    //renderer.ExecuteWithShader()
        //    //renderer.Execute(PrimitiveType.TriangleIndices);
        //}
        public IDisposable Use(IMyCamera camera, ILight lighting)
        {
            _camera = camera;
            _lightingProvider = lighting;
            int previousProgram = GL.GetInteger(GetPName.CurrentProgram);
            GL.UseProgram(ProgramID);
            BindUniforms(camera, lighting);
            return new ShaderUseScope(previousProgram, this);  // 스코프에서 UnUse 호출
        }
        public IDisposable Use()
        {
            if (disposed) throw new ObjectDisposedException("Shader", "Shader object already disposed.");
            if (ProgramID == 0) throw new InvalidOperationException("Shader program not initialized.");
            int previousProgram = GL.GetInteger(GetPName.CurrentProgram);
            GL.UseProgram(ProgramID);
            return new ShaderUseScope(previousProgram, this);
        }

        public int GetHandle() => ProgramID;

        // --- Uniform Setters ---
        private static readonly Dictionary<Type, Action<int, object>> UniformDataSetters = new()
        {
            [typeof(int)] = (loc, val) => GL.Uniform1(loc, (int)val),
            [typeof(float)] = (loc, val) => GL.Uniform1(loc, (float)val),
            [typeof(Vector2)] = (loc, val) => GL.Uniform2(loc, (Vector2)val),
            [typeof(Vector3)] = (loc, val) => GL.Uniform3(loc, (Vector3)val),
            [typeof(Vector4)] = (loc, val) => GL.Uniform4(loc, (Vector4)val),
            [typeof(OpenTK.Mathematics.Color4)] = (loc, val) => GL.Uniform4(loc, (OpenTK.Mathematics.Color4)val),
            [typeof(Vector2i)] = (loc, val) => { Vector2i v = (Vector2i)val; GL.Uniform2(loc, v.X, v.Y); },
            [typeof(Vector3i)] = (loc, val) => { Vector3i v = (Vector3i)val; GL.Uniform3(loc, v.X, v.Y, v.Z); },
            [typeof(Vector4i)] = (loc, val) => { Vector4i v = (Vector4i)val; GL.Uniform4(loc, v.X, v.Y, v.Z, v.W); },
            [typeof(Matrix3)] = (loc, val) => { Matrix3 v = (Matrix3)val; GL.UniformMatrix3(loc, false, ref v); },
            [typeof(Matrix4)] = (loc, val) => { Matrix4 v = (Matrix4)val; GL.UniformMatrix4(loc, false, ref v); },
            [typeof(int[])] = (loc, val) => { int[] arr = (int[])val; GL.Uniform1(loc, arr.Length, arr); },
            [typeof(float[])] = (loc, val) => { float[] arr = (float[])val; GL.Uniform1(loc, arr.Length, arr); },
            [typeof(Color4[])] = (loc, val) => { Color4[] arr = (Color4[])val; if (arr.Length > 0) { GL.Uniform4(loc, arr.Length, ref arr[0].R); } },
            [typeof(Matrix4[])] = (loc, val) => { Matrix4[] arr = (Matrix4[])val; if (arr.Length > 0) { GL.UniformMatrix4(loc, arr.Length, false, ref arr[0].Row0.X); } }
        };

        public void SetUniformIfExist<T>(string name, T value)
        {
            if (GL.GetInteger(GetPName.CurrentProgram) != ProgramID)
            {
#if DEBUG
                Debug.WriteLine($"[경고] Uniform '{name}' 설정 실패: 셰이더 프로그램 (ID: {ProgramID})이 현재 활성화되어 있지 않음.");
#endif
                return;
            }

            if (!UniformLocations!.TryGetValue(name, out UniformInfo uniformInfo))
            {
#if DEBUG
                Debug.WriteLine($"[경고] 유니폼 '{name}' 없음 (셰이더에서 최적화 제거되었을 수도 있음)");
#endif
                return;
            }

            if (UniformDataSetters.TryGetValue(typeof(T), out Action<int, object>? setter))
            {
                setter(uniformInfo.Location, value!);
                GLUtil.ErrorCheck();
            }
            else
            {
#if DEBUG
                Debug.WriteLine($"[오류] OpenGL에서 지원하지 않는 uniform 데이터 type: {typeof(T).Name}");
#endif
            }
        }

        public void SetUniforms(Dictionary<string, object> uniformValues)
        {
            if (GL.GetInteger(GetPName.CurrentProgram) != ProgramID)
            {
#if DEBUG
                Debug.WriteLine($"[경고] 여러 유니폼 설정 실패: 셰이더 프로그램 (ID: {ProgramID})이 현재 활성화되어 있지 않음.");
#endif
                return;
            }

            if (UniformLocations == null) return;

            foreach (KeyValuePair<string, object> entry in uniformValues)
            {
                string name = entry.Key;
                object value = entry.Value;
                Type valueType = value.GetType();

                if (!UniformLocations.TryGetValue(name, out UniformInfo uniformInfo))
                {
#if DEBUG
                    Debug.WriteLine($"[경고] 유니폼 '{name}' 없음 (셰이더에서 최적화 제거되었을 수도 있음)");
#endif
                    continue;
                }

                if (UniformDataSetters.TryGetValue(valueType, out Action<int, object>? setter))
                {
                    setter(uniformInfo.Location, value);
                    GLUtil.ErrorCheck();
                }
                else
                {
#if DEBUG
                    Debug.WriteLine($"[오류] OpenGL에서 지원하지 않는 uniform 데이터 type: {valueType.Name} for uniform '{name}'");
#endif
                }
            }
        }

        public void Dispose()
        {
            ShaderManager.Remove(Name);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    UniformLocations = null;
                    UniformBuffers = null;
                }

                if (ProgramID != 0)
                {
                    lock (_deletionLock)
                    {
                        _pendingDeletionShaderPrograms.Add(ProgramID);
                    }
                    ProgramID = 0;
                }
                disposed = true;
            }
        }

        ~Shader()
        {
            Dispose(false);
        }

        private static void LogWarning(string message)
        {
            Debug.WriteLine($"경고: {message}");
        }

        private class CachedShaderSource(string source, DateTime lastModified)
        {
            public string Source { get; } = source;
            private DateTime LastModified { get; } = lastModified;
            public bool IsStale(DateTime lastModified) => lastModified > LastModified;
        }

        public static void ProcessPendingDeletions()
        {
            lock (_deletionLock)
            {
                if (_pendingDeletionShaderPrograms.Count == 0) return;

                var deletedIds = new List<int>();
                foreach (int id in _pendingDeletionShaderPrograms)
                {
                    if (GL.IsProgram(id))
                    {
                        try
                        {
                            GL.DeleteProgram(id);
                            GLUtil.ErrorCheck();
                            deletedIds.Add(id);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[경고] 대기 중인 셰이더 프로그램 삭제 중 오류 발생 (ID: {id}): {ex.Message}");
                        }
                    }
                    else
                    {
                        deletedIds.Add(id);
                    }
                }
                _pendingDeletionShaderPrograms.RemoveAll(id => deletedIds.Contains(id));
            }
        }

        public readonly struct UniformInfo(string name, int location, ActiveUniformType type, int size)
        {
            public string Name { get; } = name;
            public int Location { get; } = location;
            public ActiveUniformType Type { get; } = type;
            public int Size { get; } = size;
            public override readonly string ToString() => $"Name: {Name}, Location: {Location}, Type: {Type}, Size: {Size}";
        }

        // UBO 정보를 저장하기 위한 구조체
        public readonly struct UniformBufferInfo(string name, int index, int bindingPoint, int size)
        {
            public string Name { get; } = name;
            public int Index { get; } = index;
            public int BindingPoint { get; } = bindingPoint;
            public int Size { get; } = size;
            public override readonly string ToString() => $"Name: {Name}, Index: {Index}, BindingPoint: {BindingPoint}, Size: {Size}";
        }

        private class ShaderUseScope(int previousProgram, Shader shader) : IDisposable
        {
            private readonly int _previousProgram = previousProgram;
            private readonly Shader _shader = shader;
            private bool _disposed = false;
            public void Dispose()
            {
                if (!_disposed)
                {
                    GL.UseProgram(_previousProgram);
                    _disposed = true;
                }
            }
        }
    }
}