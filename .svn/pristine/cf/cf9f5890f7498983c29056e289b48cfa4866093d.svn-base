using IGX.ViewControl.Render;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace IGX.ViewControl.Buffer
{
    public interface ITexture : IDisposable
    {
        int Handle { get; }
        int Width { get; }
        int Height { get; }
        bool IsValid { get; }
        void Bind(TextureUnit unit = TextureUnit.Texture0);
        void Unbind();
    }
    public sealed class TextureBuffer : ITexture, IDisposable
    {
        public int Handle { get; private set; }
        public TextureTarget Target { get; private set; } = TextureTarget.Texture2D;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public PixelInternalFormat InternalFormat { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        public bool IsValid => Handle != 0 && !IsDisposed;

        private static readonly ConcurrentQueue<int> _pendingDeletionHandles = new();
        public TextureBuffer(
            int width, int height,
            PixelInternalFormat internalFormat,
            PixelFormat pixelFormat,
            PixelType pixelType,
            nint data = default,
            TextureMinFilter minFilter = TextureMinFilter.Nearest,
            TextureMagFilter magFilter = TextureMagFilter.Nearest,
            TextureWrapMode wrapS = TextureWrapMode.ClampToEdge,
            TextureWrapMode wrapT = TextureWrapMode.ClampToEdge)
        {
            GLUtil.EnsureContextActive();
            if (width <= 0 || height <= 0) throw new ArgumentException("텍스처의 너비와 높이는 0보다 커야 합니다.");

            Width = width;
            Height = height;
            InternalFormat = internalFormat;
            Target = TextureTarget.Texture2D;

            Handle = GL.GenTexture();
            if (Handle == 0) throw new InvalidOperationException("OpenGL 텍스처 핸들을 생성하는 데 실패했습니다. OpenGL 컨텍스트가 유효한지 확인하세요.");

            Bind();

            GL.TexImage2D(Target, 0, internalFormat, width, height, 0, pixelFormat, pixelType, data);

            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)wrapS);
            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)wrapT);

            Unbind();
            GLUtil.ErrorCheck();
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0, Shader? shader = null, string? uniformName = null)
        {
            if (IsDisposed || Handle == 0)
            {
                Debug.WriteLine("경고: Dispose되었거나 유효하지 않은 Texture2D를 사용하려고 시도.");
                return;
            }

            GL.ActiveTexture(unit);
            GLUtil.ErrorCheck();

            Bind();

            if (shader != null && !string.IsNullOrEmpty(uniformName))
            {
                int unitIndex = (int)unit - (int)TextureUnit.Texture0;
                shader.SetUniformIfExist(uniformName, unitIndex);
                
            }
            GLUtil.ErrorCheck();
        }

        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            if (IsDisposed || Handle == 0) return;

            GL.ActiveTexture(unit);
            GL.BindTexture(Target, Handle);
        }

        public void Unbind()
        {
            GL.BindTexture(Target, 0);
        }

        public void AttachToFramebuffer(FramebufferAttachment attachment, int level = 0)
        {
            if (IsDisposed || Handle == 0)
            {
                Debug.WriteLine("경고: Dispose되었거나 유효하지 않은 Texture2D를 프레임버퍼에 연결하려고 시도.");
                return;
            }
            GLUtil.EnsureContextActive();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, Target, Handle, level);
            GLUtil.ErrorCheck();
        }

        public void Dispose()
        {
            if (IsDisposed) return;

            if (Handle != 0)
                _pendingDeletionHandles.Enqueue(Handle);

            Handle = 0;
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        public static void ProcessPendingDeletions()
        {
            while (_pendingDeletionHandles.TryDequeue(out int handleToDelete))
            {
                if (GL.IsTexture(handleToDelete))
                {
                    GL.DeleteTexture(handleToDelete);
                    Debug.WriteLine($"TextureBuffer {handleToDelete} deleted.");
                }
            }
            GLUtil.ErrorCheck();
        }
    }
}