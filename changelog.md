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
  - Make interface more C# idiomatic
  - Unmanaged
    - VK
      - Add Load(VkDevice, string) and Load(VkInstance, string) methods
  - VkInstance
    - Change type of Layers property to IList<VkLayer>
    - Change type of Extensions property to IList<VkExtension>
  - VkDevice
    - Change type of Extensions property to IList<VkExtension>
  - VkDebugReportCallback
    - Change `Callback` property to an event
  - VkShaderModule
    - Add Device property
  - VkQueue
    - Add Native property