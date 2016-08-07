using System;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public static class Vulkan {
        public static void Init() {
            Instance.Init();
        }

        public static void Load<T>(ref T del) {
            string name = typeof(T).Name;
            string command = name.Substring(0, name.Length - 8);
            IntPtr ptr = GLFW.GLFW.GetInstanceProcAddress(VkInstance.Null, command);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
    }
}
