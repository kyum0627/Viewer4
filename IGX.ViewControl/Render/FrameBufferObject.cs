using IGX.ViewControl.Buffer;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
namespace IGX.ViewControl.Render
{
    public sealed class FrameBufferObject : IDisposable
    {
        private static readonly ConcurrentQueue<int> _pendingDeletionFBOs = new();
        private int FboHandle { get; set; }
        public int Handle => FboHandle;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TextureBuffer? Position { get; private set; }
        public TextureBuffer? Normal { get; private set; }
        public TextureBuffer? Color { get; private set; }
        public TextureBuffer? ObjectID { get; private set; }
        public TextureBuffer? DepthStencil { get; private set; }
        public TextureBuffer? OverlapAccumXray { get; private set; }

        private bool _isDisposed;
        private bool _currentIncludeXrayState;
        private bool _includeXray;
        public bool IsValid => FboHandle != 0 && !_isDisposed;

        public FrameBufferObject(int width, int height, bool includeXray = false)
        {
            Resize(width, height, includeXray);
        }

        public void Resize(int width, int height) => Resize(width, height, _includeXray);
        public void Resize(int width, int height, bool includeXray = false)
        {
            GLUtil.EnsureContextActive(); // GL 호출 전에 컨텍스트 활성화 보장

            if (_isDisposed) throw new ObjectDisposedException(nameof(FrameBufferObject));
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException($"{nameof(width)} or {nameof(height)} must be positive.");

            // 크기, Xray 상태가 같으면 크기 조정 건너뛰기
            if (FboHandle != 0 && Position?.Width == width && Position?.Height == height && _currentIncludeXrayState == includeXray) return;

            int newFboHandle = 0;
            TextureBuffer? newPosition = null, newNormal = null, newColor = null, newObjectID = null, newDepthStencil = null, newOverlapAccumXray = null;

            try
            {
                // 1. 기존 리소스 해제 (큐에 넣음)
                ClearAndEnqueueOldFBO();

                // 2. 새 FBO 및 텍스처 생성
                Width = width;
                Height = height;
                _includeXray = includeXray;

                newFboHandle = GL.GenFramebuffer();
                GLUtil.ErrorCheck();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, newFboHandle);
                GLUtil.ErrorCheck();

                // G-Buffer 텍스처 생성 및 연결
                newPosition = CreateAndAttachTexture(width, height, FramebufferAttachment.ColorAttachment0, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float);
                newNormal = CreateAndAttachTexture(width, height, FramebufferAttachment.ColorAttachment1, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float);
                newColor = CreateAndAttachTexture(width, height, FramebufferAttachment.ColorAttachment2, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float);
                newObjectID = CreateAndAttachTexture(width, height, FramebufferAttachment.ColorAttachment3, PixelInternalFormat.R32i, PixelFormat.RedInteger, PixelType.Int);

                // 깊이/스텐실 버퍼
                newDepthStencil = CreateAndAttachTexture(width, height,
                    FramebufferAttachment.DepthStencilAttachment,
                    PixelInternalFormat.Depth24Stencil8,
                    PixelFormat.DepthStencil,
                    PixelType.UnsignedInt248,
                    TextureMinFilter.Nearest,
                    TextureMagFilter.Nearest);

                var drawBuffers = new List<DrawBuffersEnum>
                {
                    DrawBuffersEnum.ColorAttachment0,
                    DrawBuffersEnum.ColorAttachment1,
                    DrawBuffersEnum.ColorAttachment2,
                    DrawBuffersEnum.ColorAttachment3
                };

                if (includeXray)
                {
                    // Xray/Accumulation 버퍼는 다음 Color Attachment 4에 할당
                    newOverlapAccumXray = CreateAndAttachTexture(width, height, FramebufferAttachment.ColorAttachment4, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float);
                    drawBuffers.Add(DrawBuffersEnum.ColorAttachment4);
                }

                GL.DrawBuffers(drawBuffers.Count, drawBuffers.ToArray());

                // FBO 상태 확인
                var fboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (fboStatus != FramebufferErrorCode.FramebufferComplete)
                    throw new InvalidOperationException($"Framebuffer creation failed with status: {fboStatus}.");

                // 3. 성공 시 새 핸들 및 텍스처를 멤버 변수에 할당
                FboHandle = newFboHandle;
                Position = newPosition;
                Normal = newNormal;
                Color = newColor;
                ObjectID = newObjectID;
                DepthStencil = newDepthStencil;
                OverlapAccumXray = newOverlapAccumXray;
                _currentIncludeXrayState = includeXray;
            }
            catch (Exception)
            {
                // 실패 시 생성 중이던 새 리소스 즉시 정리 (Dispose는 큐에 넣고 TextureBuffer.Dispose는 안전함)
                if (newFboHandle != 0) GL.DeleteFramebuffer(newFboHandle);
                newPosition?.Dispose();
                newNormal?.Dispose();
                newColor?.Dispose();
                newObjectID?.Dispose();
                newDepthStencil?.Dispose();
                newOverlapAccumXray?.Dispose();
                throw;
            }
            finally
            {
                // FBO 바인딩 해제
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GLUtil.ErrorCheck();
            }
        }
        private TextureBuffer CreateAndAttachTexture(
            int width, int height,
            FramebufferAttachment attachment,
            PixelInternalFormat internalFormat,
            PixelFormat format,
            PixelType type,
            TextureMinFilter minFilter = TextureMinFilter.Linear,
            TextureMagFilter magFilter = TextureMagFilter.Linear)
        {
            var texture = new TextureBuffer(width, height, internalFormat, format, type, 0, minFilter, magFilter);
            texture.AttachToFramebuffer(attachment);
            return texture;
        }
        public void BindForWriting()
        {
            GLUtil.EnsureContextActive();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, Handle);
        }
        public void BindForReading(int textureUnit = 0)
        {
            Position?.Use(TextureUnit.Texture0);
            Normal?.Use(TextureUnit.Texture1);
            Color?.Use(TextureUnit.Texture2);
            ObjectID?.Use(TextureUnit.Texture3);
            DepthStencil?.Use(TextureUnit.Texture4);
            OverlapAccumXray?.Use(TextureUnit.Texture5);
        }

        public void Bind()
        {
            GLUtil.EnsureContextActive();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboHandle);
        }
        public void Unbind()
        {
            GLUtil.EnsureContextActive();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void ClearAndEnqueueOldFBO()
        {
            if (FboHandle != 0)
            {
                _pendingDeletionFBOs.Enqueue(FboHandle);
                FboHandle = 0;
            }
            DisposeTextures();

            Position = null;
            Normal = null;
            Color = null;
            ObjectID = null;
            DepthStencil = null;
            OverlapAccumXray = null;
        }

        public void Dispose()
        {

            if (_isDisposed)
            {
                return;
            }

            ClearAndEnqueueOldFBO();
            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
        private void DisposeTextures()
        {
            Position?.Dispose();
            Normal?.Dispose();
            Color?.Dispose();
            DepthStencil?.Dispose();
            ObjectID?.Dispose();
            OverlapAccumXray?.Dispose();
        }

        public static void ProcessPendingDeletions()
        {
            while (_pendingDeletionFBOs.TryDequeue(out int fboHandle))
            {
                if (fboHandle != 0) // 0은 유효한 FBO 핸들이 아님
                {
                    try
                    {
                        GL.DeleteFramebuffer(fboHandle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to delete FBO {fboHandle}: {ex.Message}");
                    }
                }
            }
        }
        private static bool IsContextLikelyActive()
        {
            try
            {
                GL.GetError();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}