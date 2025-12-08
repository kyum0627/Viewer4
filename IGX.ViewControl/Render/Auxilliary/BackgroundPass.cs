using IGX.Geometry.Common;
using IGX.ViewControl.Buffer;
using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Runtime.InteropServices;
using Image = SixLabors.ImageSharp.Image;

namespace IGX.ViewControl.Render.Auxilliary
{
    /// <summary>
    /// 배경 렌더링 Pass - RenderPassBase를 상속받아 배경 텍스처 렌더링에 집중
    /// </summary>
    internal class BackgroundPass : RenderPassBase
    {
        public override string Name => "BackgroundPass";
        
        private ITexture? _backgroundTexture;
        private int _vao, _vbo;

        [StructLayout(LayoutKind.Sequential)]
        private struct BackgroundVertex
        {
            public Vector2 Position;
            public Vector2 TexCoord;

            public BackgroundVertex(Vector2 position, Vector2 texCoord)
            {
                Position = position;
                TexCoord = texCoord;
            }

            public static readonly int SizeInBytes = Marshal.SizeOf<BackgroundVertex>();
            public static readonly int Stride = SizeInBytes;
        }

        private readonly BackgroundVertex[] _quadVertices =
        {
            new BackgroundVertex(new Vector2(-1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
            new BackgroundVertex(new Vector2(-1.0f, -1.0f), new Vector2(0.0f, 0.0f)),
            new BackgroundVertex(new Vector2( 1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
            new BackgroundVertex(new Vector2(-1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
            new BackgroundVertex(new Vector2( 1.0f, -1.0f), new Vector2(1.0f, 0.0f)),
            new BackgroundVertex(new Vector2( 1.0f,  1.0f), new Vector2(1.0f, 1.0f))
        };

        public override void Initialize(object? context1 = null, object? context2 = null)
        {
            base.Initialize(context1, context2);

            if (context1 is IMyCamera camera)
            {
                SetCamera(camera);
            }

            // 셰이더 생성
            PassShader = ShaderManager.Create(
                "BackGround",
                ShaderSource.backgroundVertexGlsl,
                ShaderSource.backgroundFragmentGlsl,
                false,
                true);

            // VAO/VBO 생성 및 설정
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(
                BufferTarget.ArrayBuffer,
                _quadVertices.Length * BackgroundVertex.SizeInBytes,
                _quadVertices,
                BufferUsageHint.StaticDraw);

            int stride = BackgroundVertex.Stride;

            // Position attribute
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);

            // TexCoord attribute
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, Vector2.SizeInBytes);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            // 텍스처 로드
            string texturePath = Path.Combine(AppContext.BaseDirectory, "Shaders", "infoget.png");
            LoadTexture(texturePath);

            // 셰이더 초기화
            if (PassShader != null)
            {
                PassShader.Use();
                PassShader.SetUniformIfExist("backgroundTexture", 0);
                GL.UseProgram(0);
            }
        }

        private void LoadTexture(string texturePath)
        {
            if (!File.Exists(texturePath))
            {
                throw new FileNotFoundException("TextureBuffer image not found", texturePath);
            }

            using SixLabors.ImageSharp.Image<Rgba32> image = Image.Load<Rgba32>(texturePath);
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            byte[] pixelArray = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixelArray.AsSpan());

            unsafe
            {
                fixed (byte* ptr = pixelArray)
                {
                    _backgroundTexture = new TextureBuffer(
                        image.Width, image.Height,
                        PixelInternalFormat.Rgba,
                        PixelFormat.Rgba,
                        PixelType.UnsignedByte,
                        (nint)ptr,
                        TextureMinFilter.LinearMipmapLinear,
                        TextureMagFilter.Linear,
                        TextureWrapMode.ClampToEdge,
                        TextureWrapMode.ClampToEdge);
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, _backgroundTexture.Handle);
            GL.GenerateMipmap((GenerateMipmapTarget)TextureTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Execute()
        {
            if (!Enabled || Camera == null || PassShader == null || _backgroundTexture == null)
                return;

            using (PassShader.Use())
            {
                PassShader.SetUniformIfExist("iResolution", new Vector2(Camera.ViewportWidth, Camera.ViewportHeight));
                
                _backgroundTexture.Bind(TextureUnit.Texture0);
                GL.BindVertexArray(_vao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
                GL.BindVertexArray(0);
                _backgroundTexture.Unbind();
            }
        }

        public override void Dispose()
        {
            if (_disposed) return;

            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            _backgroundTexture?.Dispose();

            base.Dispose();
        }
    }
}