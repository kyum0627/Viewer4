# IGX.ViewControl 클래스 역할 중복 제거 실행 계획

## ?? 발견된 중복 및 미사용 클래스

### 1. **즉시 삭제 가능** ??

#### A. SelevtionManager.cs (오타 + 미사용)
```
파일: ..\IGX.ViewControl\GLDataStructure\SelevtionManager.cs
상태: 전체 주석 처리
문제: 
- 파일명 오타 (Selevtion → Selection)
- 전체 코드 주석 처리
- 사용처 없음

조치: 파일 삭제
```

#### B. ModelRendererManager.cs (미사용 + 중복 기능)
```
파일: ..\IGX.ViewControl\ModelRendererManager.cs
역할: IDrawBuffer 컬렉션 관리
문제:
- IgxViewAPI에서 사용 안 함
- Model3dBufferManager.AllDrawBuffers로 대체됨
- 단순 래퍼 역할만 수행

조치: 파일 삭제 (Model3dBufferManager 사용)
```

---

### 2. **Mock 클래스 분리 필요** ??

```
..\IGX.ViewControl\Render\MockDrawCommandManager.cs
..\IGX.ViewControl\Render\MockBufferRenderer.cs
```

**문제:**
- 테스트용 Mock이 운영 코드에 혼재
- 테스트 프로젝트로 분리 필요

**조치:**
1. IGX.ViewControl.Tests 프로젝트 생성 (없으면)
2. Mock 클래스 이동
3. 빌드 확인

---

### 3. **역할 명확화 필요** ??

#### A. Manager 클래스들

| 클래스 | 역할 | 상태 | 조치 |
|--------|------|------|------|
| **Model3dDataManager** | 모델 데이터 관리 (CPU) | ? 유지 | 명확 |
| **Model3dBufferManager** | GPU 버퍼 관리 | ? 유지 | 명확 |
| **ModelRendererManager** | 렌더러 컬렉션 | ? 삭제 | 미사용 |
| **SelectionManager** | 선택 관리 | ? 유지 | 명확 |
| **ShaderManager** | 셰이더 캐시 | ? 유지 | 명확 |
| **DefaultContextManager** | GL 컨텍스트 | ? 유지 | 명확 |
| **DefaultViewportManager** | 뷰포트 | ? 유지 | 명확 |
| **GLResourceManager** | GL 리소스 삭제 | ? 유지 | 명확 |

**결론:** ModelRendererManager만 삭제

#### B. Helper 클래스들

| 클래스 | 역할 | 사용처 | 조치 |
|--------|------|--------|------|
| **PickHelper** | 피킹 로직 | GLView | ? 유지 |
| **HitTestHelper** | 히트 테스트 | PickHelper | ? 유지 |
| **TreeViewHelper** | 트리뷰 헬퍼 | ? | ?? 확인 필요 |
| **SceneGraphHelper** | 씬 그래프 | ? | ?? 확인 필요 |
| **CameraHelper** | 카메라 초기화 | IgxViewAPI | ? 유지 |
| **MatrixInstanceHelper** | 매트릭스 변환 | Render | ? 유지 |
| **OpenGLStd140Helper** | Std140 레이아웃 | UBO | ? 유지 |
| **DefaultMatrixCalculator** | 매트릭스 계산 | ? | ?? 확인 필요 |

**조치:** 사용처 확인 후 미사용 클래스 제거

---

## ?? 실행 계획

### Phase 1: 즉시 삭제 (안전) ?

#### 1-1. SelevtionManager.cs 삭제
```bash
# 파일 삭제
Remove: ..\IGX.ViewControl\GLDataStructure\SelevtionManager.cs

# 이유:
- 전체 주석 처리
- 사용처 없음
- 오타 있는 파일명
```

#### 1-2. ModelRendererManager.cs 삭제
```bash
# 참조 확인
Search: "ModelRendererManager"

# 참조 없으면 삭제
Remove: ..\IGX.ViewControl\ModelRendererManager.cs

# 대체:
- Model3dBufferManager.AllDrawBuffers 사용
```

---

### Phase 2: Mock 클래스 분리 (중요도: 중) ??

```bash
# 1. 테스트 프로젝트 확인
Check: IGX.ViewControl.Tests 프로젝트 존재 여부

# 2. Mock 클래스 이동
Move: 
  ..\IGX.ViewControl\Render\MockDrawCommandManager.cs
  ..\IGX.ViewControl\Render\MockBufferRenderer.cs
  
To:
  ..\IGX.ViewControl.Tests\Mocks\

# 3. namespace 변경
Before: IGX.ViewControl.Render
After: IGX.ViewControl.Tests.Mocks

# 4. 참조 업데이트
```

