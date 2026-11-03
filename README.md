# Nexus

Nexus is an external-PC server platform and player Launchpad for **Space Engineers 1** and **Space Engineers 2**.

## Project goals
- Run Nexus Server on a separate Windows or Linux PC.
- Provide a Windows Launchpad that detects local Space Engineers installs.
- Discover and connect to Nexus-enabled servers.
- Expose one shared plugin API for C#, Lua, and JavaScript.
- Keep SE1 and SE2 integration in separate adapters.

## Repository status
This branch establishes the first buildable architecture: shared contracts, read-only Steam installation discovery, separate game adapter projects, and a minimal ASP.NET Core host. Actual dedicated-server lifecycle integration, authenticated remote control, server listing, and language runtimes are subsequent milestones. Adapter scaffolds do not claim live game compatibility yet.

## Requirements
- .NET 10 SDK
- Windows 10/11 for the initial desktop Launchpad
- Windows or Linux for the server host

## Build
```bash
dotnet restore Nexus.sln
dotnet build Nexus.sln --configuration Release
dotnet run --project src/Nexus.Server
```

Endpoints:
- `GET /health`
- `GET /api/v1/games/installed`

The detection endpoint reports installations on the machine running the server. Launchpad will use the same detector locally.

## Architecture
```
Nexus.Launchpad (Windows)
       | HTTPS + authenticated connection (planned)
       v
Nexus.Server (Windows/Linux)
  |-- Nexus.Contracts
  |-- Nexus.GameDetection
  |-- Nexus.GameAdapters.Abstractions
  |     |-- Nexus.GameAdapters.SE1
  |     `-- Nexus.GameAdapters.SE2
  `-- Nexus.PluginRuntime (planned: C#, Lua, JavaScript)
```

## Security
- Do not expose administrative APIs publicly without TLS and authentication.
- Treat third-party plugins as untrusted code.
- Do not execute arbitrary shell commands from remote requests.
- Grant plugins explicit capabilities and isolate runtimes.
- Keep secrets out of source control.

## Compatibility note
SE1 and SE2 have distinct game and modding interfaces. Nexus uses separate adapters and will validate each against supported game/server builds before enabling management features.
