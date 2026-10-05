# TurtlePath Studio

TurtlePath Studio is distributed as the `TurtlePath.Studio` NuGet package.

The current Studio release is `1.0.12`, aligned with TurtlePath `1.9.1`, TurtlePath.Template `1.9.1`, Heroes Showcase `1.6.1`, and TurtlePath.Template.Documentation `1.5.1`.

Install it with the companion tool:

```powershell
dotnet tool install --global TurtlePath.Studio.Tool
turtlepath-studio install
```

The Studio checks NuGet for the latest `TurtlePath.Studio` version and updates its self-contained Windows x64 payload from the package. No GitHub release asset, update manifest, or configurable manifest URL is required.
