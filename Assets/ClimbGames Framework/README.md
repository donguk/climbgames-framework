# 🏔️ ClimbGames Framework

유니티 프로젝트의 핵심 기능과 효율적인 **씬 전환(Scene Transition)** 시스템을 제공하는 프레임워크입니다.

---

## 🚀 주요 기능
* **Scene Management**: `UniTask` 기반의 비동기 씬 로딩 및 스무스한 전환 로직
* **Reactive Architecture**: `R3`를 활용한 고성능 이벤트 처리 및 상태 관리 지원

## 🛠 필수 요구 사항 (Dependencies)
이 패키지는 다음 라이브러리들이 설치되어 있어야 정상 동작합니다.
* [**UniTask**](https://github.com/Cysharp/UniTask) (v2.5.10 이상)
* [**R3**](https://github.com/Cysharp/R3) (v1.3.0 이상)
  * *참고: 본 패키지는 R3 핵심 DLL 및 Unity Extension을 포함하고 있습니다.*

---

## 📦 설치 방법 (Quick Start)

### 1. Scoped Registry 등록
프로젝트 루트의 `Packages/manifest.json` 파일을 열어 `scopedRegistries` 섹션에 아래 내용을 추가하세요.

```json
"scopedRegistries": [
  {
    "name": "ClimbGames",
    "url": "[http://192.168.0.10:4873](http://192.168.0.10:4873)",
    "scopes": [
      "com.climbgames"
    ]
  }
]