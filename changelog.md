# Changelog

## Unreleased
### Added
- GLFW
  - Window
    - Add `SetPosition` method

### Changed
- GLFW
  - Monitor
    - Change `Width` to `PhysicalWidth`
    - Change `Height` to `PhysicalHeight`

## [0.1.6] - 2017-10-1
### Added
- NativeStringArray
  - Add getter for indexer
  
### Removed
- Vulkan
  - Unmanaged
    - VK
      - Remove extension commands
      - Remove `Load<T>(ref T)` method
  - VkCommandBuffer
    - Remove `WaitEvents` overload
      
### Changed
- Vulkan
  - Move all types in structs.cs to the `CSGL.Vulkan.Unmanaged` namespace
  - Merge `Vulkan` class with `CSGL.Vulkan.Unmanaged.VK`
  - Add "Vk" to all class names
  - Make interface more C# idiomatic
  - Unmanaged
    - VK
      - Add `Load(VkDevice, string)` and `Load(VkInstance, string)` methods
  - VkInstance
    - Change type of `Layers` property to `IList<VkLayer>`
    - Change type of `Extensions` property to `IList<VkExtension>`
  - VkDevice
    - Change type of Extensions property to `IList<VkExtension>`
  - VkDebugReportCallback
    - Change `Callback` property to an event
  - VkShaderModule
    - Add `Device` property
  - VkQueue
    - Add `Native` property
  - VkEvent
    - Make `GetStatus` method into `Status` property
- GLFW
  - Use `IList<T>` instead of `T[]`
  - Window
    - Fix `PathDrop` callback
- Native
  - `Value` property returns `ref T`
- NativeArray
  - Indexer returns `ref T`