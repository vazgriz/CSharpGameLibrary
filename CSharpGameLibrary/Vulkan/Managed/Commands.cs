using System;
using System.Reflection;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public partial class Device {
        public class DeviceCommands {
            public vkDeviceWaitIdleDelegate waitDeviceIdle = null;

            public vkGetDeviceQueueDelegate getDeviceQueue = null;

            public vkCreateSwapchainKHRDelegate createSwapchain = null;
            public vkDestroySwapchainKHRDelegate destroySwapchain = null;
            public vkGetSwapchainImagesKHRDelegate getSwapchainImages = null;

            public vkCreateImageViewDelegate createImageView = null;
            public vkDestroyImageViewDelegate destroyImageView = null;

            public vkCreateImageDelegate createImage = null;
            public vkDestroyImageDelegate destroyImage = null;

            public vkCreateShaderModuleDelegate createShaderModule = null;
            public vkDestroyShaderModuleDelegate destroyShaderModule = null;

            public vkCreateGraphicsPipelinesDelegate createGraphicsPiplines = null;
            public vkCreateComputePipelinesDelegate createComputePipelines = null;
            public vkDestroyPipelineDelegate destroyPipeline = null;

            public vkCreatePipelineLayoutDelegate createPipelineLayout = null;
            public vkDestroyPipelineLayoutDelegate destroyPipelineLayout = null;

            public vkCreatePipelineCacheDelegate createPipelineCache = null;
            public vkDestroyPipelineCacheDelegate destroyPipelineDestroy = null;

            public vkCreateRenderPassDelegate createRenderPass = null;
            public vkDestroyRenderPassDelegate destroyRenderPass = null;

            internal DeviceCommands(Device device) {
                Type t = typeof(DeviceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string command = Vulkan.GetCommand(fields[i].FieldType);
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

            public vkGetPhysicalDevicePropertiesDelegate getProperties = null;
            public vkGetPhysicalDeviceQueueFamilyPropertiesDelegate getQueueFamilyProperties = null;
            public vkGetPhysicalDeviceFeaturesDelegate getFeatures = null;
            public vkEnumerateDeviceExtensionPropertiesDelegate getExtensions = null;
            public vkGetPhysicalDeviceSurfaceSupportKHRDelegate getPresentationSupport = null;

            public vkEnumeratePhysicalDevicesDelegate enumeratePhysicalDevices = null;
            public vkGetPhysicalDevicePropertiesDelegate getPhysicalDeviceProperties = null;

            public vkDestroySurfaceKHRDelegate destroySurface = null;
            public vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities = null;
            public vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats = null;
            public vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes = null;

            internal InstanceCommands(Instance instance) {
                Type t = typeof(InstanceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string command = Vulkan.GetCommand(fields[i].FieldType);
                    IntPtr ptr = instance.GetProcAddress(command);
                    fields[i].SetValue(this, Marshal.GetDelegateForFunctionPointer(ptr, fields[i].FieldType));
                }
            }
        }
    }
}