---

### Phase 3: Helper 사용처 확인 (중요도: 중) ??

```bash
# 확인할 Helper들
1. TreeViewHelper
2. SceneGraphHelper  
3. DefaultMatrixCalculator

# 각 파일에 대해:
- 사용처 검색
- 사용 빈도 확인
- 미사용이면 삭제 또는 Obsolete 표시
```

---

## ?? 삭제 체크리스트

### SelevtionManager.cs
- [ ] 파일 전체 주석 처리 확인
- [ ] 사용처 검색 (0건 확인)
- [ ] 파일 삭제
- [ ] 빌드 확인

### ModelRendererManager.cs
- [ ] IgxViewAPI에서 사용 안 함 확인
- [ ] 전체 프로젝트에서 참조 검색
- [ ] Model3dBufferManager.AllDrawBuffers로 대체 가능 확인
- [ ] 파일 삭제
- [ ] 빌드 확인

---

## ?? 예상 효과

### Before:
```
IGX.ViewControl/
├── SelectionManager.cs          ? 사용
├── SelevtionManager.cs          ? 미사용 (삭제)
├── Model3dBufferManager.cs      ? 사용
├── ModelRendererManager.cs      ? 미사용 (삭제)
├── MockDrawCommandManager.cs    ?? 운영 코드에 혼재
└── MockBufferRenderer.cs        ?? 운영 코드에 혼재

총 클래스: 110개
미사용 클래스: 2개 + Mock 2개
```

### After:
```
IGX.ViewControl/
├── SelectionManager.cs          ? 사용
├── Model3dBufferManager.cs      ? 사용
└── (정리된 구조)

IGX.ViewControl.Tests/Mocks/
├── MockDrawCommandManager.cs    ? 테스트 전용
└── MockBufferRenderer.cs        ? 테스트 전용

총 클래스: 106개 (-4개)
미사용 클래스: 0개
Mock 분리: 완료
```

**개선 효과:**
- ? 4개 클래스 정리 (삭제 2 + 이동 2)
- ? 코드베이스 3.6% 감소
- ? Mock과 운영 코드 명확히 분리
- ? 혼란 감소 (오타 파일 제거)

---

## ?? 상세 분석 필요 항목

### 1. GeometryBase.cs
```csharp
// 확인 필요
- 추상 클래스인가?
- 상속하는 클래스가 있는가?
- 사용처는?
```

### 2. Interfaces.cs
```csharp
// 파일 내용 확인
- 어떤 인터페이스들이 포함?
- IDrawBuffer, IGeometryBuffer 등?
- 파일 분리 필요한가?
```

### 3. SceneState.cs vs SceneParameters.cs
```csharp
// 역할 비교
SceneState (Render/)
- 씬 상태?

SceneParameters (Root)
- 씬 파라미터?

→ 역할 중복 확인 필요
```

### 4. SceneRenderer.cs vs DrawRenderer.cs
```csharp
// 역할 비교
SceneRenderer (Render/)
- 씬 전체 렌더링?

DrawRenderer (Render/)
- 개별 드로우 렌더링?

→ 계층 구조 확인 필요
```

### 5. CurveRenderer.cs vs DrawCurves.cs
```csharp
// 역할 비교
CurveRenderer (Render/)
DrawCurves (Render/)

→ 역할 중복 가능성
```

---

## ?? 작업 우선순위

| 우선순위 | 작업 | 난이도 | 영향도 | 시간 |
|---------|------|--------|--------|------|
| **P0** | SelevtionManager 삭제 | 낮음 | 낮음 | 5분 |
| **P0** | ModelRendererManager 삭제 | 낮음 | 낮음 | 10분 |
| **P1** | Mock 클래스 분리 | 중간 | 중간 | 30분 |
| **P2** | Helper 사용처 확인 | 중간 | 낮음 | 1시간 |
| **P3** | 상세 분석 항목 확인 | 높음 | 중간 | 2시간 |

---

## ? 다음 단계

1. **SelevtionManager.cs 삭제** - 즉시 실행 가능
2. **ModelRendererManager.cs 참조 확인 및 삭제**
3. **빌드 검증**
4. **Mock 클래스 분리 계획 수립**
5. **Helper 클래스 사용처 분석**

---

이 계획에 따라 단계적으로 중복을 제거합니다!
