using System;
using System.Reflection;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public partial class Device {
        public class DeviceCommands {
            public vkDeviceWaitIdleDelegate waitDeviceIdle;

            public vkGetDeviceQueueDelegate getDeviceQueue;

            public vkCreateSwapchainKHRDelegate createSwapchain;
            public vkDestroySwapchainKHRDelegate destroySwapchain;
            public vkGetSwapchainImagesKHRDelegate getSwapchainImages;

            public vkCreateImageViewDelegate createImageView;
            public vkDestroyImageViewDelegate destroyImageView;

            public vkCreateImageDelegate createImage;
            public vkDestroyImageDelegate destroyImage;

            public vkCreateShaderModuleDelegate createShaderModule;
            public vkDestroyShaderModuleDelegate destroyShaderModule;

            public vkCreateGraphicsPipelinesDelegate createGraphicsPiplines;
            public vkCreateComputePipelinesDelegate createComputePipelines;
            public vkDestroyPipelineDelegate destroyPipeline;

            public vkCreatePipelineLayoutDelegate createPipelineLayout;
            public vkDestroyPipelineLayoutDelegate destroyPipelineLayout;

            public vkCreatePipelineCacheDelegate createPipelineCache;
            public vkDestroyPipelineCacheDelegate destroyPipelineDestroy;

            public vkCreateRenderPassDelegate createRenderPass;
            public vkDestroyRenderPassDelegate destroyRenderPass;

            public vkCreateFramebufferDelegate createFramebuffer;
            public vkDestroyFramebufferDelegate destroyFramebuffer;

            public vkCreateSemaphoreDelegate createSemaphore;
            public vkDestroySemaphoreDelegate destroySemaphore;

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
            public vkCreateDeviceDelegate createDevice;
            public vkDestroyDeviceDelegate destroyDevice;
            public vkGetDeviceProcAddrDelegate getDeviceProcAddr;

            public vkGetPhysicalDevicePropertiesDelegate getProperties;
            public vkGetPhysicalDeviceQueueFamilyPropertiesDelegate getQueueFamilyProperties;
            public vkGetPhysicalDeviceFeaturesDelegate getFeatures;
            public vkEnumerateDeviceExtensionPropertiesDelegate getExtensions;
            public vkGetPhysicalDeviceSurfaceSupportKHRDelegate getPresentationSupport;

            public vkEnumeratePhysicalDevicesDelegate enumeratePhysicalDevices;
            public vkGetPhysicalDevicePropertiesDelegate getPhysicalDeviceProperties;

            public vkDestroySurfaceKHRDelegate destroySurface;
            public vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities;
            public vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats;
            public vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes;

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
