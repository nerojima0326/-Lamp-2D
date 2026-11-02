# 등불지기 (Lampkeeper) — 프로젝트 가이드

Unity 2D 플랫포머 / 메트로배니아. 이 문서는 Claude Code가 작업 전 항상 참고하는 기준이다.

## 1. 한 줄 컨셉

어둠이 기억을 먹어치우는 세계에서, 마지막 등불을 든 아이가 밝힐지 숨을지 매 순간 선택하며 나아가는 게임.

## 2. 세계관 요약

- 세계는 '망각(어둠)'에 잠식되고 있다. 빛이 닿은 곳만 형태를 유지한다.
- 주인공은 마지막 등불을 지키는 아이. 등불의 기름은 사람들의 기억에서 짜낸 것.
- 어둠 속에는 빛에 끌리는 존재(그림자)와, 눈이 멀어 소리에만 반응하는 존재(종지기의 잔재)가 산다.
- 세계관은 대사보다 배경·적 디자인·아이템 설명으로 전달한다.

## 3. 핵심 기믹 (설계 원칙: 기믹 = 세계관)

### 3-1. 빛 = 땅

- `LightPlatform`은 어떤 `LightSource`의 범위 안에 있을 때만 콜라이더가 활성화된다.
- 빛을 벗어나면 즉시 사라지지 않고 0.2초 디졸브 후 비활성화 (밟고 있다가 억울하게 떨어지지 않게).
- 다시 빛이 닿으면 페이드인과 함께 실체화.

### 3-2. 등불 던지기

- 등불을 던지면 그 위치에 고정된 `LightSource`가 된다. 먼 곳에 길을 만들 수 있다.
- 등불이 손을 떠나면 플레이어 주변은 작은 '불씨' 반경만 남는다 → 발밑이 무너질 위험.
- 회수 키로 등불을 다시 불러온다 (회수 중 궤적에도 잠깐 빛이 남음 — 확장 아이디어).

### 3-3. 기름 = HP이자 자원

- 피격 시 기름 감소. 0이 되면 사망.
- 밝기 단계(어둡게 / 보통 / 밝게)에 따라 기름 소모량과 빛 반경이 달라진다.

### 3-4. 밝힐수록 들킨다 (메아리 컨셉에서 가져온 딜레마)

- 밝기가 높을수록 `그림자` 계열 적의 감지 범위가 커지고 몰려온다.
- 어둡게 하면 숨을 수 있지만 발판이 사라진다.

### 3-5. 소리 (메아리 컨셉에서 가져온 요소)

- `눈먼 자` 계열 적은 빛을 무시하고 소리(착지, 달리기, 종)에만 반응한다.
- 후반 능력 '종': 치면 음파가 퍼지며 어둠 속 지형 윤곽이 잠깐 보인다. 대신 모든 눈먼 자가 반응.
- 종탑 구역은 이 소리 기믹 중심으로 설계한다.

## 4. 기술 스택 (변경 시 이 섹션 수정)

- Unity 6.5 (6000.5.2f1), URP 2D Renderer (17.5.0), 2D Lights
- New Input System (1.19.0)
- Tilemap, Cinemachine (3.1.6)
- 물리: Rigidbody2D (Dynamic) + 커스텀 컨트롤러
- 렌더 파이프라인 에셋: `Assets/_Project/Settings/Lampkeeper_URP.asset` (메뉴 `Lampkeeper/URP 2D 렌더러 설정`으로 생성·지정)

## 5. 아키텍처

- `LightSource` : 반경·강도를 가진 컴포넌트. 정적 레지스트리에 자신을 등록/해제.
- `LightField` : `IsLit(Vector2 pos)`, `GetExposure(Vector2 pos)` 조회 서비스. 성능 문제가 생기면 공간 분할 도입.
- `LightPlatform` : FixedUpdate에서 `LightField.IsLit` 확인 → 상태(Solid / Dissolving / Gone / Forming) 전환.
- `Lantern` : 들고 있음 / 날아감 / 고정됨 / 회수 중 상태 머신.
- `OilTank` : 기름 수치, 밝기 단계별 소모, 피격 처리. 이벤트로 UI에 알림.
- `NoiseSystem` : `NoiseEvent(position, loudness)` 발행 → 범위 내 청각 적이 수신.
- 적: `Shade`(빛 노출도 기반 감지), `Blind`(소리 기반 감지). 상태 머신 기반.
- 튜닝 값은 전부 ScriptableObject로 분리 (점프 높이, 빛 반경, 기름 소모량 등).

