# HANDOFF — 등불지기 (Lampkeeper)

다른 AI 도구(GPT/Codex 등)나 다른 세션이 이어받을 수 있도록 현재 상태를 적는 문서. 작업 단위가 끝날 때마다 갱신한다.
프로젝트 규칙·세계관·아키텍처는 `CLAUDE.md` (= `AGENTS.md`, 같은 내용)에 있다. 먼저 그것을 읽을 것.

- 마지막 갱신: 2026-11-02
- 저장소: GitHub `nerojima0326/-Lamp-2D`, 브랜치 `main`

## 1. 실행 방법

- Unity **6.5 (6000.5.2f1)** 로 이 폴더를 연다. 씬: `Assets/_Project/Scenes/M1_Greybox.unity`
- 이 에디터는 외부에서 코드를 바꿔도 자동 재컴파일하지 않는다. 코드를 바꾼 뒤 **Ctrl+R** (Assets > Refresh).
- 메뉴
  - `Lampkeeper/그레이박스 씬 만들기` — 테스트 씬 전체를 새로 만든다 (레벨, 플레이어, 등불, 빛 발판, 카메라, 순간이동 지점) + 마지막에 비주얼 패스 적용. 기존 설정 에셋은 재사용.
  - `Tools/Lampkeeper/Apply Visual Pass` — 현재 씬에 빛·그림자·색을 입힌다. 멱등 (찾아서 없으면 만들기).
  - `Lampkeeper/URP 2D 렌더러 설정` — URP 2D 파이프라인 에셋 생성·지정 (이미 적용됨).
- 조작: 이동 A/D·방향키·왼쪽 스틱, 점프 Space/Z/게임패드 A, 등불 던지기·회수 X/J/게임패드 X (방향키로 8방향 조준)
- 테스트용 순간이동: 플레이 중 숫자 키 1~4 (시작 / 간격 점프 / 떠 있는 계단·낙하 / 빛 발판 구간). 에디터·개발 빌드에서만.
- 튜닝: `Assets/_Project/Data/` 의 ScriptableObject. 플레이 중 바꾸면 바로 반영되고 플레이를 멈춰도 유지된다.

## 2. 완료된 것

| 단계 | 내용 |
|---|---|
| M1 그레이박스 + 컨트롤러 | 코요테 0.1s, 점프 버퍼 0.1s, 가변 점프(최소 높이 지정), 하강 중력 배수, 최대 낙하 속도, 최고점 체공 보정, 가감속 곡선, 천장 박기 처리. 4구간 테스트 레벨 |
| M2 빛 기믹 | `LightField`(빛 판정), `LightPlatform` 상태 머신(Solid/Dissolving/Gone/Forming, 0.2s 디졸브, 몸이 겹쳐 있으면 실체화 보류), 등불 던지기→공중 고정(벽에 닿거나 최대 거리)→회수, 불씨(손을 떠나면 발밑이 무너짐), 8방향 조준 |
| URP 2D | 파이프라인 에셋 `Assets/_Project/Settings/Lampkeeper_URP.asset` |
| 비주얼 1단계 (라이팅) | 아래 3장 |

조작감 수치는 아직 사람이 플레이하며 튜닝하지 않았다 (기본값 상태).

## 3. 비주얼 퀄리티 작업 (진행 중)

방향: "실루엣 + 따뜻한 등불 빛". 외부 에셋 없이 코드/에디터 스크립트로 생성. **조작감과 물리 판정은 바꾸지 않는다** (비주얼은 자식 오브젝트·별도 컴포넌트로).

순서: 1 라이팅 정상화 → 2 지형 질감 → 3 패럴랙스 배경 → 4 플레이어 연출 → 5 파티클 → 6 포스트 프로세싱/카메라

### 1단계 라이팅 — 완료 (`Assets/_Project/Scripts/Visual/`)

2026-11-02 Unity 6.5 플레이로 확인: 등불 주변 공기가 따뜻하게 밝고, 바위 몸통은 어두운 실루엣, 빛 안의 윗면 띠만 밝게 드러나며, 턱 뒤 바닥은 그림자에 가린다. 레벨 밖 검은 영역은 그대로 (3단계 과제).

- `LightSourceVisual`: 게임플레이 `LightSource.Radius`를 매 프레임 읽어 2D 라이트를 맞춘다. 등불은 밝은 핵(반경×0.4) + 은은한 번짐(끝이 빛 발판 판정 경계와 같음, 공기 중 볼류메트릭), 불씨는 하나. Perlin 흔들림(밝기 ±8%, 반경 ±2%).
- `SceneLighting`: 전역 빛(차가운 남색, 0.12)과 카메라 배경색을 팔레트에 맞춘다.
- `TerrainShadowStyle`: 지형 블록의 `ShadowCaster2D`를 Cast+Self Shadow로. 벽 뒤로 그림자, 바위 몸통은 자기 그림자로 어두운 실루엣.
- 지형 윗면 띠: URP의 자기 그림자는 스프라이트 전체를 덮어서 윗면이 밝아지지 않는다. 그래서 `VisualPass`가 블록 윗면 바로 위에 얇은 띠(`Surface n`)를 얹어 빛을 받게 한다. 다른 블록에 덮인 구간은 `TerrainSurfaces.ExposedTopSpans`로 뺀다.
- 설정 에셋: `Data/Visual/VisualPalette`, `LightingProfile`(플레이 중 즉시 반영), `TerrainStyle`(Apply Visual Pass 재실행 시 반영)
  - 볼류메트릭 그림자 세기는 1 (0.85면 바위 몸통 위로 빛 번짐이 15% 비쳐 "바위 속이 밝은" 것처럼 보였다)
  - 지형 색(몸통·윗면 띠)과 빛 발판 색은 Apply Visual Pass 실행 시에 입혀진다. 빛 발판은 런타임에 시작 색을 캐시하므로 플레이 중 팔레트 변경이 반영되지 않는다.
