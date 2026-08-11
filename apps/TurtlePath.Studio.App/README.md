# TurtlePath Studio App

TurtlePath Studio App is the first desktop host for the reusable TurtlePath Studio UI.

The current host is Windows-only and uses MAUI Blazor Hybrid. The core Studio workflow is intentionally split across reusable projects so another host can reuse the same contracts, application use cases, infrastructure, and Razor UI.

## Publish

Use the Windows folder publish profile:

```powershell
dotnet publish apps/TurtlePath.Studio.App/TurtlePath.Studio.App.csproj `
  -f net10.0-windows10.0.19041.0 `
  -p:PublishProfile=win-x64-folder
```

The app is configured as an unpackaged Windows executable with `WindowsPackageType=None`.
