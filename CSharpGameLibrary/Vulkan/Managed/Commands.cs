using System;
using System.Reflection;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public partial class Device {
        public class DeviceCommands {
            public vkGetDeviceQueueDelegate getDeviceQueue = null;

            public DeviceCommands(Device device) {
                Type t = typeof(DeviceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string name = fields[i].FieldType.Name;
                    string command = name.Substring(0, name.Length - 8); //"Delegate".Length == 8
                    IntPtr ptr = device.GetProcAdddress(command);
                    fields[i].SetValue(this, Marshal.GetDelegateForFunctionPointer(ptr, fields[i].FieldType));
                }
            }
        }
    }

    public partial class Instance {
        public class InstanceCommands {
            public vkCreateDeviceDelegate createDevice = null;
            public vkDestroyDeviceDelegate destroyDevice = null;
            public vkGetDeviceProcAddrDelegate getDeviceProcAddr = null;

            public vkEnumeratePhysicalDevicesDelegate enumeratePhysicalDevices = null;
            public vkGetPhysicalDevicePropertiesDelegate getPhysicalDeviceProperties = null;

            public vkCreateSwapchainKHRDelegate createSwapchain = null;
            public vkDestroySwapchainKHRDelegate destroySwapchain = null;

            public vkDestroySurfaceKHRDelegate destroySurface = null;
            public vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities = null;
            public vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats = null;
            public vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes = null;

            public InstanceCommands(Instance instance) {
                Type t = typeof(InstanceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string name = fields[i].FieldType.Name;
                    string command = name.Substring(0, name.Length - 8); //"Delegate".Length == 8
                    IntPtr ptr = instance.GetProcAddress(command);
                    fields[i].SetValue(this, Marshal.GetDelegateForFunctionPointer(ptr, fields[i].FieldType));
                }
            }
        }
    }
}
