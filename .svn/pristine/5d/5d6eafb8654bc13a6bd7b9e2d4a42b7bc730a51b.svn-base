using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// OpenGL 리소스(버퍼) 관리자
    /// 리소스 삭제를 안전하게 지연 처리하여 컨텍스트 오류 방지
    /// 
    /// 사용 이유:
    /// - OpenGL 리소스는 생성된 컨텍스트에서만 삭제 가능
    /// - 즉시 삭제 시 컨텍스트 오류 발생 가능
    /// - 큐에 모아두고 안전한 시점(프레임 종료 등)에 일괄 삭제
    /// 
    /// 스레드 안전성: ConcurrentQueue 사용으로 멀티스레드 안전
    /// </summary>
    public static class GLResourceManager
    {
        /// <summary>
        /// 삭제 대기 중인 버퍼 핸들 큐
        /// </summary>
        private static readonly ConcurrentQueue<int> _pendingDeletions = new();
        
        /// <summary>
        /// 총 등록된 삭제 요청 수 (통계용)
        /// </summary>
        private static int _totalEnqueued = 0;
        
        /// <summary>
        /// 총 삭제된 버퍼 수 (통계용)
        /// </summary>
        private static int _totalDeleted = 0;

        /// <summary>
        /// 버퍼를 삭제 대기 큐에 추가
        /// 실제 삭제는 ProcessPendingDeletions() 호출 시 수행
        /// </summary>
        /// <param name="handle">삭제할 버퍼 핸들 (0 이하는 무시)</param>
        public static void EnqueueForDeletion(int handle)
        {
            if (handle <= 0)
            {
                Debug.WriteLine($"[GLResourceManager] 잘못된 핸들 무시: {handle}");
                return;
            }
            
            _pendingDeletions.Enqueue(handle);
            Interlocked.Increment(ref _totalEnqueued);
            
#if DEBUG
            Debug.WriteLine($"[GLResourceManager] 삭제 대기 추가: 핸들={handle}, 대기 중={_pendingDeletions.Count}");
#endif
        }

        /// <summary>
        /// 대기 중인 버퍼 삭제 처리
        /// 활성 OpenGL 컨텍스트가 있을 때만 실행
        /// 프레임 종료 시점에 호출 권장
        /// </summary>
        public static void ProcessPendingDeletions()
        {
            // 컨텍스트 체크
            if (!GLUtil.IsContextActive())
            {
#if DEBUG
                Debug.WriteLine($"[GLResourceManager] 컨텍스트 비활성 - 삭제 연기 (대기: {_pendingDeletions.Count}개)");
#endif
                return;
            }

            if (_pendingDeletions.IsEmpty)
                return;

            // 큐에서 모든 핸들 가져오기
            var handles = new List<int>();
            while (_pendingDeletions.TryDequeue(out int handle))
            {
                handles.Add(handle);
            }

            if (handles.Count > 0)
            {
                try
                {
                    // 일괄 삭제
                    GL.DeleteBuffers(handles.Count, handles.ToArray());
                    GLUtil.ErrorCheck($"버퍼 삭제 ({handles.Count}개)");
                    
                    Interlocked.Add(ref _totalDeleted, handles.Count);
                    
#if DEBUG
                    Debug.WriteLine($"[GLResourceManager] 버퍼 삭제 완료: {handles.Count}개 " +
                                  $"(총 등록: {_totalEnqueued}, 총 삭제: {_totalDeleted})");
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[GLResourceManager] 삭제 실패: {ex.Message}");
                    
                    // 실패한 핸들 재등록 (다음 프레임에 재시도)
                    foreach (var handle in handles)
                    {
                        _pendingDeletions.Enqueue(handle);
                    }
                }
            }
        }

        /// <summary>
        /// 컨텍스트 활성 여부 확인
        /// </summary>
        private static bool IsContextActive()
        {
            try
            {
                // GL.GetError() 호출이 예외 없이 성공하면 컨텍스트 활성
                _ = GL.GetError();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 통계 정보 반환
        /// </summary>
        /// <returns>(대기 중, 총 등록, 총 삭제)</returns>
        public static (int Pending, int TotalEnqueued, int TotalDeleted) GetStatistics()
        {
            return (_pendingDeletions.Count, _totalEnqueued, _totalDeleted);
        }

        /// <summary>
        /// 통계 정보 출력 (디버깅용)
        /// </summary>
        [Conditional("DEBUG")]
        public static void PrintStatistics()
        {
            var (pending, enqueued, deleted) = GetStatistics();
            Debug.WriteLine($"[GLResourceManager 통계]");
            Debug.WriteLine($"  - 대기 중: {pending}개");
            Debug.WriteLine($"  - 총 등록: {enqueued}개");
            Debug.WriteLine($"  - 총 삭제: {deleted}개");
            Debug.WriteLine($"  - 누적률: {(deleted * 100.0 / Math.Max(1, enqueued)):F1}%");
        }
    }
}