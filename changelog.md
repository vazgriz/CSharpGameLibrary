# Changelog

## [Unreleased]
### Removed
- Vulkan
  - Unmanaged
    - VK
      - Remove extension commands
      - Remove Load<T>(ref T) method
      
### Changed
- Vulkan
  - Move all types in `structs.cs` to the `CSGL.Vulkan.Unmanaged` namespace
  - Move `Vulkan` class to `CSGL.Vulkan.Unmanaged`
  - Add "Vk" to all class names
  - Unmanaged
    - VK
      - Add Load(VkDevice, string) and Load(VkInstance, string) methods
  - VkDebugReportCallback
    - Change `Callback` property to an event
  - VkShaderModule
    - Add Device property
  - VkQueue
    - Add Native property