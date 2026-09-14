# 🏔️ ClimbGames Framework

유니티 프로젝트의 핵심 기능과 효율적인 **씬 전환(Scene Transition)** 시스템을 제공하는 프레임워크입니다.

---

## 🚀 주요 기능
* **Scene Management**: `UniTask` 기반의 비동기 씬 로딩 및 스무스한 전환 로직
* **Reactive Architecture**: `R3`를 활용한 고성능 이벤트 처리 및 상태 관리 지원

## 🛠 필수 요구 사항 (Dependencies)
이 패키지는 다음 라이브러리들이 설치되어 있어야 정상 동작합니다.
* [**UniTask**](https://github.com/Cysharp/UniTask) (v2.5.10 이상)
* [**Addressables**](v2.5.10 이상)
---

## 📦 설치 방법 (Quick Start)

### 1. Scoped Registry 등록
프로젝트 루트의 `Packages/manifest.json` 파일을 열어 `dependencies` 섹션에 아래 내용을 추가하세요.

```json
"dependencies": {
    "com.climbgames.framework": "https://github.com/donguk/climbgames-framework.git?path=/Assets/ClimbGames Framework#develop",
}  