using OpenTK.Graphics.OpenGL4;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 기하 데이터 기본 인터페이스
    /// 렌더링에 필요한 최소 정보 제공 (렌더링 실행은 하지 않음)
    /// </summary>
    public interface IGeometry : IDisposable
    {
        /// <summary>
        /// VAO 바인딩
        /// </summary>
        void Bind();
        
        /// <summary>
        /// VAO 언바인딩
        /// </summary>
        void Unbind();
        
        /// <summary>
        /// 프리미티브 타입 (Triangles, Lines 등)
        /// </summary>
        PrimitiveType PrimitiveType { get; set; }
        
        /// <summary>
        /// 유효성 여부 (버퍼가 올바르게 생성되었는지)
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// 정점 수 (Arrays 방식용)
        /// </summary>
        int VertexCount { get; }
    }

    /// <summary>
    /// 인덱스 기반 기하 데이터
    /// DrawElements 계열 렌더링용
    /// </summary>
    public interface IIndexedGeometry : IGeometry
    {
        /// <summary>
        /// 인덱스 수
        /// </summary>
        int IndexCount { get; }
        
        /// <summary>
        /// 인덱스 타입 (UnsignedInt, UnsignedShort 등)
        /// </summary>
        DrawElementsType IndexType { get; }
    }

    /// <summary>
    /// 인스턴싱 기반 기하 데이터
    /// DrawElementsInstanced 계열 렌더링용
    /// </summary>
    public interface IInstancedGeometry : IIndexedGeometry
    {
        /// <summary>
        /// 인스턴스 수
        /// </summary>
        int InstanceCount { get; }
    }

    /// <summary>
    /// 간접 렌더링 기하 데이터
    /// DrawElementsIndirect 계열 렌더링용
    /// </summary>
    public interface IIndirectGeometry : IGeometry
    {
        /// <summary>
        /// 간접 명령 버퍼 바인딩
        /// </summary>
        void BindCommandBuffer();
        
        /// <summary>
        /// 드로우 명령 수
        /// </summary>
        int CommandCount { get; }
    }
}
