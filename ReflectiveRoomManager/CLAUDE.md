# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Reflective Room Manager — modular networked room management system for Unity built on top of [Mirror](https://github.com/MirrorNetworking/Mirror). Manages multiplayer rooms, additive scene loading, per-room physics isolation, and player lifecycle.

**Unity 6000.0.40f1** · **Mirror Networking** · **MIT License**

## Build & Development

This is a Unity project — no CLI build/test commands. Open in Unity Editor, use standard Unity build pipeline.

**Assembly definitions:**
- `ReflectiveRM.Runtime` — all runtime code
- `ReflectiveRM.Editor` — custom inspectors, collision visualizers
- `Examples` — example game implementations

## Architecture

### Core Flow

```
ReflectiveNetworkManager (extends Mirror.NetworkManager)
    └── ReflectiveConnectionManager (static facade)
            ├── NetworkConnections (Mirror lifecycle events)
            └── RoomConnections (room-specific events + message handlers)

RoomManagerBase (abstract, partial class, singleton)
    └── RoomManager (concrete: CreateRoom, JoinRoom, ExitRoom, RemoveRoom)
            └── Room (ID, Name, Scene, Connections, CustomData)
```

### Key Source Paths

All under `Assets/ReflectiveRoomManager/Scripts/Runtime/`:

| Path | Purpose |
|------|---------|
| `NETWORK/Manager/` | `ReflectiveNetworkManager` — Mirror extension, auto-loads prefabs from `Resources/SpawnablePrefabs` |
| `NETWORK/Room/Managers/` | `RoomManagerBase` (split into 5 partial files: Variables, Room, Server, Callback, Initialization) + `RoomManager` |
| `NETWORK/Room/Structs/` | Network messages: `RoomInfo`, `ServerRoomMessage`, `ClientRoomMessage`, `SceneLoadMessage` |
| `NETWORK/Room/Loader/` | `IRoomLoader` interface, `SceneRoomLoader` impl |
| `NETWORK/Room/Scene/` | `RoomSceneSynchronizer`, `RoomSceneChanger` — scene sync to clients |
| `NETWORK/Room/Events/` | `RoomEventManager` — server-side room lifecycle events |
| `NETWORK/Connection/` | `ConnectionEvent<T>` generic event system, connection managers |
| `NETWORK/Player/Utilities/` | `PlayerCreatorUtilities` (spawn/replace/remove), `PlayerMoveUtilities` (cross-scene transfer) |
| `Container/` | `RoomContainer` with `SingletonContainer` + `ListenerContainer` per room |
| `Singleton/` | `RoomSingleton<T>` — auto-registering NetworkBehaviour base for per-room singletons |
| `SceneManagement/` | `ReflectiveSceneManager`, processors (Additive/Single), async loading with physics isolation |
| `Physic/` | `PhysicSimulator` (2D/3D factory), modular collision system |

### Room Loading Strategies

Configured via `RoomLoaderType` enum on `RoomManagerBase`:
- **NoneScene** — no scene management
- **SingleScene** — single scene reload
- **AdditiveScene** — additive scene loading with physics isolation

### Client-Server Communication Flow

1. Client requests room list → Server sends `RoomListChangeMessage` (Add/Update/Remove)
2. Client sends `ClientRoomMessage` (Create/Join/Exit) → Server validates
3. Server loads scene via `IRoomLoader` → sends `SceneLoadMessage` to client (container scene + room scene)
4. Client loads scenes → Server sends `ClientRoomIDMessage` → player spawned via `PlayerCreatorUtilities`

### Design Patterns

- **Partial classes** — `RoomManagerBase` split by concern (Variables, Room, Server, Callback, Initialization)
- **Factory** — `PhysicSimulatorFactory`, `SceneProcessorFactory`
- **Generic events** — `ConnectionEvent<T>` with 0-2 type params
- **Static facades** — `ReflectiveConnectionManager`, `RoomContainer`
- **Auto-registering singletons** — `RoomSingleton<T>` registers in `RoomContainer` on Awake

## Dependencies

- **Mirror** (required) — networking foundation
- **DOTween** (examples only) — space shooter example
- **Post-Processing** (examples only) — space shooter example
