using IGX.ViewControl.GLDataStructure;
using IGX.ViewControl.Render;
using IGX.ViewControl.Render.Strategies;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 인스턴스드 렌더링을 위한 지오메트리 버퍼 관리 클래스
    /// 기본 지오메트리(정점, 인덱스)와 인스턴스 데이터(변환 행렬 등)를 분리 관리
    /// </summary>
    /// <typeparam name="VTX">정점 데이터 타입 (unmanaged)</typeparam>
    /// <typeparam name="IDX">인덱스 데이터 타입 (unmanaged)</typeparam>
    /// <typeparam name="NST">인스턴스 데이터 타입 (unmanaged)</typeparam>
    public class DrawInstanceGeometry<VTX, IDX, NST> : IDisposable
        where VTX : unmanaged
        where IDX : unmanaged
        where NST : unmanaged
    {
        #region Properties
        
        /// <summary>
        /// Vertex Array Object ID
        /// </summary>
        public int VAO => _baseGeometry.VAO;
        
        /// <summary>
        /// Vertex Buffer Object ID
        /// </summary>
        public int VBO => _baseGeometry.VBO;
        
        /// <summary>
        /// Index Buffer Object ID
        /// </summary>
        public int IBO => _baseGeometry.IBO;
        
        /// <summary>
        /// 정점 개수
        /// </summary>
        public int VertexCount => _baseGeometry.VertexCount;
        
        /// <summary>
        /// 인덱스 개수
        /// </summary>
        public int IndexCount => _baseGeometry.IndexCount;
        
        /// <summary>
        /// 드로우 커맨드 개수
        /// </summary>
        public int CommandCount => _baseGeometry.CommandCount;
        
        /// <summary>
        /// 인스턴스 개수
        /// </summary>
        public int InstanceCount => _instanceBuffer.Count;
        
        /// <summary>
        /// 인덱스 요소 타입
        /// </summary>
        public DrawElementsType ElementType => _baseGeometry.ElementType;
        
        /// <summary>
        /// GPU 메모리 사용량 (바이트)
        /// </summary>
        public int GpuMemoryBytes => 
            (_baseGeometry.VertexCount * Unsafe.SizeOf<VTX>()) +
            (_baseGeometry.IndexCount * sizeof(uint)) +
            (_instanceBuffer.Count * Unsafe.SizeOf<NST>());

        #endregion

        #region Fields

        private readonly DrawElementGeometry<VTX> _baseGeometry;
        private readonly InstanceBuffer<NST> _instanceBuffer;
        private readonly object _lock = new();
        private bool _isDisposed = false;
        private IDrawBuffer _renderer;

        #endregion

        #region Events

        /// <summary>
        /// 인스턴스 데이터 변경 시 발생하는 이벤트
        /// </summary>
        public event EventHandler<InstanceChangedEventArgs>? InstanceChanged;

        /// <summary>
        /// 버퍼 업데이트 시 발생하는 이벤트
        /// </summary>
        public event EventHandler? BufferUpdated;

        #endregion

        #region Constructor

        /// <summary>
        /// 인스턴스드 지오메트리 버퍼 생성
        /// instances가 비어있으면 기본 인스턴스 1개 자동 생성
        /// </summary>
        public DrawInstanceGeometry(
            ReadOnlySpan<VTX> vertices,
            ReadOnlySpan<uint> indices,
            ReadOnlySpan<NST> instances,
            Shader? defaultShader = null,
            BufferUsageHint vertexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint indexHint = BufferUsageHint.StaticDraw,
            BufferUsageHint instanceHint = BufferUsageHint.DynamicDraw)
        {
            GLUtil.EnsureContextActive();
            
            if (vertices.IsEmpty)
                throw new ArgumentException("정점 데이터가 비어있습니다.", nameof(vertices));
            if (indices.IsEmpty)
                throw new ArgumentException("인덱스 데이터가 비어있습니다.", nameof(indices));
            
            try
            {
                _baseGeometry = new DrawElementGeometry<VTX>(vertices, indices);
                
                // instances가 비어있으면 팩토리로 기본 인스턴스 생성
                ReadOnlySpan<NST> actualInstances = instances;
                if (instances.IsEmpty)
                {
                    var defaultInstance = InstanceFactory.CreateDefault<NST>();
                    actualInstances = new ReadOnlySpan<NST>(new[] { defaultInstance });
                    Debug.WriteLine($"[DrawInstanceGeometry] 인스턴스 없음 - 기본 인스턴스 1개 생성");
                }
                
                _instanceBuffer = new InstanceBuffer<NST>(actualInstances.ToArray(), instanceHint, keepCpuData: true);
                
                _baseGeometry.Bind();
                _instanceBuffer.SetAttributes();
                _baseGeometry.Unbind();
                
                InitializeRenderer(defaultShader);
                
                Debug.WriteLine($"[DrawInstanceGeometry] 생성 완료: {VertexCount}개 정점, {IndexCount}개 인덱스, {InstanceCount}개 인스턴스");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DrawInstanceGeometry] 초기화 실패: {ex.Message}");
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 렌더러 초기화
        /// </summary>
        private void InitializeRenderer(Shader? defaultShader)
        {
            // 새로운 통합 렌더러 사용
            var strategy = new InstanceDrawStrategy<VTX, IDX, NST>(this);
            _renderer = new DrawRenderer(strategy, defaultShader);
            
            Debug.WriteLine($"[DrawInstanceGeometry] 렌더러 초기화 완료: DrawRenderer with InstanceDrawStrategy");
        }

        #endregion

        #region Renderer Property

        /// <summary>
        /// 렌더링 실행 객체
        /// </summary>
        public IDrawBuffer Renderer
        {
            get
            {
                EnsureNotDisposed();
                return _renderer;
            }
            set
            {
                EnsureNotDisposed();
                _renderer = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        #endregion

        #region Drawing

        /// <summary>
        /// 지오메트리 렌더링
        /// </summary>
        public void Draw(PrimitiveType primitiveType = PrimitiveType.Triangles)
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            Renderer.PrimType = primitiveType;
            Renderer.Execute();
        }

        #endregion

        #region Instance Management

        /// <summary>
        /// 인스턴스 추가
        /// </summary>
        public void AddInstance(NST instance)
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                Bind();
                _instanceBuffer.AddData(new ReadOnlySpan<NST>(new[] { instance }));
                Unbind();
                
                OnInstanceChanged(new InstanceChangedEventArgs(InstanceCount - 1, InstanceChangeType.Added));
            }
        }

        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        public void RemoveInstance(int index)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= InstanceCount)
            {
                Debug.WriteLine($"[DrawInstanceGeometry] 인스턴스 제거 실패: 잘못된 인덱스 {index}");
                return;
            }
            
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                Bind();
                _instanceBuffer.RemoveAt(index);
                Unbind();
                
                OnInstanceChanged(new InstanceChangedEventArgs(index, InstanceChangeType.Removed));
            }
        }

        /// <summary>
        /// 단일 인스턴스 업데이트
        /// </summary>
        public void UpdateSingleInstance(int index, NST instance)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= InstanceCount)
                throw new ArgumentOutOfRangeException(nameof(index), 
                    $"인덱스 {index}가 범위 [0, {InstanceCount})를 벗어났습니다.");
            
            UpdateInstanceRange(new NST[] { instance }.AsSpan(), index);
            
            OnInstanceChanged(new InstanceChangedEventArgs(index, InstanceChangeType.Modified));
        }

        /// <summary>
        /// 인스턴스 범위 업데이트
        /// </summary>
        public void UpdateInstanceRange(ReadOnlySpan<NST> data, int offset = 0)
        {
            if (data.IsEmpty) return;
            
            if (offset < 0 || offset + data.Length > InstanceCount)
                throw new ArgumentOutOfRangeException(nameof(offset), 
                    $"범위 [{offset}, {offset + data.Length})가 인스턴스 개수 {InstanceCount}를 초과합니다.");
            
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                _instanceBuffer.SyncToGpuSub(data, (nint)(offset * Unsafe.SizeOf<NST>()));
                OnBufferUpdated();
            }
        }

        /// <summary>
        /// 모든 인스턴스 업데이트
        /// </summary>
        public void UpdateInstances(ReadOnlySpan<NST> data)
        {
            if (data.IsEmpty) return;
            
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                Bind();
                _instanceBuffer.SyncToGpuAll(data);
                Unbind();
                
                OnBufferUpdated();
            }
        }

        /// <summary>
        /// 인스턴스 변환 행렬 업데이트 (MeshInstanceGL 전용)
        /// </summary>
        public void UpdateInstanceTransform(int index, Matrix4 transform)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= InstanceCount)
                throw new ArgumentOutOfRangeException(nameof(index), 
                    $"인스턴스 인덱스 {index}가 범위 [0, {InstanceCount})를 벗어났습니다.");

            NST instanceData = GetLocalInstanceData(index);

            if (instanceData is MeshInstanceGL meshInstance)
            {
                meshInstance.Model = transform;
                UpdateSingleInstance(index, (NST)(object)meshInstance);
            }
            else
            {
                throw new InvalidOperationException(
                    $"NST 타입 '{typeof(NST).Name}'은 변환 행렬 업데이트를 위해 MeshInstanceGL이어야 합니다.");
            }
        }

        /// <summary>
        /// 인스턴스 버퍼 크기 조정
        /// </summary>
        public void ResizeInstanceBuffer(int newSize)
        {
            EnsureNotDisposed();
            
            if (newSize < InstanceCount)
                throw new ArgumentException(
                    $"새 크기 {newSize}는 현재 인스턴스 개수 {InstanceCount}보다 작을 수 없습니다.", 
                    nameof(newSize));
            
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                Bind();
                _instanceBuffer.ReallocateGpuBuffer(newSize);
                Unbind();
                
                Debug.WriteLine($"[DrawInstanceGeometry] 인스턴스 버퍼 크기 조정: {newSize}");
            }
        }

        /// <summary>
        /// CPU 데이터를 GPU로 플러시
        /// </summary>
        public void FlushInstanceData()
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            lock (_lock)
            {
                Bind();
                _instanceBuffer.SyncToGpuAll(_instanceBuffer.GetCpuDataSnapshot());
                Unbind();
                
                OnBufferUpdated();
            }
        }

        /// <summary>
        /// 로컬 인스턴스 데이터 가져오기 (읽기 전용)
        /// </summary>
        public NST GetLocalInstanceData(int index)
        {
            EnsureNotDisposed();
            
            if (index < 0 || index >= InstanceCount)
                throw new ArgumentOutOfRangeException(nameof(index), 
                    $"인덱스 {index}가 범위 [0, {InstanceCount})를 벗어났습니다.");
            
            return _instanceBuffer.CpuData[index];
        }

        /// <summary>
        /// 필터링된 인스턴스 데이터 가져오기
        /// </summary>
        public ReadOnlySpan<NST> GetFilteredInstanceData(HashSet<int> excludeSet)
        {
            EnsureNotDisposed();
            
            var snapshot = _instanceBuffer.GetCpuDataSnapshot();
            int count = snapshot.Length - excludeSet.Count;
            
            if (count <= 0) 
                return ReadOnlySpan<NST>.Empty;
            
            NST[] result = new NST[count];
            int idx = 0;
            
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (!excludeSet.Contains(i))
                    result[idx++] = snapshot[i];
            }
            
            return result.AsSpan(0, idx);
        }

        #endregion

        #region Vertex & Index Update

        /// <summary>
        /// 단일 정점 업데이트
        /// </summary>
        public void UpdateSingleVertex(int vertexIndex, VTX newVertexData)
        {
            EnsureNotDisposed();
            _baseGeometry.UpdateSingleVertex(vertexIndex, newVertexData);
        }

        /// <summary>
        /// 정점 범위 업데이트
        /// </summary>
        public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> newVertexData)
        {
            EnsureNotDisposed();
            _baseGeometry.UpdateVertices(startIndex, newVertexData);
        }

        /// <summary>
        /// 인덱스 범위 업데이트
        /// </summary>
        public void UpdateIndices(int startIndex, ReadOnlySpan<uint> newIndices)
        {
            EnsureNotDisposed();
            _baseGeometry.UpdateIndices(startIndex, newIndices);
        }

        #endregion

        #region Bind / Unbind

        /// <summary>
        /// VAO 및 버퍼 바인딩
        /// </summary>
        public void Bind()
        {
            EnsureNotDisposed();
            GLUtil.EnsureContextActive();
            
            _baseGeometry.Bind();
            _instanceBuffer.Bind();
        }

        /// <summary>
        /// VAO 및 버퍼 언바인딩
        /// </summary>
        public void Unbind()
        {
            _instanceBuffer.Unbind();
            _baseGeometry.Unbind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 인스턴스 변경 이벤트 발생
        /// </summary>
        protected virtual void OnInstanceChanged(InstanceChangedEventArgs e)
        {
            InstanceChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 버퍼 업데이트 이벤트 발생
        /// </summary>
        protected virtual void OnBufferUpdated()
        {
            BufferUpdated?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// 리소스 해제
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            
            lock (_lock)
            {
                if (_isDisposed) return;
                
                if (disposing)
                {
                    try
                    {
                        _instanceBuffer?.Dispose();
                        _baseGeometry?.Dispose();
                        
                        Debug.WriteLine($"[DrawInstanceGeometry] 해제 완료: {VertexCount}개 정점, {IndexCount}개 인덱스");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DrawInstanceGeometry] 해제 오류: {ex.Message}");
                    }
                }
                
                _isDisposed = true;
            }
        }

        /// <summary>
        /// 리소스 해제
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 소멸자
        /// </summary>
        ~DrawInstanceGeometry()
        {
            Dispose(false);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Disposed 상태 확인
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name, "DrawInstanceGeometry가 이미 해제되었습니다.");
        }

        #endregion
    }

    #region Event Args

    /// <summary>
    /// 인스턴스 변경 이벤트 인자
    /// </summary>
    public class InstanceChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 변경된 인스턴스의 인덱스
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 변경 타입
        /// </summary>
        public InstanceChangeType ChangeType { get; }

        public InstanceChangedEventArgs(int index, InstanceChangeType changeType)
        {
            Index = index;
            ChangeType = changeType;
        }
    }

    /// <summary>
    /// 인스턴스 변경 타입
    /// </summary>
    public enum InstanceChangeType
    {
        /// <summary>
        /// 인스턴스 추가
        /// </summary>
        Added,

        /// <summary>
        /// 인스턴스 제거
        /// </summary>
        Removed,

        /// <summary>
        /// 인스턴스 수정
        /// </summary>
        Modified
    }

    #endregion
}
