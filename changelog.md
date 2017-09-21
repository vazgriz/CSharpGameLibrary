# Changelog

## [Unreleased]
### Removed
- Vulkan
  - Unmanaged
    - VK
      - Remove extension commands
      - Remove Load<T>(ref T) method
  - VkCommandBuffer
    - Remove WaitEvents overload
      
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
- GLFW
  - Use IList<T> instead of T[]
  - Window
    - Fix PathDrop callback
- Native
  - Value returns `ref T`
- NativeArray
  - Indexer returns `ref T`