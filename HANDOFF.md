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
- 조작: 이동 A/D·방향키·왼쪽 스틱, 점프 Space/Z/게임패드 A, 대시 왼쪽·오른쪽 Shift/게임패드 RB, 등불 던지기·회수 X/J/게임패드 X (방향키로 8방향 조준)
- 테스트용 순간이동: 플레이 중 숫자 키 1~5 (시작 / 간격 점프 / 떠 있는 계단·낙하 / 빛 발판 / 대시와 등불 시험 구간). 에디터·개발 빌드에서만.
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

### 대시·등불 시험 구간

- `DashTrialSetup` 메뉴로 기존 `M1_Greybox`에 별도 구간을 추가하고 저장했다. 플레이 중 숫자 5로 진입·재시작한다. 기존 맵과 물리적으로 떨어진 테스트 구역이다.
- 시작 지대 → 7.5유닛 틈(점프+대시) → 중간 지대 → 6.5유닛 틈과 빛 발판(앞으로 등불을 던진 뒤 점프+대시) → 도착 지대. 밑에는 안전 바닥과 복귀 계단이 있다. 빛 발판과 도착 지대 사이도 점프로 건넌다.
- 씬을 다시 만들 때도 `GreyboxSceneBuilder`가 생성하며, `Tools/Lampkeeper/Apply Dash Trial`을 반복 실행해 재적용할 수 있다. 변경: `Editor/DashTrialSetup.cs`, `Editor/GreyboxSceneBuilder.cs`, `Scenes/M1_Greybox.unity`, 메타 파일, 이 문서.
- Unity 컴파일, 메뉴 실행 및 씬 저장 확인. 자동 UI 입력으로 연속 이동을 조작할 수 없어 전체 통과 난이도는 실제 플레이 검증이 필요하다.

### Shift 대시·잔상 — 적용됨

- `PlayerInputReader`에 양쪽 Shift 및 게임패드 RB 바인딩. 한 번 누를 때 한 번 실행한다.
- `PlayerMotor`에서 마지막으로 바라보는 방향으로 수평 대시. 방향 전환은 대시가 끝난 뒤 반영한다. 지상·공중 모두 가능하며 대시 중 수직 속도는 0, 종료 후 중력이 다시 적용된다. 공중 대시 횟수 제한·무적은 없다.
- `PlayerMovementSettings`: 속도 22, 지속시간 0.16초(장애물 없을 때 3.52유닛), 시작 기준 재사용 대기 0.55초. 재사용 대기 중 입력은 예약하지 않는다. 대시 종료 시 달리기 속도로 제한하고, 순간이동/비활성화 시 대시를 초기화한다. Rigidbody2D 연속 충돌 판정을 사용한다.
- `PlayerDashTrail`은 애니메이션 갱신 뒤 현재 스프라이트/방향/월드 위치를 복사한다. 독립적인 잔상 풀 12개를 재사용하며 비활성화/파괴 시 정리한다. `DashTrailSettings`에서 생성 간격 0.025초, 수명 0.22초, 푸른색·투명도를 조정한다. Unlit 재질로 어둠에서도 잔상이 보인다.
- `Tools/Lampkeeper/Apply Player Dash`를 실행해 `M1_Greybox`에 연결·저장했다. Visual Pass와 씬 재생성에도 연결된다. 현재 씬에는 수동 작업이 필요 없다.
- 변경: `Scripts/Player/{PlayerInputReader,PlayerMotor,PlayerController,PlayerMovementSettings}.cs`, `Scripts/Visual/{PlayerDashTrail,DashTrailSettings}.cs`, `Editor/{PlayerDashSetup,VisualPass}.cs`, `Tests/EditMode/PlayerDashTests.cs`, `Data/Visual/DashTrailSettings.asset`, `Art/Characters/Animation/DashAfterimage.mat`, `Scenes/M1_Greybox.unity` 및 메타 파일.
- Unity EditMode **79/79 통과**(대시 9개 추가: 좌우 방향, 방향 고정, 고정 시간 간격별 거리, 낙하 복귀, 쿨다운/중복 입력, 공중 점프 방지, 초기화). 플레이 진입 확인. 사용자가 플레이 중이라 UI의 Shift 입력 시도가 차단되어 실제 잔상 화면 검증은 완료하지 못했다. 플레이 모드는 유지했다.

### 등불 위치·불꽃 애니메이션 — 적용됨

- `M1_Greybox`에 적용·저장. `LanternSettings.holdOffset`을 `(0.4, -0.32)`로 내려 손 아래에 들도록 하고, 등불 셀 높이를 0.65로 줄였다. 판정과 화면 빛은 같은 Lantern 위치를 따른다. 빛 반경·던지기 속도 등은 유지했다.
- `Art/Characters/Animation/Lantern_Flame.png`: 기존 Simple 등불을 참조한 내장 이미지 생성 결과. 프롬프트는 동일한 청동 프레임, 투명 배경, 2×2 배열, 불꽃만 왼쪽/중앙/오른쪽/중앙으로 흔들리는 단순 픽셀 이미지. 각 셀의 불투명 영역 중심을 기준점으로 맞춘 Sprite 4개를 `LanternFrames`에 생성했다.
- `LanternFlameAnimation`이 초당 6프레임으로 반복 재생. `Data/Visual/LanternFlameSettings`에서 FPS와 셀 높이를 실시간 조절한다. 최초 설치만 손 위치를 변경하며 재적용 시 위치 튜닝을 보존한다.
- 적용 메뉴: `Tools/Lampkeeper/Apply Lantern Animation`. 전체 Visual Pass 및 씬 빌더에도 연결했다. 현재 씬은 별도 수동 적용이 필요 없다.
- 변경 파일: `Scripts/Light/LanternSettings.cs`, `Scripts/Visual/LanternFlameAnimation.cs`, `Scripts/Visual/LanternFlameSettings.cs`, `Editor/LanternVisualSetup.cs`, `Editor/VisualPass.cs`, 위 이미지·프레임 에셋, `Data/LanternSettings.asset`, `Data/Visual/LanternFlameSettings.asset`, `Scenes/M1_Greybox.unity` 및 메타 파일.
- Unity 컴파일 및 플레이 화면의 등불 위치·스프라이트 표시 확인. 이번 비주얼 변경에서 EditMode 테스트는 재실행하지 않았다(직전 결과 70/70). 사용자 플레이 입력이 감지되어 플레이 모드를 유지했다.

