# 등불지기 (Lampkeeper) — Lamp 2D

Unity 6 · URP 2D 플랫포머 / 메트로배니아 프로젝트.
어둠이 기억을 먹어치우는 세계에서, 마지막 등불을 든 아이가 **밝힐지 숨을지** 선택하며 나아가는 게임.

## 열기
1. Unity Hub → Add → 이 저장소 폴더 선택 (`ProjectSettings/ProjectVersion.txt`의 에디터 버전 사용)
2. `Assets/_Project/Scenes/M1_Greybox.unity` 열기

## 웹에서 플레이
**https://nerojima0326.github.io/-Lamp-2D/** — `main`에 푸시할 때마다 GitHub Actions가 WebGL로 빌드해서 이 주소에 올린다 (`.github/workflows/webgl-pages.yml`, 빌드 10~20분).

처음 한 번만 저장소 설정이 필요하다.
1. **Settings → Pages → Build and deployment → Source: GitHub Actions**
2. **Settings → Secrets and variables → Actions → New repository secret** 으로 세 개 등록
   - `UNITY_LICENSE`: `C:\ProgramData\Unity\Unity_lic.ulf` 파일 내용 전체. 파일이 없으면 Unity Hub → Preferences → Licenses → Add → 무료 Personal 라이선스를 받은 뒤 다시 확인 (숨김 폴더)
   - `UNITY_EMAIL`, `UNITY_PASSWORD`: Unity 계정
3. **Actions** 탭 → `WebGL → GitHub Pages` → Run workflow (또는 아무 커밋이나 푸시)

로컬에서 빌드하려면 Unity Hub에서 에디터에 **Web Build Support** 모듈을 추가하고 `Lampkeeper/웹 빌드 (WebGL)` 메뉴를 쓴다.

## 문서
- `CLAUDE.md` / `AGENTS.md` — 프로젝트 규칙, 세계관, 아키텍처, 마일스톤 (같은 내용, AI 코딩 도구용)
- `HANDOFF.md` — 현재 진행 상황, 미완성 부분, 다음 할 일
