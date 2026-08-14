# 🛠 ClimbGames Framework - Key Features

유니티 게임 개발 시 반복되는 핵심 공통 모듈(Scene, Singleton, FSM, UI)을 독립된 패키지로 모듈화하여 제어하는 C# 기반 프레임워크입니다.

---

## 📑 목차
1. [SceneTransition (비동기 씬 관리)](#1-scenetransition-비동기-씬-관리)
2. [MonoSingleton (제네릭 싱글톤)](#2-monosingleton-제네릭-싱글톤)
3. [FSM (유한 상태 머신)](#3-fsm-유한-상태-머신)
4. [UIManager (UI 및 레이어 관리)](#4-uimanager-ui-및-레이어-관리)

---

## 1. SceneTransition (비동기 씬 관리)
`UniTask` 기반의 비동기 씬 전환 시스템으로, 씬 전환 사이의 생명주기와 전환 연출(Transition Handler)을 정교하게 제어합니다[cite: 3].

### 🌟 주요 특징
* **비동기 씬 로딩 흐름 제어:** `MonoScene` 인터페이스를 확장하여 씬의 비활성화(`Deactivate`), 초기화(`InitializeAsync`), 활성화(`ActivateAsync`) 수명주기를 비동기로 제어[cite: 3]
* **EmptyScene 기법 활용:** 메모리 해제 및 안정적인 씬 전환을 위해 선택적으로 중간 거점(`EmptyScene`)을 거쳐 로딩하도록 구현 (`FrameworkSettings` 연동)[cite: 3]
* **커스텀 트랜지션 연출 지원:** `ITransitionHandler` 인터페이스를 통해 씬 로딩 전/후 연출(Fade In/Out, Progress Bar)을 유연하게 주입 가능[cite: 3]
* **이벤트 기반 연동:** `transitionStarted`, `sceneLoaded` 이벤트를 전달하여 UIManager 등 타 시스템과 자동 동기화[cite: 3]

### 💻 사용 예시
```csharp
// 씬 파라미터 전달 및 비동기 전환 실행
var param = new FishingSceneParameter { SpawndId = 101 };
await SceneTransition.TransitionAsync("FishingScene", param);
```

---

## 2. MonoSingleton (제네릭 싱글톤)
Thread-safe 특성을 보장하며, 리소스 프리팹 자동 생성 및 Attribute 기반 설정을 지원하는 강력한 MonoBehaviour 싱글톤입니다[cite: 2].

### 🌟 주요 특징
* **Lock 기반 Thread-Safe 및 Quitting 안전성:** `lock`을 사용하여 멀티스레드 인스턴스 생성 예외를 방지하고, 앱 종료 시점(`_applicationIsQuitting`)의 예외적 오브젝트 생성을 차단[cite: 2]
* **Attribute 기반 경로 및 설정 관리:**
  * `[AssetPathAttribute]`: Resources 또는 에셋 경로를 지정하여 씬에 오브젝트가 없더라도 자동으로 프리팹을 로드 및 생성[cite: 2]
  * `[SingletonConfigAttribute]`: `DontDestroyOnLoad` 적용 여부를 코드 외부 속성으로 깔끔하게 설정[cite: 2]
* **자동 씬 정리(OnDestroy):** 씬 이동 등으로 싱글톤이 파괴될 경우 내부 정적 참조를 자동으로 해제하여 메모리 누수 방지[cite: 2]

### 💻 사용 예시
```csharp
[SingletonConfig("Resources/UIManager", DontDestroy = true)]
public class UIManager : MonoSingleton<UIManager>
{
    // Instance 호출 시 Resources/UIManager 프리팹을 자동 생성하고 DontDestroyOnLoad 적용
}
```

---

## 3. FSM (유한 상태 머신)
열거형(Enum) 또는 커스텀 타입을 Key로 사용하는 제네릭 기반의 상태 머신 코어입니다[cite: 1].

### 🌟 주요 특징
* **`FSM<T>` 제네릭 지원:** 상태 구분자를 Enum, int, string 등 사용자 정의 타입으로 유연하게 설정 가능[cite: 1]
* **중복 전환 및 예외 방지:** `isChanging` 플래그를 두어 상태 전환 도중 발생하는 중복 `ChangeState` 요청을 안전하게 차단[cite: 1]
* **파라미터 전달 기능:** `IStateParam` 인터페이스를 통해 상태 진입 시(`Enter`) 필요한 데이터를 동적으로 전달[cite: 1]
* **전역 업데이트 루프 통합:** `FSMUpdater`를 지원하여 개별 FSM의 `Resume()` / `Pause()` 제어 및 메모리 해제(`Dispose`) 시 상태 정리 수명주기 캡슐화[cite: 1]

### 💻 사용 예시
```csharp
// FSM 생성 및 상태 등록
var fsm = new FSM<PlayerState>("PlayerFSM", showDebug: true);
fsm.Initialize(
    (PlayerState.Idle, new IdleState()),
    (PlayerState.Casting, new CastingState())
);

// 상태 시작 및 전환
fsm.Start(PlayerState.Idle);
fsm.ChangeState(PlayerState.Casting, new CastingParam { Force = 10f });
```

---

## 4. UIManager (UI 및 레이어 관리)
URP(Universal Render Pipeline) 환경에 최적화된 계층형 UI 및 World Space UI 관리 시스템입니다[cite: 4].

### 🌟 주요 특징
* **URP 카메라 스택(Camera Stacking) 자동 구성:** 
  * MainCamera 뒤에 `WorldUICamera` 및 `UICamera`를 Overlay 타입으로 자동 스택팅[cite: 4]
  * 씬 로딩 및 트랜지션 상황에서도 UI 카메라 렌더링 스택을 동적으로 재구성[cite: 4]
* **레이어(UILayer) 기반 계층 관리:**
  * `World`, `HUD`, `View`, `Popup`, `Top`, `System`, `Transition`으로 상하 레이어를 엄격히 분리하여 뎁스 꼬임 방지[cite: 4]
* **비동기 UI 생성 및 관리:** `AssetManager`와 연동하여 `ShowUI<T>`를 통한 비동기(`UniTask`) 팝업/뷰 생성 지원[cite: 4]
* **EventSystem 자동 복구:** 씬 로딩 시 EventSystem이 없을 경우 `InputSystemUIInputModule`이 포함된 EventSystem을 자동 생성하여 입력 유실 방지[cite: 4]

### 💻 사용 예시
```csharp
// 비동기로 View 레이어에 UI 생성 및 오픈
var viewData = new InventoryUIData();
var invView = await UIManager.Instance.ShowUI<InventoryView>("UI/InventoryView", UILayer.View, viewData);

// 특정 UI 닫기
UIManager.Instance.Hide(invView);
```