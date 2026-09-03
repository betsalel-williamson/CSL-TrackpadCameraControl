# Rewrite tests

Tier A golden Maps+ fixtures and light tier B mapper/session coverage for
`TrackpadCameraControl.Rewrite`. See [Harnesses and testing](../docs/developer/harnesses-and-testing.md)
and greenfield redesign lessons L10.

```bash
dotnet test rewrite/tests/TrackpadCameraControl.Rewrite.Tests/TrackpadCameraControl.Rewrite.Tests.csproj
```

Does **not** cover AppKit hardware sampling or Harmony postfix order (tier C).
