# Changelog

## [Unreleased]
### Changed
- Vulkan
  - Move all types in `structs.cs` to the `CSGL.Vulkan.Unmanaged` namespace
  - Move `Vulkan` class to `CSGL.Vulkan.Unmanaged`
  - Add "Vk" to all class names
  - Remove extension commands from `Unmanaged.VK`
  - VkDebugReportCallback
    - Change `Callback` property to an event
  - VkShaderModule
    - Add Device property
  - VkQueue
    - Add Native property