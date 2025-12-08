# IGX.ViewControl 클래스 역할 분석 및 중복 제거 계획

## ?? 전체 클래스 구조 파악

### 1. 디렉토리별 클래스 분류 (총 110개 클래스)

#### Buffer/ (24개)
```
Core Buffers (8개):
├── GLBuffer.cs                 - 버퍼 기본 클래스
├── VertexBuffer.cs             - 정점 버퍼
├── ElementBuffer.cs            - 인덱스 버퍼
├── InstanceBuffer.cs           - 인스턴스 버퍼
├── IndirectCommandBuffer.cs    - 간접 명령 버퍼
├── ShaderStorageBuffer.cs      - SSBO
├── UniformBuffer.cs            - UBO
└── TextureBuffer.cs            - 텍스처 버퍼

Geometry (6개):
├── IGeometry.cs                - 인터페이스 (새로 추가)
├── DrawElementGeometry.cs      - 인덱스 기반
├── DrawArraysGeometry.cs       - 배열 기반
├── DrawInstanceGeometry.cs     - 인스턴싱
├── DrawIndirectGeometry.cs     - 간접 렌더링
└── DrawSSBObaseGeometry.cs     - SSBO 기반

GL State (6개):
├── GLUtil.cs                   - 상태 유틸리티
├── GLStateSnapshot.cs          - 상태 캡처
├── GLStateScope.cs             - 자동 복원
├── GLStateException.cs         - 예외
├── GLResourceManager.cs        - 리소스 관리
└── VertexArrayObject.cs        - VAO

Others (4개):
├── ArraysIndirectCommand.cs    - 간접 명령 구조체
├── IInstanceFactory.cs         - 인스턴스 팩토리
├── BufferValidation.cs         - 검증 유틸리티
├── ImmutableBuffer.cs          - 불변 버퍼
└── MutableBuffer.cs            - 가변 버퍼
```

#### Render/ (38개)
```
Core (6개):
├── Shader.cs                   - 셰이더
├── ShaderManager.cs            - 셰이더 관리
├── ShaderSource.cs             - 셰이더 소스
├── DrawRenderer.cs             - 통합 렌더러
├── DrawRendererFactory.cs      - 렌더러 팩토리
└── IDrawStrategy.cs            - 렌더링 전략

Strategies (5개):
├── ElementDrawStrategy.cs      - 인덱스 렌더링
├── ArraysDrawStrategy.cs       - 배열 렌더링
├── InstanceDrawStrategy.cs     - 인스턴싱
├── IndirectDrawStrategy.cs     - 간접 렌더링
└── SSBODrawStrategy.cs         - SSBO 렌더링

Pipeline (4개):
├── RenderPipeline.cs           - 파이프라인
├── RenderPassBase.cs           - 패스 기본
├── StandardRenderPassFactory.cs - 패스 팩토리
└── IVAOSetup.cs                - VAO 설정 인터페이스

Auxiliary Passes (4개):
├── BackgroundPass.cs           - 배경
├── CoordinatePass.cs           - 좌표축
├── ObjectBoxPass.cs            - 바운딩 박스
└── NormalVectorsPass.cs        - 법선 벡터

ClipPlane (3개):
├── ClipPlanePass.cs            - 클리핑 패스
├── ClipPlaneSystem.cs          - 클리핑 시스템
├── ClipBoxController.cs        - 클립박스 컨트롤러
└── ClipBoxGizmoRenderer.cs     - 클립박스 렌더러

Camera & Scene (5개):
├── MyCamare.cs                 - 카메라
├── CameraHelper.cs             - 카메라 헬퍼
├── SceneState.cs               - 씬 상태
├── SceneRenderer.cs            - 씬 렌더러
└── FrameBufferObject.cs        - FBO

Materials (6개):
├── Material.cs                 - 재질
├── MeshInfo.cs                 - 메시 정보
├── ClippingBox.cs              - 클리핑 박스
├── QuadModel.cs                - 쿼드 모델
├── Arrow.cs                    - 화살표
├── CommandInstance.cs          - 명령 인스턴스
└── MatrixInstanceHelper.cs     - 매트릭스 헬퍼

Models (4개):
├── MeshedModel.cs              - 메시 모델
├── MeshedModelBuilder.cs       - 모델 빌더
├── ModelQuery.cs               - 모델 쿼리
└── ModelSerializer.cs          - 모델 직렬화

Curves (3개):
├── CurveRenderer.cs            - 커브 렌더러
├── DrawCurves.cs               - 커브 그리기
├── IDrawCurve.cs               - 커브 인터페이스
└── CurveData.cs                - 커브 데이터

Others (3개):
├── ILight.cs                   - 조명 인터페이스
├── RendererConstants.cs        - 상수
├── Quad.cs                     - 쿼드
└── MockDrawCommandManager.cs   - Mock (테스트용?)
└── MockBufferRenderer.cs       - Mock (테스트용?)
```

