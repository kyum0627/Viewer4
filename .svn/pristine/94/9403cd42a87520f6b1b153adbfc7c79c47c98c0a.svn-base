using OpenTK.Graphics.OpenGL4;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL Element Buffer Object (인덱스 버퍼) 관리 클래스
    /// 선택적 CPU 미러링을 지원하여 CPU-GPU 동기화 제공
    /// 
    /// 스레딩 정책:
    /// - _lock을 사용하여 CPU 데이터 접근 동기화
    /// - GPU 업데이트는 호출자가 GL 컨텍스트 스레드에서 실행 보장
    /// </summary>
    public class ElementBuffer : GLBuffer<uint>
    {
        #region Fields
        
        /// <summary>
        /// CPU 데이터 접근 동기화용 락
        /// </summary>
        private readonly object _lock = new();
        
        /// <summary>
        /// CPU 측 인덱스 데이터 미러 (keepCpuData=true일 때만)
        /// ArrayPool에서 임대하여 메모리 효율성 확보
        /// </summary>
        private uint[]? _cpuData;
        
        /// <summary>
        /// CPU 데이터 버퍼 용량
        /// </summary>
        private int _cpuDataCapacity;
        
        /// <summary>
        /// CPU 복사본 유지 여부
        /// </summary>
        private readonly bool _keepCpuCopy;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// CPU 복사본 유지 여부
        /// </summary>
        public bool KeepCpuCopy => _keepCpuCopy;
        
        /// <summary>
        /// CPU 측 인덱스 데이터 (읽기 전용)
        /// keepCpuData=false면 빈 Span 반환
        /// </summary>
        public Span<uint> CpuData
        {
            get
            {
                lock (_lock)
                {
                    return _keepCpuCopy && _cpuData != null
                        ? _cpuData.AsSpan(0, Count)
                        : Span<uint>.Empty;
                }
            }
        }
        
        /// <summary>
        /// DrawElements에서 사용할 인덱스 타입
        /// 현재는 UnsignedInt만 지원
        /// </summary>
        public DrawElementsType GetDrawElementsType => DrawElementsType.UnsignedInt;
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Element Buffer 생성
        /// </summary>
        /// <param name="data">초기 인덱스 데이터</param>
        /// <param name="usageHint">버퍼 사용 힌트</param>
        /// <param name="keepCpuData">CPU 복사본 유지 여부</param>
        public ElementBuffer(ReadOnlySpan<uint> data, BufferUsageHint usageHint, bool keepCpuData) 
            : base(BufferTarget.ElementArrayBuffer, usageHint)
        {
            if (data.IsEmpty)
                throw new ArgumentException("인덱스 데이터가 비어있습니다.", nameof(data));
            
            _keepCpuCopy = keepCpuData;
            
            if (_keepCpuCopy)
            {
                _cpuDataCapacity = data.Length;
                _cpuData = ArrayPool<uint>.Shared.Rent(_cpuDataCapacity);
                data.CopyTo(_cpuData.AsSpan());
            }
            
            SyncToGpuAll(data);
            Count = data.Length;
            
            System.Diagnostics.Debug.WriteLine(
                $"[ElementBuffer] 생성 완료: {Count}개 인덱스, CPU 미러={keepCpuData}");
        }
        
        #endregion

        #region Update Methods
        
        /// <summary>
        /// 인덱스 범위 업데이트
        /// CPU 미러와 GPU 버퍼를 모두 동기화
        /// </summary>
        /// <param name="startIndex">시작 인덱스</param>
        /// <param name="newIndices">새 인덱스 데이터</param>
        public void UpdateIndices(int startIndex, ReadOnlySpan<uint> newIndices)
        {
            if (newIndices.IsEmpty) return;
            
            // 범위 검증 (표준화된 유틸리티 사용)
            BufferValidation.ValidateUpdateRange(startIndex, newIndices.Length, Count, nameof(startIndex));
            
            // GL 컨텍스트 검증
            BufferValidation.EnsureGLContextActive("인덱스 업데이트");

            // CPU 미러 업데이트
            if (_keepCpuCopy && _cpuData != null)
            {
                lock (_lock)
                {
                    newIndices.CopyTo(_cpuData.AsSpan(startIndex, newIndices.Length));
                }
            }
            
            // GPU 업데이트
            nint byteOffset = (nint)startIndex * Unsafe.SizeOf<uint>();
            SyncToGpuSub(newIndices, byteOffset);
            
            System.Diagnostics.Debug.WriteLine(
                $"[ElementBuffer] 인덱스 업데이트: [{startIndex}, {startIndex + newIndices.Length})");
        }
        
        #endregion

        #region Draw Methods
        
        /// <summary>
        /// DrawElements 호출
        /// 일반적으로 Geometry 클래스나 DrawStrategy에서 호출
        /// </summary>
        /// <param name="primitiveType">프리미티브 타입</param>
        [Obsolete("이 메서드는 DrawStrategy에서 호출해야 합니다. 직접 호출하지 마세요.", false)]
        public void Draw(PrimitiveType primitiveType)
        {
            GL.DrawElements(primitiveType, Count, GetDrawElementsType, nint.Zero);
        }
        
        #endregion

        #region Bind / Unbind
        
        /// <summary>
        /// Element Buffer 바인딩
        /// VAO가 먼저 바인딩되어 있어야 함
        /// </summary>
        public override void Bind()
        {
            // VAO 바인딩 상태 검증 (표준화된 유틸리티 사용)
            BufferValidation.EnsureVAOBound();
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
        }

        /// <summary>
        /// Element Buffer 언바인딩
        /// ElementBuffer는 VAO에 묶여있으므로 일반적으로 언바인딩하지 않음
        /// </summary>
        public override void Unbind()
        {
            // ElementBuffer는 VAO 상태의 일부이므로 명시적 언바인딩은 권장하지 않음
            // VertexBuffer와 달리 ElementBuffer는 VAO에 종속적
            if (BufferTarget != BufferTarget.ElementArrayBuffer)
            {
                base.Unbind();
            }
        }
        
        #endregion

        #region Dispose
        
        /// <summary>
        /// 리소스 해제
        /// ArrayPool에서 임대한 메모리 반환
        /// </summary>
        public override void Dispose(bool disposing)
        {
            if (disposing && _keepCpuCopy && _cpuData != null)
            {
                lock (_lock)
                {
                    ArrayPool<uint>.Shared.Return(_cpuData, clearArray: false);
                    _cpuData = null;
                    
                    System.Diagnostics.Debug.WriteLine("[ElementBuffer] CPU 미러 메모리 반환 완료");
                }
            }
            
            base.Dispose(disposing);
        }
        
        #endregion
    }
}