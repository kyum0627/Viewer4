using IGX.Geometry.Common;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL Vertex Buffer Object (정점 버퍼) 관리 클래스
    /// 정점 정보(positions, normals, texCoords 등)를 GPU로 전송
    /// 
    /// 특징:
    /// - VAO가 attribute "읽기 방식"을 정의하고 실제 데이터는 VertexBuffer가 보유
    /// - 하나의 VBO를 여러 VAO에서 공유 가능
    /// - VertexAttribute를 통한 자동 attribute 설정
    /// - CPU 미러링 항상 활성화 (keepCpuData=true)
    /// 
    /// 스레딩 정책:
    /// - _lock을 사용하여 CPU 데이터 접근 동기화
    /// - GPU 업데이트는 호출자가 GL 컨텍스트 스레드에서 실행 보장
    /// </summary>
    public class VertexBuffer<VTX> : MutableBuffer<VTX> where VTX : unmanaged
    {
        #region Static Fields & Cache
        
        /// <summary>
        /// 타입별 VertexAttribute 메타데이터 캐시
        /// 리플렉션 비용을 최소화하기 위한 정적 캐시
        /// </summary>
        private static readonly ConcurrentDictionary<Type, VertexAttributeDesc[]> _cachedAttributes = new();
        
        /// <summary>
        /// Float 계열 타입
        /// </summary>
        private static readonly HashSet<All> FloatTypes = new()
        {
            All.Float,
            All.HalfFloat,
            All.Fixed
        };

        /// <summary>
        /// 정규화 가능한 정수 타입
        /// </summary>
        private static readonly HashSet<All> NormalizedIntTypes = new()
        {
            All.UnsignedShort,
            All.UnsignedByte,
            All.Short,
            All.Byte
        };

        /// <summary>
        /// 정수 타입
        /// </summary>
        private static readonly HashSet<All> IntTypes = new()
        {
            All.Int,
            All.UnsignedInt
        };

        /// <summary>
        /// Double 타입
        /// </summary>
        private static readonly HashSet<All> DoubleTypes = new()
        {
            All.Double
        };
        
        #endregion

        #region Fields
        
        /// <summary>
        /// CPU 데이터 접근 동기화용 락
        /// </summary>
        private readonly object _lock = new();
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Vertex Buffer 생성
        /// CPU 미러링 항상 활성화 (keepCpuData=true)
        /// </summary>
        /// <param name="data">초기 정점 데이터</param>
        /// <param name="usage">버퍼 사용 힌트</param>
        public VertexBuffer(ReadOnlySpan<VTX> data, BufferUsageHint usage)
            : base(data, BufferTarget.ArrayBuffer, usage, keepCpuData: true)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[VertexBuffer<{typeof(VTX).Name}>] 생성 완료: {Count}개 정점");
        }
        
        #endregion

        #region Vertex Attribute Setup
        
        /// <summary>
        /// 정점 attribute 설정
        /// VertexAttribute 어노테이션을 기반으로 자동 설정
        /// </summary>
        public void SetAttributes()
        {
            GLUtil.EnsureContextActive();
            
            var attrs = _cachedAttributes.GetOrAdd(typeof(VTX), _ => CacheAttributes());

            Bind();
            int stride = Marshal.SizeOf<VTX>();

            foreach (VertexAttributeDesc attr in attrs)
            {
                GL.EnableVertexAttribArray(attr.Location);
                
                // Matrix4는 4개의 vec4로 분할하여 설정
                if (attr.IsMatrix4)
                {
                    SetupMatrix4Attribute(attr, stride);
                }
                // Float 또는 정규화된 정수 타입
                else if (FloatTypes.Contains(attr.GLType) ||
                         attr.Normalized && NormalizedIntTypes.Contains(attr.GLType))
                {
                    GL.VertexAttribPointer(
                        attr.Location,
                        attr.Size,
                        (VertexAttribPointerType)attr.GLType,
                        attr.Normalized,
                        stride,
                        attr.Offset
                    );
                }
                // 정수 타입
                else if (IntTypes.Contains(attr.GLType))
                {
                    GL.VertexAttribIPointer(
                        attr.Location,
                        attr.Size,
                        (VertexAttribIntegerType)attr.GLType,
                        stride,
                        attr.Offset
                    );
                }
                // Double 타입
                else if (DoubleTypes.Contains(attr.GLType))
                {
                    GL.VertexAttribLPointer(
                        attr.Location,
                        attr.Size,
                        (VertexAttribDoubleType)attr.GLType,
                        stride,
                        attr.Offset
                    );
                }
                else
                {
                    throw new InvalidOperationException(
                        $"지원되지 않는 GLType {attr.GLType}입니다. 필드: '{attr.FieldName}'");
                }

                // Instancing divisor 설정 (Matrix4가 아닐 때만)
                if (!attr.IsMatrix4 && attr.Divisor > 0)
                {
                    GL.VertexAttribDivisor(attr.Location, attr.Divisor);
                }
            }
            
            Unbind();
            
            System.Diagnostics.Debug.WriteLine(
                $"[VertexBuffer<{typeof(VTX).Name}>] Attribute 설정 완료: {attrs.Length}개");
        }

        /// <summary>
        /// Matrix4 attribute 설정
        /// Matrix4는 4개의 vec4로 분할하여 설정
        /// </summary>
        private void SetupMatrix4Attribute(VertexAttributeDesc attr, int stride)
        {
            for (int i = 0; i < 4; i++)
            {
                GL.VertexAttribPointer(
                    attr.Location + i,
                    4,
                    VertexAttribPointerType.Float,
                    attr.Normalized,
                    stride,
                    attr.Offset + Vector4.SizeInBytes * i
                );
                GL.VertexAttribDivisor(attr.Location + i, attr.Divisor);
            }
        }

        /// <summary>
        /// VertexAttribute 메타데이터 캐싱
        /// 리플렉션을 사용하여 타입의 attribute 정보를 추출하고 캐시
        /// </summary>
        private VertexAttributeDesc[] CacheAttributes()
        {
            var type = typeof(VTX);
            var list = new List<VertexAttributeDesc>();
            
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var attr = field.GetCustomAttribute<VertexAttribute>();
                if (attr != null)
                {
                    list.Add(new VertexAttributeDesc
                    {
                        FieldName = field.Name,
                        Location = attr.Location,
                        Size = attr.Size,
                        GLType = attr.Type,
                        Normalized = attr.Normalized,
                        Offset = (int)Marshal.OffsetOf<VTX>(field.Name),
                        Divisor = attr.Divisor,
                        IsMatrix4 = field.FieldType == typeof(Matrix4)
                    });
                }
            }
            
            var attrs = list.OrderBy(a => a.Location).ToArray();
            
            System.Diagnostics.Debug.WriteLine(
                $"[VertexBuffer<{type.Name}>] Attribute 캐싱 완료: {attrs.Length}개");
            
            return attrs;
        }
        
        #endregion

        #region Update Methods
        
        /// <summary>
        /// 정점 범위 업데이트
        /// CPU 미러와 GPU 버퍼를 모두 동기화
        /// </summary>
        /// <param name="startIndex">시작 인덱스</param>
        /// <param name="newVertices">새 정점 데이터</param>
        public void UpdateVertices(int startIndex, ReadOnlySpan<VTX> newVertices)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(VertexBuffer<VTX>), 
                    "VertexBuffer가 이미 해제되었습니다.");
            
            if (newVertices.IsEmpty) return;
            
            // 범위 검증 (표준화된 유틸리티 사용)
            BufferValidation.ValidateUpdateRange(startIndex, newVertices.Length, Count, nameof(startIndex));
            
            // CPU 미러 업데이트
            lock (_lock)
            {
                newVertices.CopyTo(_cpuData.AsSpan(startIndex));
            }
            
            // GPU 업데이트
            SyncToGpuSub(newVertices, startIndex * Unsafe.SizeOf<VTX>());
            
            System.Diagnostics.Debug.WriteLine(
                $"[VertexBuffer<{typeof(VTX).Name}>] 정점 업데이트: [{startIndex}, {startIndex + newVertices.Length})");
        }
        
        #endregion
    }
}