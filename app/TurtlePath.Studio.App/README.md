# TurtlePath Studio App

TurtlePath Studio App is the first desktop host for the reusable TurtlePath Studio UI.

The current host is Windows-only and uses .NET MAUI. The core Studio workflow is intentionally split across reusable projects so another host can reuse the same contracts, application use cases, and infrastructure.

## Updates

Studio can update itself from a public manifest URL. The default manifest is published with GitHub Releases:

```text
https://github.com/mape1402/turtlepath/releases/latest/download/studio.manifest.json
```

The update source is configurable from `Environment > Studio updates`, so a team can point Studio to a public CDN, storage bucket, or any reachable static manifest endpoint.

The release workflow injects the app version from `.studio.release` and publishes both:

- `TurtlePath.Studio.win-x64.zip`
- `studio.manifest.json`

`studio.manifest.json` contains the latest semantic version, release notes URL, package URL, SHA256 hash, and package size.

## Publish

Use the Windows folder publish profile:

```powershell
dotnet publish app/TurtlePath.Studio.App/TurtlePath.Studio.App.csproj `
  -f net10.0-windows10.0.19041.0 `
  -p:PublishProfile=win-x64-folder
```

The app is configured as an unpackaged Windows executable with `WindowsPackageType=None`.