#### GLDataStructure/ (8개)
```
├── GLVertex.cs                 - 정점 구조체
├── BasicInstance.cs            - 기본 인스턴스
├── MeshInstanceGL.cs           - 메시 인스턴스
├── SSBOInstanceElement.cs      - SSBO 인스턴스
├── VertexAttribute.cs          - 정점 속성
├── VertexAttributeDesc.cs      - 정점 속성 설명
├── VertexListComparer.cs       - 정점 비교자
├── SelectionControl3D.cs       - 선택 컨트롤
└── OpenGLStd140Helper.cs       - Std140 헬퍼
```

#### Root Level (40개)
```
Core API (3개):
├── IgxViewAPI.cs               - 핵심 API
├── GLView.cs                   - GL 뷰 컨트롤
└── GLView.Designer.cs          - 디자이너

Managers (5개):
├── Model3dDataManager.cs       - 모델 데이터 관리
├── Model3dBufferManager.cs     - 모델 버퍼 관리
├── ModelRendererManager.cs     - 모델 렌더러 관리 ??
├── SelectionManager.cs         - 선택 관리
└── SelevtionManager.cs         - 선택 관리 (오타?) ??

Passes (2개):
├── ForwardPass.cs              - Forward 패스
└── FBOPass.cs                  - FBO 패스
└── PostFBOPass.cs              - Post FBO 패스

Parameters (3개):
├── SceneParameters.cs          - 씬 파라미터
├── ShaderParameters.cs         - 셰이더 파라미터
└── AuxillaryDrawSetting.cs     - 보조 렌더링 설정

Helpers (7개):
├── PickHelper.cs               - 피킹 헬퍼
├── HitTestHelper.cs            - 히트 테스트 헬퍼
├── TreeViewHelper.cs           - 트리뷰 헬퍼
├── SceneGraphHelper.cs         - 씬 그래프 헬퍼
├── DefaultMatrixCalculator.cs  - 매트릭스 계산
├── DefaultContextManager.cs    - 컨텍스트 관리
└── DefaultViewportManager.cs   - 뷰포트 관리

Selection (3개):
├── PickedGeometry.cs           - 선택된 지오메트리
├── SelectionOptions.cs         - 선택 옵션
└── SelectionControl3D.cs       - (중복?)

Others (7개):
├── GeometryBase.cs             - 지오메트리 기본
├── LightingProvider.cs         - 조명 제공자
├── ShadeMode.cs                - 셰이딩 모드 (enum)
├── Interfaces.cs               - 인터페이스 모음
├── IEventHandler.cs            - 이벤트 핸들러
├── GLViewToolbar.cs            - 툴바
└── ...
```

---

## ? 발견된 문제점

### 1. 중복 클래스 ??????

| 클래스 1 | 클래스 2 | 위치 | 문제 |
|----------|----------|------|------|
| **SelectionManager.cs** | **SelevtionManager.cs** | Root | 오타로 인한 중복? |
| **ModelRendererManager.cs** | **Model3dBufferManager.cs** | Root | 역할 중복 가능성 |
| **SelectionControl3D.cs** | **SelectionControl3D.cs** | GLDataStructure vs Root | 중복? |
| **MockDrawCommandManager.cs** | **MockBufferRenderer.cs** | Render | 테스트용? 운영 코드? |

### 2. 역할 불명확 클래스

| 클래스 | 위치 | 문제 |
|--------|------|------|
| **GeometryBase.cs** | Root | 무엇의 기본 클래스? |
| **Interfaces.cs** | Root | 여러 인터페이스 혼재? |
| **SceneState.cs** | Render | SceneParameters와 역할 중복? |
| **SceneRenderer.cs** | Render | DrawRenderer와 역할 중복? |
| **CurveRenderer.cs** | Render | DrawCurves와 역할 중복? |

### 3. Manager 과다

```
- Model3dDataManager         (모델 데이터)
- Model3dBufferManager        (모델 버퍼)
- ModelRendererManager        (모델 렌더러) ??
- SelectionManager            (선택)
- ShaderManager               (셰이더)
- DefaultContextManager       (컨텍스트)
- DefaultViewportManager      (뷰포트)
- GLResourceManager           (GL 리소스)
```

**문제:**
- Manager 클래스가 너무 많음 (8개)
- 역할 중복 가능성
- 책임 분산 과다

### 4. Helper 과다

```
- PickHelper
- HitTestHelper
- TreeViewHelper
- SceneGraphHelper
- CameraHelper
- MatrixInstanceHelper
- OpenGLStd140Helper
- DefaultMatrixCalculator
```

**문제:**
- Helper 클래스가 너무 많음 (8개)
- 일부는 static utility로 통합 가능
- 역할 불명확

### 5. Mock 클래스 운영 코드 혼재 ??

```
Render/
├── MockDrawCommandManager.cs
└── MockBufferRenderer.cs
```

**문제:**
- 테스트용 Mock이 운영 코드에 혼재
- 테스트 프로젝트로 분리 필요

---

## ?? 우선순위 높은 문제

### Priority 1: 중복 클래스 제거 ??

#### 1. SelectionManager vs SelevtionManager
```bash
# 확인 필요
..\IGX.ViewControl\SelectionManager.cs
..\IGX.ViewControl\GLDataStructure\SelevtionManager.cs
```