- 블록은 Transform 스케일 대신 SpriteRenderer Sliced 크기 + BoxCollider2D 크기로 만든다 (그림자 모양이 늘어나지 않게). 충돌 크기는 이전과 같다.

## 4. 다음 할 일

1. **비주얼 2단계 지형 질감** — 결정 필요: 레이아웃에 0.5·2.2 같은 비정수 좌표가 있어 1칸 Tilemap/Rule Tile에 그대로 안 맞는다.
   - (A) 현재 블록에 절차적 9-슬라이스 텍스처(윗면 하이라이트, 불규칙 윤곽, 돌 노이즈, 이끼) — 레이아웃 그대로
   - (B) Tilemap + Rule Tile (2D Tilemap Extras 패키지 필요) — 0.5 격자 또는 레이아웃 일부 수정
2. 비주얼 3단계: 패럴랙스 3~4층(하늘 그라데이션, 원경 폐허, 중경, 전경), 높이 안개, 레벨 밖이 안 보이게 Cinemachine Confiner + 배경 범위
3. 비주얼 4단계: 망토 두른 아이 실루엣, 등불 스프링 흔들림, 스쿼시/스트레치, 방향 반전, 망토 끌림, 먼지
4. 비주얼 5단계: 떠다니는 재(Lit), 등불 불씨, 빛 경계의 어둠 연기
5. 비주얼 6단계: URP Volume(Bloom, Vignette, Film Grain, Color Adjustments, 약한 Chromatic Aberration), 카메라 데드존·흔들림 유틸 뼈대
6. 그 뒤 M3 (기름·밝기 단계·UI)

## 5. 알려진 문제·주의

- **빛 판정이 벽을 통과한다.** `LightField`는 가림을 고려하지 않는데 화면에는 그림자가 있다. 벽 뒤에 빛 발판을 두면 "어두워 보이는데 밟힌다". 지금 레이아웃엔 해당 위치 없음. 언젠가 판정에도 가림(레이캐스트)을 넣을 것.
- 빛 발판은 일부만 빛에 걸쳐도 전체가 실체화된다 (판정 규칙). 화면에서는 빛 원 안에 든 부분만 밝게 보인다. 예: M2 구간 첫 발판은 바닥 끝에서 왼쪽 끝만 밝다.
- Unity 6.5에서 `Object.GetInstanceID()`는 컴파일 에러(obsolete), `FindObjectsByType(..., FindObjectsSortMode)`·`FindFirstObjectByType`는 경고. `FindObjectsByType<T>(FindObjectsInactive)`, `FindAnyObjectByType<T>`를 쓴다.
- 씬·에셋 YAML은 직접 고치지 않는다 (`CLAUDE.md` 9장). 씬 변경은 에디터 메뉴 스크립트로.
- `CLAUDE.md`를 고치면 `AGENTS.md`도 같은 내용으로 맞춘다 (AGENTS.md는 CRLF).

## 6. 코드 지도

```
Assets/_Project/
  Scripts/Player/   PlayerMotor(순수 이동 로직) · PlayerController · PlayerInputReader · PlayerMovementSettings · DebugWarp
  Scripts/Light/    LightField · LightCircle · LightSource · LightPlatform(+StateMachine, Settings) · Lantern(+Motion, Aim, Settings)
  Scripts/Visual/   LightSourceVisual · SceneLighting · TerrainShadowStyle · TerrainSurfaces · FlickerNoise · VisualPalette · LightingProfile · TerrainStyle
  Editor/           GreyboxSceneBuilder · VisualPass · UrpSetup · EditorAssetUtility
  Tests/EditMode/   순수 로직 테스트
```

## 7. 검증 상태

- EditMode 테스트: 56개 (PlayerMotor 16, LanternMotion 10, LightField 8, LightPlatformStateMachine 7, TerrainSurfaces 6, LanternAim 5, FlickerNoise 4) — 2026-11-02 Unity 6.5 배치 모드에서 56/56 통과
- 테스트 실행: Unity의 Window > General > Test Runner > EditMode > Run All, 또는 에디터를 닫은 상태에서
  `Unity.exe -batchmode -projectPath <이 폴더> -runTests -testPlatform EditMode -testResults results.xml`
