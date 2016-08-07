using System;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public static class Vulkan {
        public static void Init() {
            Instance.Init();
        }

        public static void Load<T>(ref T del, VkInstance instance) {
            string name = typeof(T).Name;
            string command = name.Substring(0, name.Length - 8);    //"Delegate".Length == 8
            IntPtr ptr = GLFW.GLFW.GetInstanceProcAddress(instance, command);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del) {
            Load(ref del, VkInstance.Null);
        }

        public static void Load<T>(ref T del, Instance instance) {
            Load(ref del, instance.Native);
        }

        public static void Load<T>(ref T del, VkDevice device, vkGetDeviceProcAddrDelegate loader) {
            string name = typeof(T).Name;
            string command = name.Substring(0, name.Length - 8);
            byte[] array = Interop.GetUTF8(command);
            unsafe
            {
                fixed (byte* s = array) {
                    IntPtr ptr = loader(device, s);
                    del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
                }
            }
        }
    }
}
