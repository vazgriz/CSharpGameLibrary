# Changelog

## [Unreleased]
### Changed
- Vulkan
  - Move all types in `structs.cs` to the `CSGL.Vulkan.Unmanaged` namespace. Move `structs.cs` to `Unmanaged` folder
  - DebugReportCallback
    - Change `Callback` property to an event
  - ShaderModule
    - Add Device property