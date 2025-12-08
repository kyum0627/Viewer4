//using IGX.Geometry.Curves;
//using IGX.Geometry.DataStructure;
//using IGX.ViewControl.Buffer;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;

//namespace IGX.ViewControl.Render
//{
//    public class CurveRenderer : IDisposable
//    {
//        private readonly VertexArrayObject vao;
//        private readonly VertexBuffer<Vector3> vbo;
//        private readonly Shader PassShader;

//        private bool disposedValue = false;

//        // 생성자: 렌더링에 사용할 셰이더를 받음.
//        public CurveRenderer(Shader PassShader)
//        {
//            this.PassShader = PassShader ?? throw new ArgumentNullException(nameof(PassShader));
//            vao = new VertexArrayObject(); // VAO 생성
//            vbo = new VertexBuffer<Vector3>([], BufferUsageHint.DynamicDraw, BufferTarget.InstanceBuffer); // VBO 생성 (동적 드로잉용)
//            vao.Bind(); // VAO 바인딩
//            vbo.SetAttributes(); // VBO 속성 설정 (위치 데이터만)
//            vao.Unbind(); // VAO 언바인딩
//        }

//        // 단일 곡선 객체를 렌더링
//        public void DrawElements(Matrix4 model, IMyCamera camera, Vector4 color, CurveBase curve, bool drawPoints = false)
//        {
//            PassShader.Use();
//            PassShader.SetUniformIfExist("uModel", model);
//            PassShader.SetUniformIfExist("uView", camera._viewMatrix);
//            PassShader.SetUniformIfExist("uProjection", camera._projectionMatrix);

//            // VBO에 데이터를 한 번만 업로드
//            // CurveBase.Positions는 이미 제어점을 포함한다고 가정
//            vbo.SyncToGpuSub(curve.Vertices.ToArray(), 0);
//            vao.Bind();

//            // 곡선 그리기
//            PassShader.SetUniformIfExist("uObjectColor", color);
//            GL.DrawArrays(PrimitiveType.LineStrip, 0, curve.Vertices.CommandCount);

//            if (drawPoints)
//            {
//                // 제어점 그리기
//                GL.PointSize(5.0f);
//                PassShader.SetUniformIfExist("uObjectColor", new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
//                // 제어점은 Vertices 배열의 시작 부분에 있으므로 시작 인덱스는 0, 
//                // ControlPointIDs 리스트의 개수만큼 그리면 됨
//                GL.DrawArrays(PrimitiveType.Vertices, 0, curve.ControlPointIDs.CommandCount);
//            }
//            vao.Unbind();
//        }

//        // 여러 곡선을 멀티 렌더링
//        public void MultiRender(Matrix4 model, IMyCamera camera, Vector4 curveColor, Vector4 pointColor, CurveData curveData, bool drawPoints = false)
//        {
//            PassShader.Use(); // 셰이더 사용
//            PassShader.SetUniformIfExist("uModel", model); // 모델 행렬 유니폼 설정
//            PassShader.SetUniformIfExist("uView", camera._viewMatrix); // 뷰 행렬 유니폼 설정
//            PassShader.SetUniformIfExist("uProjection", camera._projectionMatrix); // 투영 행렬 유니폼 설정

//            vbo.SyncToGpuSub([.. curveData.AllVertices], 0); // 모든 곡선 데이터를 VBO에 업로드
//            vao.Bind(); // VAO 바인딩

//            // 곡선 렌더링
//            PassShader.SetUniformIfExist("uObjectColor", curveColor); // 곡선 색상 유니폼 설정
//            curveData.GetMultiDrawArraysCommands(PrimitiveType.LineStrip, out int[] curveFirsts, out int[] curveCounts); // 곡선 드로우 명령 가져오기
//            if (curveFirsts.Length > 0)
//            {
//                GL.MultiDrawArrays(PrimitiveType.LineStrip, curveFirsts, curveCounts, curveFirsts.Length); // 여러 곡선 한 번에 그리기
//            }

//            if (drawPoints) // 제어점을 그릴 경우
//            {
//                PassShader.SetUniformIfExist("uObjectColor", pointColor); // 제어점 색상 유니폼 설정
//                GL.PointSize(5.0f); // 점 크기 설정
//                curveData.GetMultiDrawArraysCommands(PrimitiveType.Vertices, out int[] pointFirsts, out int[] pointCounts); // 제어점 드로우 명령 가져오기
//                if (pointFirsts.Length > 0)
//                {
//                    GL.MultiDrawArrays(PrimitiveType.Vertices, pointFirsts, pointCounts, pointFirsts.Length); // 여러 제어점 한 번에 그리기
//                }
//            }
//            vao.Unbind(); // VAO 언바인딩
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    vbo.Dispose();
//                    PassShader.Dispose();
//                }
//                vao.Dispose();
//                disposedValue = true;
//            }
//        }
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}