# ClimbGames Framework

ClimbGames Framework는 Unity 프로젝트에서 반복적으로 사용되는 **Core 시스템, Build Pipeline, Table 데이터 관리 기능**을 하나의 Framework로 제공합니다.

## Overview

* [**Core**](#1-core) — 게임 실행에 필요한 공통 시스템

  * Scene Transition
  * Singleton
  * FSM

* [**Build**](#2-build) — Unity Editor 및 Jenkins 기반 빌드/배포 시스템

  * Editor Build Tool
  * Addressables Build
  * Jenkins Shared Library
  * Jenkins Custom Scripts

* [**Table**](#3-table) — Excel 기반 게임 데이터 관리 및 코드 생성 시스템

  * Excel → C# Table 변환
  * Table Code Generation
  * Enum 자동 생성
  * Table 통합 접근
  * Binary 데이터 저장
  * Table Preview

* [**4. UI**](#4-ui) — UI 관리 및 프리팹 기반 컨트롤 자동 바인딩 시스템

  * UI Manager
  * UI Prefab Convert
  * Automatic Binding Code Generation
  * Custom Binding Schema
  * Nested Group & List Binding
  * Base Type Configuration

---

# 1. Core

게임에서 공통적으로 사용되는 기본 시스템을 제공합니다.

## Scene Transition

`UniTask` 기반의 비동기 씬 전환 시스템으로, 씬 전환 과정에서 발생하는 **초기화, 활성화, 비활성화 및 Transition 연출**을 일관된 흐름으로 관리합니다.

### 주요 기능

* **비동기 씬 생명주기 관리**

  * `Deactivate`
  * `InitializeAsync`
  * `ActivateAsync`

* **Empty Scene 지원**

  * 씬 전환 과정에서 중간 `EmptyScene`을 거쳐 기존 씬의 리소스를 안정적으로 해제
  * `FrameworkSettings`를 통해 사용 여부 설정

* **Custom Transition 지원**

  * `ITransitionHandler`를 통해 씬 전환 전/후 연출을 자유롭게 구현
  * Fade, Loading UI, Progress Bar 등의 연출에 활용 가능

* **이벤트 기반 연동**

  * `transitionStarted`
  * `sceneLoaded`
  * 등의 이벤트를 통해 UI 및 기타 시스템과 연동

---

## Singleton

Unity의 `MonoBehaviour`를 기반으로 한 Singleton 시스템입니다.

`SingletonConfig`를 이용하여 Singleton 생성 방식과 `DontDestroyOnLoad` 여부를 설정할 수 있습니다.

### 사용 예시

```csharp
[SingletonConfig("Resources/UIManager", DontDestroy = true)]
public class UIManager : MonoSingleton<UIManager>
{
}
```

설정된 Singleton은 `Instance` 접근 시 필요한 경우 `Resources`의 Prefab을 자동으로 생성하고 `DontDestroyOnLoad`를 적용합니다.

---

## FSM

Enum 또는 Custom Type을 Key로 사용할 수 있는 **제네릭 기반 Finite State Machine**입니다.

상태 등록, 상태 전환 및 상태별 데이터 전달을 지원하며, 게임플레이 로직을 명확한 상태 단위로 분리할 수 있습니다.

### 사용 예시

```csharp
var fsm = new FSM<PlayerState>("PlayerFSM", showDebug: true);

fsm.Initialize(
    (PlayerState.Idle, new IdleState()),
    (PlayerState.Casting, new CastingState())
);

fsm.Start(PlayerState.Idle);

fsm.ChangeState(
    PlayerState.Casting,
    new CastingParam { Force = 10f }
);
```

---

# 2. Build

Unity Editor에서 직접 빌드하거나 Jenkins를 이용한 **원격 빌드 및 배포 환경**을 구성할 수 있습니다.

## Editor Build Tool

![빌드 윈도우 스크린샷](./Images/image_build_window.png)

**메뉴 위치**

`Tools` > `ClimbGames` > `Build` >  `Settings`

### 주요 기능

* Unity Application Build
* Addressables 기본 Build
* Addressables Content Update Build
* `addressables_content_state.bin` 기반 Content Update 지원
* Addressables Build 결과물 백업
* `EditorEnv.zip` 생성 및 테스트 지원
* BuildPreset 생성(`Tools` > `ClimbGames` > `Build` > `Create Preset`) 및 Jenkins 연동

---

## Jenkins Shared Library

**경로**

```text
ClimbGames Framework/
└─ Editor/
   └─ Build/
      └─ Jenkins/
         └─ SharedLibrary/
```

Jenkins Pipeline에서 공통으로 사용할 수 있는 **OOP 기반 Build / Deploy Framework**를 제공합니다.

### 주요 구성

* `PipelineConfig`
* `DefaultSettings`
* `DefaultProcess`

프로젝트마다 반복되는 빌드 및 배포 로직을 공통화하고, 프로젝트별 설정과 프로세스를 확장할 수 있도록 구성되어 있습니다.

---

## Jenkins Custom Scripts

**경로**

```text
ClimbGames Framework/
└─ Editor/
   └─ Build/
      └─ Jenkins/
         └─ Scripts/
```

프로젝트별 Jenkins Pipeline을 구현하기 위한 Custom Script 영역입니다.

Framework에서 제공하는 기본 Build 기능을 기반으로 프로젝트의 환경에 맞는 **빌드, 배포 및 후처리 작업을 확장**할 수 있습니다.

---

# 3. Table

Excel로 관리하는 게임 데이터를 Unity에서 사용할 수 있는 **C# Table 및 ScriptableObject 기반 데이터로 자동 변환**하는 시스템입니다.

Excel Schema를 기반으로 C# 코드를 자동 생성하기 때문에 별도의 Table 클래스를 직접 작성하지 않고 데이터를 관리할 수 있습니다.

**메뉴 위치**

`Tools` > `ClimbGames` > `Table Converter`

![테이블 정의](./Images/image_table_excel.png)
![테이블 컨버터](./Images/image_table_convert.png)

## 주요 기능

### Excel → C# Table 변환

Excel 데이터를 Unity에서 사용할 수 있는 C# Table 데이터로 변환합니다.

다음과 같은 Collection 타입을 지원합니다.

```text
@list
@dictionary
@keyvalue
```

선언된 타입에 따라 적절한 Table 구조를 자동으로 생성합니다.

TableHeader Column 정의

```
fieldName // excel 서식에 따라 타입이 정해짐
fieldName[int]  // 구체적인 타입 정의
fieldName[enum:ItemType] // enum 타입 정의
fieldName[List<int>] // 리스트 정의
fieldName[key<int>] // dictionary 타입의 key column 으로 사용
```

---

### C# Code Generation

Excel Schema를 기반으로 Table 관련 C# 코드를 자동 생성합니다.

예를 들어 다음과 같은 공통 코드가 자동으로 생성됩니다.

```text
TableEnum.cs
Tables.cs
```

* **`TableEnum.cs`**

  * Table에서 사용하는 Enum을 자동 생성

* **`Tables.cs`**

  * 프로젝트의 모든 Table에 접근할 수 있는 통합 진입점 제공

---

### Binary Data 지원

Table 데이터를 Binary 형태로 저장하고 로드할 수 있습니다.

이를 통해 프로젝트 환경에 따라 ScriptableObject 기반 데이터와 Binary 기반 데이터를 선택하여 사용할 수 있습니다.

---

### Table Preview

Unity Editor에서 변환된 Table 데이터를 직접 확인할 수 있는 **Table Preview 기능**을 제공합니다.

또한 전체 Table을 다시 변환하지 않고 **개별 Table 단위로 Conversion**할 수 있습니다.

---

## 사용 방법

### Asset 기반 Table Load

Unity Asset으로 생성된 Table 데이터를 로드합니다.

```csharp
await Tables.LoadAsync("tables");
```

### Binary 기반 Table Load

Binary 데이터가 포함된 `TextAsset`을 이용하여 Table을 로드할 수 있습니다.

```csharp
await Tables.LoadAsync<TextAsset>("tables");
```

---

## Table Workflow

전체적인 Table 데이터 작업 흐름은 다음과 같습니다.

```text
Excel
  │
  ▼
Table Converter
  │
  ├─ Delete UnusedFiles
  |
  ├─ C# Code Generation
  │    ├─ TableEnum.cs
  │    └─ Tables.cs
  │
  ├─ Table Asset
  │
  └─ Binary Data
        │
        ▼
     Tables.LoadAsync()
```

Excel 데이터를 수정한 후 Table Converter를 실행하면 **불필요 파일삭제 → 필요한 C# 코드 생성 → Table Asset 생성** 과정이 자동으로 수행됩니다.

---

# 4. UI

UI 관리 및 프리팹 기반 컨트롤 자동 바인딩 시스템을 제공합니다.

## UI Manager

게임 내 UI의 생성, 표시, 숨김 및 레이어 관리를 담당하는 UI 관리 시스템입니다.

### 주요 기능

* **Camera Stacking 자동 구성**

  * `MainCamera`
  * `WorldUICamera`
  * `UICamera`
    를 기반으로 UI Camera Stack을 자동 구성
  * 씬 전환 시 Camera Stack을 동적으로 재구성

* **UILayer 기반 UI 관리**

  UI를 다음과 같은 계층으로 관리합니다.

  ```text
  Layer
   └─ World
   └─ UI
      └─ HUD
      └─ View
      └─ Popup
      └─ Top
      └─ System
      └─ Transition
  ```

* **비동기 UI 생성**

  * `AssetManager`와 연동하여 UI Prefab을 비동기로 로드
  * `ShowUI<T>`를 통한 UI 생성 및 표시

* **EventSystem 자동 구성**

  * 씬에 EventSystem이 없는 경우 필요한 EventSystem을 자동 생성
  * `InputSystemUIInputModule`을 사용하여 Unity Input System 기반 UI 입력 지원

### 사용 예시

```csharp
var viewData = new InventoryUIData();

var inventoryView =
    await UIManager.Instance.ShowUI<InventoryView>(
        "UI/InventoryView",
        UILayer.View,
        viewData
    );

UIManager.Instance.Hide(inventoryView);
```

## UI Prefab Convert

UI 작업자가 제작한 원본 프리팹을 분석하여 컨트롤 바인딩용 C# 스크립트를 자동 생성하고, 생성된 스크립트를 연결한 런타임용 프리팹을 생성합니다.

원본 프리팹과 변환된 프리팹의 역할을 분리하여, UI 작업자는 원본 프리팹을 자유롭게 수정하고 게임에서는 변환된 프리팹을 로딩하여 사용할 수 있습니다.

![프리팹 컨버터](./Images/image_prefab_convert.png)

### Workflow

```text
Origin Prefab
  │
  ▼
UI Prefab Convert
  │
  ├─ Analyze Prefab
  │
  ├─ C# Code Generation
  │    └─ UI Binding Script
  │
  └─ Convert Prefab
        │
        ├─ Copy Origin Prefab
        ├─ Attach Generated Script
        └─ Bind UI Controls
              │
              ▼
        Converted Prefab
              │
              ▼
          Runtime Load
```

### Prefab Structure & Base Type

- **Origin Prefab** — UI 작업자가 제작하고 수정하는 원본 프리팹입니다. UI 레이아웃과 컨트롤 구성을 관리합니다.
- **Converted Prefab** — 원본 프리팹을 기반으로 생성된 런타임용 프리팹입니다. 자동 생성된 바인딩 스크립트가 연결되어 있으며, 실제 앱에서는 이 프리팹을 로딩하여 사용합니다.
- **Base Type** — 프리팹 변환 시 생성되는 바인딩 스크립트의 Base Type을 지정합니다. UI 종류에 따라 적절한 기반 클래스를 상속하도록 설정하여 프로젝트의 공통 UI 기능과 구조를 적용할 수 있습니다.

원본 프리팹을 수정한 경우 변환 작업을 다시 수행하여 변경된 구조와 바인딩 정보를 반영합니다.

### Automatic Binding Code Generation

![프리팹 컨버터 1](./Images/image_prefab_result_1.png)
![프리팹 컨버터 2](./Images/image_prefab_result_2.png)

컨트롤 이름에 등록된 prefix를 기반으로 바인딩 필드와 연결 코드를 자동 생성합니다.

예를 들어 다음과 같은 이름의 컨트롤이 있는 경우

- `trs_root`
- `txt_title`
- `img_icon`
- `sld_volume`

등록된 바인딩 규칙에 따라 해당 컨트롤을 적절한 타입의 필드로 생성하고, 변환된 프리팹에서 실제 오브젝트와 연결합니다.

#### Default Prefix

| Prefix | Purpose |
| --- | --- |
| `trs_` | Transform 및 RectTransform 참조 |
| `txt_` | 텍스트 컨트롤 |
| `img_` | 이미지 컨트롤 |
| `sld_` | 슬라이더 컨트롤 |
| `grp_` | 하위 컨트롤을 중첩 클래스 구조로 바인딩 |
| `list_` | 하위 컨트롤을 리스트 형태로 바인딩 |

#### Nested Group Binding

`grp_` prefix는 하위 컨트롤을 중첩된 클래스(Nested Class) 구조로 생성합니다.

그룹 내부의 컨트롤을 별도의 클래스로 구성하여 논리적인 단위로 묶고, 생성된 바인딩 구조를 통해 각 컨트롤에 접근할 수 있습니다.

#### List Binding

`list_` prefix는 하위 컨트롤을 `List<T>` 형태로 바인딩합니다.

제네릭 타입 `T`는 **첫 번째 자식 컨트롤의 바인딩 타입**을 기준으로 결정됩니다. 따라서 첫 번째 자식 컨트롤이 리스트의 타입을 결정하는 기준이 됩니다.

동일한 구조의 컨트롤을 개별 필드로 선언하지 않고 리스트 형태로 관리할 수 있습니다.

### Prefab Convert Settings

`Project Settings > ClimbGames > UI`에서 프리팹 변환 경로와 바인딩 규칙을 설정할 수 있습니다.

![프리팹 컨버터](./Images/image_prefab_settings.png)

사용자는 기본 규칙 외에도 프로젝트에서 사용하는 prefix와 컴포넌트 타입을 추가로 등록할 수 있습니다.

각 Schema에서 다음 항목을 설정합니다.

- **Prefix** — 바인딩 대상 컨트롤을 식별하는 이름 규칙
- **Options: Include Children** — 하위 오브젝트를 바인딩 대상으로 포함할지 설정합니다.
- **Options: Fallback** — 기본 타입을 찾을 수 없을 때 대체 타입을 사용할지 설정합니다.
- **Type** — 해당 prefix에 연결할 컴포넌트 타입
- **Fallback Types** — Fallback을 활성화했을 때 사용할 대체 타입 목록

타입 선택 기능을 통해 원하는 컴포넌트를 지정할 수 있으며, 필요한 경우 여러 Fallback 타입을 등록할 수 있습니다.
