<!--
================================================================================
UNIVERSAL AI CONTEXT — Read this first when entering this project.
Every AI agent, in any tool, should be able to understand the project completely
from this one file and start contributing immediately.
================================================================================
-->

# Clean State Machine — Unity Project

## Project Overview
- A visual state machine editor and runtime system for Unity, delivered as an embedded UPM package (`com.cleanstatemachine`)
- Editor tool provides a graph-based canvas with nodes (states), edges (transitions), groups, blackboard variables, behaviours, and conditions
- Runtime executes the graph via `StateMachineComponent`, a MonoBehaviour you attach to any GameObject
- Unity 6000.0+ required; no external package dependencies; MIT licensed
- The project root is a standard Unity project shell that **only exists to host and test the package** — the actual product is the package itself

## Scope / What to Focus On
- **Only the package folder matters**: `Packages/com.cleanstatemachine/`
- **Everything else is a standard Unity project wrapper** (Assets, ProjectSettings, Library, Temp, etc.) — ignore it for code contributions unless you need demo scenes, sample assets, or build settings
- The `README.md` at the project root is documentation for end users of the package

## File & Folder Structure

### Package root: `Packages/com.cleanstatemachine/`

```
package.json                  — UPM package manifest (name, version, unity 6000.0)
Runtime/                      — Runtime C# code (assembly: CleanStateMachine.Runtime)
  CleanStateMachine.Runtime.asmdef
  StateMachineController.cs   — ScriptableObject asset that stores the entire graph (serialized data + sub-assets)
  StateMachineData.cs          — Serializable data models: StateData, ConnectionData, GroupData, SerializableData, ConditionEntry
  StateMachineComponent.cs     — MonoBehaviour runtime executor; manages state transitions, behaviour lifecycle, blackboard
  StateBehaviour.cs            — Abstract ScriptableObject base class for per-state logic (OnStateEnter/Update/Exit)
  ConditionScript.cs           — Abstract ScriptableObject base class for transition conditions (Evaluate)
  StateMachineAction.cs        — Abstract MonoBehaviour for external actions that talk to the state machine via blackboard
  BlackboardVariable.cs        — Typed variable class (Bool, Int, Float, String, Vector2, Vector3) with Clone()
  BlackboardVariableReference.cs — Utility for behaviour/condition fields that toggle between direct value and blackboard variable
  Examples/                    — Empty (placeholder for example behaviours)

Editor/                        — Editor-only C# code (assembly: CleanStateMachine.Editor, references Runtime)
  CleanStateMachine.Editor.asmdef
  CleanStateMachineWindow.cs   — Main EditorWindow (Tools > CleanStateMachine); owns all controllers, modules, UI layers
  GraphSerializer.cs           — Save/Load/New/SaveAs operations; serializes StateView/ConnectionView ↔ StateData/ConnectionData
  GraphOperations.cs           — All graph mutations: create/delete states/connections/groups, copy/paste, ensure entry state
  GraphInputHandler.cs         — Mouse & keyboard input routing for the graph canvas (click, drag, selection, connecting)
  GraphPanController.cs        — Right-click/middle-mouse pan and scroll-wheel zoom
  GraphContextMenu.cs          — Right-click context menu builder; dispatches actions back to GraphOperations
  DragController.cs            — Drag-to-move selected states and groups
  SelectionBox.cs              — Rubber-band selection rectangle
  SelectionController.cs       — Multi-selection tracking (ISelectable); fires SelectionChanged event
  ConnectionController.cs      — "Press C to connect" flow: source selection, ghost line rendering, target click
  ConnectionView.cs            — Visual representation of a transition edge between two states (offset management for parallel edges)
  ConnectionArrowsLayer.cs     — Renders all connection arrows via a pooled set of VisualElements
  CommentGroupView.cs          — Visual group node (colored box + label) that parents multiple StateViews
  StateView.cs                 — Visual state node (VisualElement) with name label, entry/sub-entry badges, selection highlight
  DetailsPanel.cs              — Right-side inspector for selected state/connection/group (behaviour assignment, condition editing, group color)
  BlackboardPanel.cs           — Right-side panel section for creating/renaming/deleting/editing blackboard variables
  SidePanel.cs                 — Container that hosts DetailsPanel + BlackboardPanel with resize splitter
  ExpandedViewManager.cs       — Sub-state-machine drill-down: push/pop expanded sub-state context, breadcrumb bar
  PlayModeTracker.cs           — Runtime tracking during play mode: auto-follow active state, highlight active connections
  GraphViewAnimator.cs         — Smooth animated pan/zoom transitions (focus on content, auto-navigate on state change)
  GridBackground.cs            — Grid-background VisualElement with dynamic grid rendering
  ISelectable.cs               — Interface for graph items (StateView, ConnectionView, CommentGroupView)
  IContextMenuProvider.cs      — Public extension point: implement to add custom items to the graph's right-click menu
  MenuDropdown.cs              — Reusable dropdown menu UI toolkit component (used by context menu and extension system)
  ScriptReferenceUtility.cs    — Utility to find MonoScript by type name, load USS stylesheets from package paths
  StateMachineControllerEditor.cs — Custom Inspector for StateMachineController assets (open editor button)
  StateMachineComponentEditor.cs  — Custom Inspector for StateMachineComponent (data summary, reset button)
  StateMachineActionEditor.cs     — Custom Inspector for StateMachineAction derivatives (blackboard variable assignment)
  StateMachineAssetHandler.cs     — AssetPostprocessor for double-click .asset opening; Create menu for controller assets
  ScriptTemplates/                — .txt templates + creation wizard for "Assets > Create > Clean State Machine" menu items

  UndoRedo/                    — Command-pattern undo/redo system
    IUndoableCommand.cs         — Interface: Execute(), Undo(), Redo(), Description
    UndoRedoSystem.cs           — Dual-stack manager (max 50), fires HistoryChanged event
    CompositeCommand.cs         — Groups multiple commands into one atomic undo step
    CreateStateCommand.cs       — Undoable creation of a StateView + backing StateData
    DeleteStatesCommand.cs      — Undoable deletion of one or more states (saves connections/groups for undo)
    CreateConnectionCommand.cs  — Undoable connection between two states
    DeleteConnectionCommand.cs  — Undoable connection deletion
    MoveStatesCommand.cs        — Undoable position change for dragged states
    RenameStateCommand.cs       — Undoable state rename
    CreateGroupCommand.cs       — Undoable comment group creation
    UngroupCommand.cs           — Undoable ungroup (removes group, keeps states)
    RenameGroupCommand.cs       — Undoable group header rename
    ModifyGroupColorCommand.cs  — Undoable group color change
    ResizeGroupCommand.cs       — Undoable group resize (drag corner)
    DeleteBlackboardVariableCommand.cs — Undoable blackboard variable deletion
    ModifyBlackboardVariableCommand.cs — Undoable blackboard variable edit (rename, type, default value)

  Styles/                      — USS (Unity Style Sheets) for UI Toolkit
    StateView.uss              — State node styling
    CommentGroupView.uss       — Group node styling
    SidePanel.uss              — Right side panel
    MenuDropdown.uss           — Context menu dropdown
    ComponentInspector.uss     — StateMachineComponent inspector
    ControllerInspector.uss    — Controller inspector
    StateMachineActionInspector.uss — Action behaviour inspector

Assets/                        — Shim assembly for user-authored behaviours (assembly: CleanStateMachine.Behaviours, references Runtime)
  CleanStateMachine.Behaviours.asmdef
  ActionBehaviours/            — Example: TriggerEnter_SetBool.cs (sets a blackboard bool on trigger enter)
  ConditionBehaviours/         — Examples: CompareNumbers, SimpleBool, Timer condition scripts
  SateBehaviours/              — Example: DebugLog_StateBehaviour.cs (logs on enter/update/exit)

Demo/                          — Example .asset controller for testing
  NewStateMachineController.asset
```