## 6. 폴더 구조

```
Assets/_Project/
  Scripts/       # Lampkeeper.asmdef (런타임 전체)
    Core/        # 이벤트, 유틸, 게임 상태
    Player/      # 컨트롤러, 입력
    Light/       # LightSource, LightField, LightPlatform, Lantern
    Oil/
    Noise/
    Enemies/
    UI/
    Visual/      # 화면 연출 (빛 비주얼, 팔레트, 그림자). 게임플레이를 읽기만 한다
  Editor/        # Lampkeeper.Editor.asmdef — 메뉴 아이템, 씬/에셋 자동 생성
  Data/          # ScriptableObject 에셋 (Data/Visual/: 팔레트·라이팅·지형 스타일)
  Prefabs/
  Scenes/
  Art/
  Tests/EditMode/  # Lampkeeper.Tests.EditMode.asmdef (네임스페이스 Lampkeeper.Tests)
```

- 런타임 코드는 하나의 asmdef(`Lampkeeper`)에 둔다. 테스트 asmdef가 이것을 참조한다.
- 선택 패키지(Cinemachine 등)를 쓰는 에디터 코드는 asmdef `versionDefines`로 심볼(`LAMPKEEPER_CINEMACHINE`)을 정의하고 `#if`로 감싼다.
- `Editor` 네임스페이스 안에서는 `UnityEditor.Editor` 타입을 반드시 전체 이름으로 쓴다 (`Lampkeeper.Editor`와 충돌).
- 같은 이유로 `Lampkeeper.*` 안에서 Unity의 3D `Light`가 필요하면 `UnityEngine.Light`로 쓴다 (`Lampkeeper.Light`와 충돌). 2D 빛은 URP의 `Light2D`를 쓴다.
- 판정용 빛(`LightSource` 반경)과 화면의 `Light2D`는 항상 같은 반경이어야 한다. `Lampkeeper.Visual.LightSourceVisual`이 매 프레임 `LightSource.Radius`를 읽어 맞춘다.
- 게임플레이 코드(`Lampkeeper.Light`, `Player` 등)는 `Lampkeeper.Visual`을 참조하지 않는다. 비주얼이 게임플레이를 읽는 한 방향만 허용한다.
- 씬의 빛·그림자·색은 `Tools/Lampkeeper/Apply Visual Pass` 메뉴가 입힌다 (찾아서 없으면 만들기, 여러 번 실행해도 결과 동일). 비주얼 튜닝 값은 `Data/Visual/`의 ScriptableObject에 둔다.

## 7. 플레이어 조작감 기준 (산나비·할로우 나이트급 손맛 목표)

- 코요테 타임 0.1초, 점프 입력 버퍼 0.1초
- 가변 점프 (버튼을 일찍 떼면 낮게)
- 하강 시 중력 배수 증가, 최대 낙하 속도 제한
- 가속/감속 곡선으로 미끄럽지 않게
- 모든 수치는 ScriptableObject에서 플레이 중 조정 가능해야 한다

## 8. 마일스톤

- M1 그레이박스 + 플레이어 컨트롤러 (조작감 완성이 목표)
- M2 LightSource / LightPlatform / 등불 던지기·회수
- M3 기름 시스템 + 밝기 단계 + UI
- M4 적: Shade(빛 감지), Blind(소리 감지) + NoiseSystem
- M5 버티컬 슬라이스: 1구역 + 보스 1마리

각 마일스톤은 "네모 박스만으로도 재밌는가"를 먼저 확인하고 다음으로 넘어간다.

## 9. Claude Code 작업 규칙

- 스크립트는 C#으로 작성하고, 네임스페이스는 `Lampkeeper.<폴더명>`.
- `.unity` / `.prefab` / `.asset` YAML 파일을 직접 수정하지 않는다. 씬·프리팹 세팅이 필요하면 에디터에서 할 단계를 목록으로 안내하거나, `Editor/` 폴더에 메뉴 아이템 스크립트로 자동 생성한다.
- 순수 로직(기름 계산, 빛 판정 등)은 MonoBehaviour와 분리해 EditMode 테스트를 작성한다.
- `Update`에서 `FindObjectOfType`, `GetComponent` 반복 호출 금지. 캐싱한다.
- 매직 넘버 금지. 튜닝 값은 ScriptableObject로.
- 작업이 끝나면 변경 파일 목록과 에디터에서 해야 할 수동 작업을 요약한다.
