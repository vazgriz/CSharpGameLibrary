using System;
using System.Runtime.InteropServices;

using CSGL.Input;
using CSGL.Vulkan;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.GLFW.Unmanaged {
    public static unsafe class GLFW_vk_native {
        const string lib = "glfw3";

        [DllImport(lib)]
        public static extern bool glfwVulkanSupported();

        [DllImport(lib)]
        public static extern byte** glfwGetRequiredInstanceExtensions(out uint count);

        [DllImport(lib)]
        public static extern IntPtr glfwGetInstanceProcAddress(VkInstance instance, string procName);

        [DllImport(lib)]
        public static extern bool glfwGetPhysicalDevicePresentationSupport(VkInstance instance, VkPhysicalDevice device, uint queuefamily);

        [DllImport(lib)]
        public static extern VkResult glfwCreateWindowSurface(VkInstance instance, WindowPtr window, IntPtr allocator, out VkSurfaceKHR surface);
    }
}
