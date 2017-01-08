using System;

using CSGL.GLFW;
using CSGL.Vulkan;
using static CSGL.GLFW.Unmanaged.GLFW_vk_native;

namespace CSGL.GLFW {
    public static class GLFW_VK {
        //this depends on the error handling setup in the main GLFW wrapper

        public static bool VulkanSupported() {
            var result = glfwVulkanSupported();
            GLFW.CheckError();
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
            GLFW.CheckError();
            return result;
        }

        public static IntPtr GetInstanceProcAddress(VkInstance instance, string proc) {
            var result = glfwGetInstanceProcAddress(instance, proc);
            GLFW.CheckError();
            return result;
        }

        public static bool GetPhysicalDevicePresentationSupport(VkInstance instance, VkPhysicalDevice device, uint queueFamily) {
            var result = glfwGetPhysicalDevicePresentationSupport(instance, device, queueFamily);
            GLFW.CheckError();
            return result;
        }

        public static VkResult CreateWindowSurface(VkInstance instance, WindowPtr ptr, IntPtr alloc, out VkSurfaceKHR surface) {
            var result = glfwCreateWindowSurface(instance, ptr, alloc, out surface);
            GLFW.CheckError();
            return result;
        }
    }
}
