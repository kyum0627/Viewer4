# GL 관련 클래스 개선 계획

## ?? 현재 상태 분석

### 1. GLResourceManager (현재 파일)

**문제점:**
- ? 주석 없음
- ? 스레드 안전성 불명확
- ? 컨텍스트 체크 방식이 비효율적 (예외 처리 사용)
- ? 삭제 큐가 무한정 증가 가능
- ? 디버그 로깅 없음

**개선 필요사항:**
1. 한글 주석 추가
2. 스레드 안전성 명시
3. 컨텍스트 체크 개선 (GLUtil.IsContextActive 사용)
4. 삭제 실패 처리 추가
5. 디버그 로깅 추가
6. 통계 정보 제공

---

### 2. GLStateException

**문제점:**
- ?? 주석이 너무 장황함
- ?? 오타 있음: "포하여" → "포함하여"

**개선 필요사항:**
1. 주석 간결화
2. 오타 수정

---

### 3. GLStateScope

**문제점:**
- ? 구조는 좋음
- ?? 주석 개선 필요

**개선 필요사항:**
1. 사용 예제 추가
2. 성능 고려사항 명시

---

### 4. GLStateSnapshot

**문제점:**
- ?? 주석 부족
- ?? Capture() 메서드가 너무 김
- ?? 에러 처리 없음

**개선 필요사항:**
1. 주석 보강
2. Capture() 메서드 리팩토링
3. 에러 처리 추가

---

### 5. GLUtil

**문제점:**
- ?? 주석 부족
- ?? lock 사용이 과도함 (모든 메서드)
- ?? ErrorCheck가 DEBUG 전용이어서 Release에서 오류 누락
- ?? IsContextActive() 구현이 부정확

**개선 필요사항:**
1. 한글 주석 추가
2. lock 최적화 (읽기 전용 작업은 제외)
3. ErrorCheck 전략 재고
4. IsContextActive() 개선

---

## ?? 개선 우선순위

### 1단계: GLResourceManager (High Priority) ???
- 리소스 누수 방지가 중요
- 주석, 로깅, 에러 처리 추가

### 2단계: GLStateException (Medium Priority) ??
- 간결화 및 오타 수정

### 3단계: GLStateScope (Low Priority) ?
- 주석 개선

### 4단계: GLStateSnapshot (Medium Priority) ??
- 주석 및 리팩토링

### 5단계: GLUtil (High Priority) ???
- 성능 및 안정성 개선

---

## ?? 개선 방향

### 공통 원칙:
1. **한글 주석** - 모든 public 멤버
2. **간결성** - 핵심만 설명
3. **예제** - 복잡한 경우 사용 예제
4. **에러 처리** - 명확한 예외 메시지
5. **디버그 로깅** - 문제 추적 가능
6. **성능** - 불필요한 lock 제거

### GLResourceManager 개선:
```csharp
/// <summary>
/// OpenGL 리소스(버퍼) 관리자
/// 리소스 삭제를 안전하게 지연 처리하여 컨텍스트 오류 방지
/// </summary>
public static class GLResourceManager
{
    private static readonly ConcurrentQueue<int> _pendingDeletions = new();
    private static int _totalEnqueued = 0;
    private static int _totalDeleted = 0;
    
    /// <summary>
    /// 버퍼를 삭제 대기 큐에 추가
    /// </summary>
    public static void EnqueueForDeletion(int handle)
    {
        if (handle <= 0) return;
        
        _pendingDeletions.Enqueue(handle);
        Interlocked.Increment(ref _totalEnqueued);
        
        Debug.WriteLine($"[GLResourceManager] 삭제 대기 추가: {handle}");
    }
    
    /// <summary>
    /// 대기 중인 버퍼 삭제 처리
    /// </summary>
    public static void ProcessPendingDeletions()
    {
        if (!GLUtil.IsContextActive())
        {
            Debug.WriteLine("[GLResourceManager] 컨텍스트 비활성 - 삭제 연기");
            return;
        }
        
        if (_pendingDeletions.IsEmpty) return;
        
        var handles = new List<int>();
        while (_pendingDeletions.TryDequeue(out int handle))
        {
            handles.Add(handle);
        }
        
        if (handles.Count > 0)
        {
            try
            {
                GL.DeleteBuffers(handles.Count, handles.ToArray());
                Interlocked.Add(ref _totalDeleted, handles.Count);
                
                Debug.WriteLine($"[GLResourceManager] 버퍼 삭제: {handles.Count}개");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GLResourceManager] 삭제 실패: {ex.Message}");
                
                // 실패한 핸들 재등록
                foreach (var handle in handles)
                {
                    _pendingDeletions.Enqueue(handle);
                }
            }
        }
    }
    
    /// <summary>
    /// 통계 정보 반환 (대기 중, 총 등록, 총 삭제)
    /// </summary>
    public static (int Pending, int TotalEnqueued, int TotalDeleted) GetStatistics()
    {
        return (_pendingDeletions.Count, _totalEnqueued, _totalDeleted);
    }
}
```

---

## ?? 예상 효과

| 클래스 | Before | After |
|--------|--------|-------|
| **GLResourceManager** | 주석 없음, 로깅 없음 | ? 한글 주석, 로깅, 통계 |
| **GLStateException** | 장황한 주석 | ? 간결한 주석 |
| **GLStateScope** | 기본 주석 | ? 예제 포함 |
| **GLStateSnapshot** | 주석 부족 | ? 상세 주석 |
| **GLUtil** | 과도한 lock | ? 최적화된 lock |

---

## ? 체크리스트

- [ ] GLResourceManager 개선
- [ ] GLStateException 간결화
- [ ] GLStateScope 예제 추가
- [ ] GLStateSnapshot 주석 보강
- [ ] GLUtil 최적화

---

## ?? 참고사항

### RAII 패턴 (GLStateScope):
```csharp
// 자동으로 상태 저장/복원
using (new GLStateScope())
{
    // 임시로 상태 변경
    GLUtil.SetDepthTest(false);
    // ... 렌더링
} // 자동으로 이전 상태 복원
```

### 리소스 관리 패턴:
```csharp
// 버퍼 생성
int buffer = GL.GenBuffer();

// 사용...

// 삭제 (안전하게 지연 처리)
GLResourceManager.EnqueueForDeletion(buffer);

// 프레임 종료 시
GLResourceManager.ProcessPendingDeletions();
```

---

이 계획에 따라 단계별로 개선 진행!