### Files/Folders to Ignore
- `Assets/` — standard Unity user-content folder; may contain test scenes but is not part of the package product
- `ProjectSettings/`, `Library/`, `Temp/`, `Logs/`, `UserSettings/` — Unity-generated project data; never read these
- `UIElementsSchema/` — auto-generated UI Toolkit schema
- `Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj` — auto-generated C# project files
- `CleanStateMachine.*.csproj` — auto-generated from asmdef files
- `*.slnx` — solution file for IDE
- `.gitignore`, `.cgcignore`, `.vscode/`, `.kilo/` — config and ignore files
- `AGENTS.md` — kilo AI agent instructions (CONTEXT.md auto-updater rule)
- `compile_log.txt` — build log artifact

## Architecture & Patterns

### Namespace
- Everything lives in `namespace CleanStateMachine`

### Three-Assembly Split
- `CleanStateMachine.Runtime` — models, runtime execution, abstract bases (no UnityEditor dependency)
- `CleanStateMachine.Editor` — the graph editor window and all editor UI (references Runtime, Editor-only platform)
- `CleanStateMachine.Behaviours` — shim assembly for user-authored behaviour/condition scripts (separates user code from package internals)

### Data Model (MVC-like)
- `SerializableData` is the single source of truth — holds lists of `StateData`, `ConnectionData`, `GroupData`, `BlackboardVariable` plus viewport state (pan, zoom, panel layout)
- `StateMachineController` is a `ScriptableObject` wrapping `SerializableData`; behaviour/condition instances are stored as **sub-assets** of the controller asset
- The editor's `CleanStateMachineWindow` has parallel view-model lists: `States` (List\<StateView\>), `Connections` (List\<ConnectionView\>), `Groups` (List\<CommentGroupView\>), `BlackboardVariables` (List\<BlackboardVariable\>)
- `GraphSerializer` is the bridge: `SaveCurrentData()` converts views → data, `LoadFromCurrentData()` converts data → views

