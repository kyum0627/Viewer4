using OpenTK.Graphics.OpenGL4;
using IGX.ViewControl.Render.Materials;

namespace IGX.ViewControl.Render
{

    public class Quad : IDisposable
    {
        private int quadVao, quadVbo; // 전체 화면 쿼드를 그리기 위한 VAO/VBO
        private bool isDisposed;
        public void InitializeQuad()
        {
            float[] vertices = QuadModel.Vertices();
            quadVao = GL.GenVertexArray();
            quadVbo = GL.GenBuffer();

            GL.BindVertexArray(quadVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void DrawQuad()
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(quadVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: 관리형 상태(관리형 개체)를 삭제.
                }
                if (quadVao != 0)
                {
                    GL.DeleteVertexArray(quadVao);
                    quadVao = 0;
                }
                if (quadVbo != 0)
                {
                    GL.DeleteBuffer(quadVbo);
                    quadVbo = 0;
                }

                // TODO: 비관리형 리소스(비관리형 개체)를 해제하고 종료자를 재정의.
                // TODO: 큰 필드를 null로 설정.
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
