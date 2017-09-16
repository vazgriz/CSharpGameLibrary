# Changelog

## [Unreleased]
### Changed
- Vulkan
  - Move all types in `structs.cs` to the `CSGL.Vulkan.Unmanaged` namespace
  - Move `Vulkan` class to `CSGL.Vulkan.Unmanaged`
  - Add "Vk" to all class names
  - Remove extension commands from `Unmanaged.VK`
  - DebugReportCallback
    - Change `Callback` property to an event
  - ShaderModule
    - Add Device property
  - Queue
    - Add Native property