### Editor Window Architecture
- `CleanStateMachineWindow` is the central hub — it owns all controllers and modules (composition via constructor injection with `this` reference)
- **Controllers** (handle state): `SelectionController`, `UndoRedoSystem`, `GraphPanController`, `DragController`, `SelectionBox`, `ConnectionController`, `GraphContextMenu`
- **Helper Modules** (stateless service objects): `GraphOperations`, `GraphInputHandler`, `ExpandedViewManager`, `GraphSerializer`, `PlayModeTracker`, `GraphViewAnimator`
- **UI Layers** (VisualElements in z-order): `GridBackground` → `GroupContainer` → `ConnectionArrowsLayer` → `StateLayer` → `GraphCanvas` (IMGUI overlay) → `SidePanelElement`
- The main loop runs in `OnGUI()` (IMGUI event processing) + UI Toolkit callbacks (visual tree)
- USS stylesheets are loaded via `ScriptReferenceUtility.LoadStyleSheet()` using `Resources.FindObjectsOfTypeAll` + `AssetDatabase.GetAssetPath` pattern

### State Runtime
- `StateMachineComponent` executes a **path-based** state machine (not flat states): `_activeStatePath` is a `List<int>` from root to leaf, supporting hierarchical sub-state-machines
- Transition evaluation walks from leaf upward (depth-first: leaf → parent → grandparent) checking `ConnectionData` with `ConditionScript.Evaluate()`
- Each behaviour/condition is instantiated at runtime via `ScriptableObject.CreateInstance` with `HideFlags.HideAndDontSave` — avoids leaking sub-asset references
- Blackboard variables are **copied** from the controller at init (`Clone()`), so runtime mutations don't affect the asset
- Events: `OnStateChanged`, `OnStateEntered`, `OnStateExited`

### Undo/Redo
- Classic Command pattern: `IUndoableCommand` interface → concrete commands → `UndoRedoSystem` (two stacks, max 50)
- `CompositeCommand` allows grouping multiple atomic commands into one undo step
- Every graph mutation in `GraphOperations` goes through `UndoRedoSystem.Execute()` — never modifies views directly

### Extensibility
- `IContextMenuProvider`: implement to inject custom items into the graph editor's right-click menu; discovered via `TypeCache.GetTypesDerivedFrom<>` at editor startup
- `StateBehaviour` / `ConditionScript`: publicly inheritable ScriptableObject base classes for user-authored behaviours
- `BlackboardVariableReference`: value-type-agnostic field pattern for behaviour/condition parameters (toggle between direct value and blackboard variable)

### Naming Conventions
- C#: PascalCase everywhere (standard Unity convention)
- Internal fields: `_camelCase` prefix for private instance fields
- Public properties: PascalCase (e.g., `PanOffset`, `CurrentStateIndex`)
- Classes: `Noun` for models, `NounView` for visual elements, `NounController` for interaction controllers, `NounCommand` for undo/redo
- Enums: PascalCase values (e.g., `BlackboardVariableType.Bool`)
- USS files: matched to the VisualElement class they style (e.g., `StateView.uss`)

### Key Files (highest priority to read)
1. `Runtime/StateMachineComponent.cs` — the runtime engine; largest file, core logic
2. `Editor/CleanStateMachineWindow.cs` — the editor window; hub of all editor logic
3. `Runtime/StateMachineData.cs` — data model definition (what gets serialized)
4. `Editor/GraphSerializer.cs` — bridge between data model and editor views
5. `Editor/GraphOperations.cs` — all graph mutations
6. `Runtime/StateMachineController.cs` — asset wrapper with sub-asset management
7. `Editor/UndoRedo/UndoRedoSystem.cs` — undo/redo infrastructure
8. `Runtime/StateBehaviour.cs` + `Runtime/ConditionScript.cs` — public extension base classes