**조치:**
- 오타 확인 (Selevtion → Selection)
- 하나로 통합
- 참조 업데이트

#### 2. SelectionControl3D 중복
```bash
..\IGX.ViewControl\GLDataStructure\SelectionControl3D.cs
```

**조치:**
- 단일 위치로 통합
- 역할 명확화

### Priority 2: Manager 역할 재분배 ??

#### ModelRendererManager 역할 확인
- Model3dBufferManager와 역할 중복?
- 실제 사용처 확인
- 통합 또는 제거

#### Manager 통합 검토
```
Before:
- Model3dDataManager         (데이터)
- Model3dBufferManager        (버퍼)
- ModelRendererManager        (렌더러?)

After:
- Model3dManager              (데이터 + 버퍼 통합?)
```

### Priority 3: Helper 통합 ??

#### Static Utility로 통합 가능
```
- PickHelper
- HitTestHelper
→ SelectionUtility (통합)

- TreeViewHelper
- SceneGraphHelper
→ SceneUtility (통합)

- DefaultMatrixCalculator
- MatrixInstanceHelper
→ MatrixUtility (통합)
```

### Priority 4: Mock 클래스 분리 ??

```
Before:
IGX.ViewControl/Render/
├── MockDrawCommandManager.cs
└── MockBufferRenderer.cs

After:
IGX.ViewControl.Tests/
├── MockDrawCommandManager.cs
└── MockBufferRenderer.cs
```

---

## ?? 상세 분석 필요 클래스

### 1. GeometryBase.cs
```
질문:
- 무엇의 기본 클래스?
- 사용하는 클래스가 있는가?
- 추상 클래스? 인터페이스?
```

### 2. Interfaces.cs
```
질문:
- 어떤 인터페이스들이 포함?
- 파일 분리 필요?
- IGeometryBuffer, IDrawBuffer 등 포함?
```

### 3. SceneState vs SceneParameters
```
SceneState.cs (Render/)
- 씬 상태 관리?

SceneParameters.cs (Root)
- 씬 파라미터?

→ 역할 중복 가능성 확인
```

### 4. SceneRenderer vs DrawRenderer
```
SceneRenderer.cs (Render/)
- 씬 렌더링?

DrawRenderer.cs (Render/)
- 드로우 렌더링?

→ 역할 중복? 계층 구조?
```

### 5. CurveRenderer vs DrawCurves
```
CurveRenderer.cs (Render/)
DrawCurves.cs (Render/)

→ 역할 중복 가능성
```

---

## ?? 제안 구조 (개선 후)

### Buffer/ (간소화)
```
Core/
├── GLBuffer.cs
├── VertexBuffer.cs
├── ElementBuffer.cs
└── ... (기존 버퍼들)

Geometry/
├── IGeometry.cs
├── DrawElementGeometry.cs
└── ... (기존 Geometry들)

State/
├── GLUtil.cs
├── GLStateSnapshot.cs
└── ... (GL 상태 관리)
```

### Render/ (간소화)
```
Core/
├── Shader.cs
├── ShaderManager.cs
├── DrawRenderer.cs
└── DrawRendererFactory.cs

Strategies/
├── IDrawStrategy.cs
└── ... (5개 Strategy)

Pipeline/
├── RenderPipeline.cs
└── StandardRenderPassFactory.cs

Passes/
├── ForwardPass.cs
├── FBOPass.cs
├── PostFBOPass.cs
└── Auxilliary/
    ├── BackgroundPass.cs
    └── ...
```

### Managers/ (통합)
```
├── Model3dManager.cs          (Data + Buffer 통합)
├── SelectionManager.cs         (단일)
├── ResourceManager.cs          (GL + Shader 통합?)
```

### Utilities/ (Helper 통합)
```
├── SelectionUtility.cs         (Pick + HitTest)
├── SceneUtility.cs             (TreeView + SceneGraph)
├── MatrixUtility.cs            (Matrix 계산들)
```

---

## ? 체크리스트

### 즉시 조치 필요
- [ ] SelectionManager vs SelevtionManager 확인
- [ ] SelectionControl3D 중복 확인
- [ ] Mock 클래스 분리
- [ ] ModelRendererManager 역할 확인

### 상세 분석 필요
- [ ] GeometryBase 사용처 확인
- [ ] Interfaces.cs 내용 확인
- [ ] SceneState vs SceneParameters 비교
- [ ] SceneRenderer vs DrawRenderer 비교
- [ ] CurveRenderer vs DrawCurves 비교

### 통합 검토
- [ ] Manager 클래스 통합 가능성
- [ ] Helper 클래스 통합 가능성
- [ ] Utility 클래스로 변환

---

## ?? 다음 단계

1. **중복 클래스 확인** (SelectionManager 등)
2. **역할 중복 클래스 분석** (Manager, Helper)
3. **통합 계획 수립**
4. **단계별 리팩토링 실행**
5. **테스트 및 검증**

---

이 분석을 바탕으로 구체적인 개선 작업을 진행합니다!
