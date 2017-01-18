using System;

using CSGL.GLFW;
using CSGL.Vulkan;
using static CSGL.GLFW.Unmanaged.GLFW_vk_native;

namespace CSGL.GLFW.Unmanaged {
    public static class GLFW_VK {
        public static bool VulkanSupported() {
            var result = glfwVulkanSupported();
            return result;
        }

        public static string[] GetRequiredInstanceExceptions() {
            string[] result;
            unsafe
            {
                uint count;
                byte** strings = glfwGetRequiredInstanceExtensions(out count);

                if (strings == null) {
                    result = null;
                } else {
                    result = new string[count];
                    for (int i = 0; i < count; i++) {
                        result[i] = Interop.GetString(strings[i]);
                    }
                }
            }

            return result;
        }

        public static IntPtr GetInstanceProcAddress(VkInstance instance, string proc) {
            return glfwGetInstanceProcAddress(instance, proc);
        }

        public static bool GetPhysicalDevicePresentationSupport(VkInstance instance, VkPhysicalDevice device, uint queueFamily) {
            return glfwGetPhysicalDevicePresentationSupport(instance, device, queueFamily);
        }

        public static VkResult CreateWindowSurface(VkInstance instance, WindowPtr ptr, IntPtr alloc, out VkSurfaceKHR surface) {
            return glfwCreateWindowSurface(instance, ptr, alloc, out surface);
        }
    }
}
