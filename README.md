# Nexus

Nexus is an external-PC server platform and player Launchpad for **Space Engineers 1** and **Space Engineers 2**.

## Project goals
- Run Nexus Server on a separate Windows or Linux PC.
- Provide a Windows Launchpad that detects local Space Engineers installs.
- Discover and connect to Nexus-enabled servers.
- Expose one shared plugin API for C#, Lua, and JavaScript.
- Keep SE1 and SE2 integration in separate adapters.

## Current implementation
The foundation includes shared contracts, read-only Steam installation discovery, separate game adapter projects, and an ASP.NET Core host. The Server Manager milestone adds a persistent local registry for server instances. Dedicated-server lifecycle integration, authenticated remote control, server listing, Launchpad UI, and language runtimes remain future milestones.

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

## API
- `GET /health`
- `GET /api/v1/games/installed`
- `GET /api/v1/server-instances`
- `GET /api/v1/server-instances/{id}`
- `POST /api/v1/server-instances`
- `DELETE /api/v1/server-instances/{id}`

Example registration body:
```json
{
  "name": "My SE1 Roleplay Server",
  "edition": "SpaceEngineers1",
  "installationPath": "D:/DedicatedServers/SpaceEngineers",
  "worldPath": "D:/Nexus/Worlds/MyServer"
}
```

The registry is stored under the host user's local application data directory in `Nexus/ServerManager/server-instances.json`. It stores metadata and paths only; it does not copy, modify, or launch game files.

## Security
Management endpoints are intended for local development only in this milestone. Do not expose them to the public internet. Remote administration must add TLS, authentication, authorization, and audit logging first. Third-party plugin runtimes will require explicit capabilities and isolation.

## Compatibility
SE1 and SE2 have distinct game and modding interfaces. Nexus uses separate adapters and will validate each against supported game/server builds before enabling management features. Current adapters validate candidate paths only and do not manage live game servers.