### 플레이어 이동 애니메이션 — 적용됨

- `Art/Characters/Animation/Lampkeeper_Movement.png`: 단순 픽셀 캐릭터를 참조해 내장 이미지 생성 도구로 만든 4×4 시트. 행 순서 대기/달리기/점프 상승/낙하, 각 4프레임. 프롬프트 방향: 동일한 남색 후드 아이, 오른쪽 측면, 투명 배경, 작은 픽셀과 제한된 색상, 모션별 4프레임.
- `PlayerAnimationSetup`이 시트를 Sprite 에셋 16개로 분할하고 발 아래 기준점을 맞춰 `M1_Greybox`에 연결·저장했다. 메뉴 `Tools/Lampkeeper/Apply Player Animation`. `Apply Visual Pass`와 씬 빌더에서도 연결된다.
- `PlayerSpriteAnimation`이 Motor의 접지/방향과 Rigidbody2D 속도를 읽어 프레임을 재생한다. Animator Controller 대신 SpriteSequence 기반 프레임 애니메이션이다. 이동과 충돌 로직은 변경하지 않았다. 기존 PlayerFacingVisual은 해당 Visual에서 제거하고 방향 반전도 애니메이션이 처리한다.
- `Data/Visual/PlayerAnimationSettings`에서 각 동작의 FPS, 반복 여부, 크기와 위치를 플레이 중 튜닝한다. 착지 시 접지 결과에 따라 대기/달리기로 즉시 복귀한다.
- Unity 컴파일, 씬 저장, 플레이 모드 이미지 표시 확인. EditMode 70/70 통과(새 애니메이션 테스트 8개 포함). 자동 UI 도구가 키 홀드를 지원하지 않아 실제 달리기·연속 점프의 시각적 타이밍 검증은 남아 있다.
- 이전 문단의 '정지 이미지' 및 Simple 이미지 적용 대기 기록은 과거 상태다. 현재 플레이어는 위 애니메이션 시트를 사용한다. 등불은 별도 스프라이트와 기존 던지기/회수 동작을 유지한다.

적 감지 카메라 줌을 `M1_Greybox`에 적용·저장했다. `Tools/Lampkeeper/Apply Encounter Camera`로 기존 씬에도 연결한다. 혼자 있을 때 Orthographic Size 4.5, 적 근처에서는 7, 적이 사라진 뒤 1.2초 후 복귀한다. `Data/Visual/EncounterCameraSettings`에서 플레이 중 튜닝한다. 현재 적 AI/적 오브젝트는 없으므로, 향후 적의 비주얼 루트에 `EnemyCameraTarget`을 붙이고 사망 시 비활성화해야 한다. 감지는 화면 직사각형 기준이며 벽/어둠의 가림은 고려하지 않는다. Unity EditMode 테스트 62/62 통과 (줌 테스트 6개 포함).

시작 구간 지형 질감 미리보기를 비주얼 패스로 적용했다. `TerrainStyle`의 Texture Preview End X와 Texture Color로 범위·색을 조정하고 메뉴를 재실행한다. 실제 플레이에서 질감의 가시성은 추가 확인이 필요하다.

캐릭터와 등불의 이미지 스프라이트를 `M1_Greybox` 씬에 적용하고 저장했다 (`Art/Characters/`). 비주얼 패스가 기존 네모를 교체하고 플레이어 방향 반전을 연결하며, Unity 게임 뷰에서 스프라이트 표시를 확인했다. `Data/Visual/CharacterVisualStyle`에서 크기와 위치를 조정한 후 메뉴를 재실행한다. 정지 이미지이며 달리기·점프 애니메이션은 아직 없다. 실제 플레이에서 움직임과 크기·정렬은 추가 확인이 필요하다.

현재 씬에는 별도로 생성한 `Lampkeeper_Pixel.png`, `Lantern_Pixel.png` 픽셀 아트 버전을 적용했다. 원본 일러스트는 같은 폴더에 보관한다. 픽셀 버전은 Point 필터와 무압축으로 가져온다.

더 단순한 픽셀 버전 `Lampkeeper_Simple.png`, `Lantern_Simple.png`을 추가하고 비주얼 패스가 이 파일을 사용하도록 변경했다. Unity 창 접근 실패로 씬에는 아직 적용하지 못했다. Ctrl+R 후 `Tools/Lampkeeper/Apply Visual Pass`를 실행해 적용·저장하고 화면을 확인해야 한다.

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
