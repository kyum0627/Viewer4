using IGX.Geometry.Common;
using IGX.ViewControl.GLDataStructure;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL Instance Buffer 관리 클래스
    /// 동일한 메시를 여러 개 인스턴싱하여 렌더링할 때 사용
    /// 
    /// 특징:
    /// - VAO에 "divisor" 값이 설정됨: 1이면 인스턴스당 한 번 읽힘
    /// - 하나의 메시가 N개의 위치/모델 행렬로 렌더링 가능
    /// - VertexAttribute를 통한 자동 attribute 설정
    /// 
    /// 스레딩 정책:
    /// - _attributeLock으로 attribute 캐시 동기화
    /// - CPU 데이터 접근은 MutableBuffer의 락 사용
    /// - GPU 업데이트는 호출자가 GL 컨텍스트 스레드에서 실행 보장
    /// </summary>
    public class InstanceBuffer<T> : MutableBuffer<T> where T : unmanaged
    {
        #region Static Fields
        
        /// <summary>
        /// 타입별 VertexAttribute 메타데이터 캐시
        /// </summary>
        private static readonly Dictionary<Type, VertexAttributeDesc[]> _cachedAttributes = new();
        
        /// <summary>
        /// Attribute 캐시 동기화용 락
        /// </summary>
        private static readonly object _attributeLock = new();
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Instance Buffer 생성
        /// </summary>
        /// <param name="data">초기 인스턴스 데이터</param>
        /// <param name="usage">버퍼 사용 힌트</param>
        /// <param name="keepCpuData">CPU 복사본 유지 여부 (기본: true)</param>
        public InstanceBuffer(ReadOnlySpan<T> data, BufferUsageHint usage, bool keepCpuData = true)
            : base(data.ToArray(), BufferTarget.ArrayBuffer, usage, keepCpuData)
        {
            GLUtil.EnsureContextActive();
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 생성 완료: {Count}개 인스턴스, CPU 미러={keepCpuData}");
        }
        
        #endregion

        #region Vertex Attribute Setup
        
        /// <summary>
        /// 인스턴스 attribute 설정
        /// VertexAttribute 어노테이션을 기반으로 자동 설정
        /// </summary>
        public void SetAttributes()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            GLUtil.EnsureContextActive();

            // Attribute 캐시 로드 또는 생성
            VertexAttributeDesc[]? attributes;
            lock (_attributeLock)
            {
                if (!_cachedAttributes.TryGetValue(typeof(T), out attributes))
                {
                    CacheAttributes();
                    attributes = _cachedAttributes[typeof(T)];
                }
            }

            if (attributes.Length == 0)
            {
                throw new InvalidOperationException(
                    $"타입 '{typeof(T).Name}'에 정의된 vertex attribute가 없습니다. " +
                    "필드에 VertexAttribute를 적용했는지 확인하세요.");
            }

            // 바인딩 및 attribute 설정
            Bind();
            try
            {
                int stride = Marshal.SizeOf<T>();
                foreach (var attr in attributes)
                {
                    // Matrix4 처리: 4개의 Vector4로 분리
                    if (attr.IsMatrix4)
                    {
                        SetupMatrix4Attribute(attr, stride);
                    }
                    else // 일반 attribute
                    {
                        SetupGeneralAttribute(attr, stride);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine(
                    $"[InstanceBuffer<{typeof(T).Name}>] Attribute 설정 완료: {attributes.Length}개");
            }
            finally
            {
                Unbind();
            }
        }

        /// <summary>
        /// Matrix4 attribute 설정
        /// Matrix4는 4개의 vec4로 분할하여 설정
        /// </summary>
        private void SetupMatrix4Attribute(VertexAttributeDesc attr, int stride)
        {
            for (int i = 0; i < 4; i++)
            {
                GL.EnableVertexAttribArray(attr.Location + i);

                GL.VertexAttribPointer(
                    attr.Location + i,
                    4, // Vector4의 크기
                    VertexAttribPointerType.Float,
                    false,
                    stride,
                    attr.Offset + Vector4.SizeInBytes * i // i번째 컬럼 오프셋
                );
                GL.VertexAttribDivisor(attr.Location + i, attr.Divisor);
            }
        }

        /// <summary>
        /// 일반 attribute 설정
        /// </summary>
        private void SetupGeneralAttribute(VertexAttributeDesc attr, int stride)
        {
            GL.EnableVertexAttribArray(attr.Location);

            // 타입별 설정
            if (attr.GLType == All.Int || attr.GLType == All.UnsignedInt)
            {
                // 정수형 타입
                GL.VertexAttribIPointer(
                    attr.Location,
                    attr.Size,
                    (VertexAttribIntegerType)attr.GLType,
                    stride,
                    attr.Offset
                );
            }
            else if (attr.GLType == All.Double)
            {
                // Double 타입
                GL.VertexAttribLPointer(
                    attr.Location,
                    attr.Size,
                    (VertexAttribDoubleType)attr.GLType,
                    stride,
                    attr.Offset
                );
            }
            else // Float, Normalized Integer 등
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

            // 인스턴스 Divisor 설정
            if (attr.Divisor > 0)
            {
                GL.VertexAttribDivisor(attr.Location, attr.Divisor);
            }
        }

        /// <summary>
        /// VertexAttribute 메타데이터 캐싱
        /// </summary>
        private void CacheAttributes()
        {
            lock (_attributeLock)
            {
                var type = typeof(T);
                if (_cachedAttributes.ContainsKey(type)) return;

                var fieldAttributes = new List<VertexAttributeDesc>();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    VertexAttribute? attribute = field.GetCustomAttribute<VertexAttribute>();
                    if (attribute != null)
                    {
                        var offset = (int)Marshal.OffsetOf<T>(field.Name);
                        
                        if (field.FieldType == typeof(Matrix4))
                        {
                            fieldAttributes.Add(new VertexAttributeDesc
                            {
                                FieldName = field.Name,
                                Location = attribute.Location,
                                Size = 4,
                                GLType = All.Float,
                                Normalized = false,
                                Offset = offset,
                                Divisor = 1,
                                IsMatrix4 = true
                            });
                        }
                        else
                        {
                            fieldAttributes.Add(new VertexAttributeDesc
                            {
                                FieldName = field.Name,
                                Location = attribute.Location,
                                Size = attribute.Size,
                                GLType = attribute.Type,
                                Normalized = attribute.Normalized,
                                Offset = offset,
                                Divisor = 1
                            });
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[InstanceBuffer] 경고: 필드 '{field.Name}'에 VertexAttribute가 없어 무시됩니다.");
                    }
                }

                _cachedAttributes[typeof(T)] = fieldAttributes.OrderBy(a => a.Location).ToArray();

                if (_cachedAttributes[typeof(T)].Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[InstanceBuffer] 경고: 타입 '{typeof(T).Name}'에 VertexAttribute가 적용된 필드가 없습니다.");
                }
            }
        }
        
        #endregion

        #region Instance Management
        
        /// <summary>
        /// 인스턴스 추가
        /// </summary>
        public void AddData(ReadOnlySpan<T> dataToAdd)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            BufferValidation.EnsureGLContextActive("인스턴스 추가");
            
            AppendToBuffer(dataToAdd);
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 인스턴스 추가: {dataToAdd.Length}개");
        }

        /// <summary>
        /// 특정 인덱스의 인스턴스 제거
        /// </summary>
        public void RemoveAt(int index)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            BufferValidation.ValidateIndex(index, Count, nameof(index));
            BufferValidation.EnsureGLContextActive("인스턴스 제거");

            RemoveCpuAt(index);
            SyncToGpuAll();
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 인스턴스 제거: 인덱스 {index}");
        }

        /// <summary>
        /// 단일 인스턴스 업데이트 (CPU + GPU)
        /// </summary>
        public void UpdateInstance(T instanceData, int index)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            BufferValidation.ValidateIndex(index, Count, nameof(index));
            BufferValidation.EnsureGLContextActive("인스턴스 업데이트");

            var cpuData = CpuData;
            cpuData[index] = instanceData;
            UpdateElement(index, instanceData);
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 인스턴스 업데이트: 인덱스 {index}");
        }

        /// <summary>
        /// CPU 데이터만 업데이트 (GPU 동기화 없음)
        /// </summary>
        public void UpdateSingleInstanceDataOnly(int index, T instanceData)
        {
            BufferValidation.ValidateIndex(index, Count, nameof(index));
            UpdateCpuElement(index, instanceData);
        }

        /// <summary>
        /// 인스턴스 범위 업데이트
        /// </summary>
        public void UpdateData(int startIndex, ReadOnlySpan<T> newData)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            if (newData.IsEmpty) return;
            
            BufferValidation.ValidateUpdateRange(startIndex, newData.Length, Count, nameof(startIndex));
            BufferValidation.EnsureGLContextActive("인스턴스 범위 업데이트");

            var cpuData = CpuData;
            newData.CopyTo(cpuData.Slice(startIndex));
            SyncToGpuSub(newData, (nint)startIndex * Unsafe.SizeOf<T>());
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 범위 업데이트: [{startIndex}, {startIndex + newData.Length})");
        }

        /// <summary>
        /// CPU 데이터 가져오기
        /// </summary>
        public ReadOnlySpan<T> GetCpuData()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            return CpuData;
        }

        /// <summary>
        /// 모든 CPU 데이터를 GPU로 업로드
        /// </summary>
        public void UploadAllData()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(InstanceBuffer<T>), 
                    "InstanceBuffer가 이미 해제되었습니다.");
            
            if (!KeepCpuCopy)
                throw new InvalidOperationException(
                    "CPU 데이터가 유지되지 않습니다. 생성자에서 keepCpuData=true로 설정하세요.");
            
            BufferValidation.EnsureGLContextActive("전체 데이터 업로드");

            SyncToGpuAll();
            
            System.Diagnostics.Debug.WriteLine(
                $"[InstanceBuffer<{typeof(T).Name}>] 전체 데이터 업로드: {Count}개 인스턴스");
        }
        
        #endregion
    }
}