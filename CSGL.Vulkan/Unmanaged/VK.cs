using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Unmanaged {
    public static partial class VK {
        public static string GetCommand<T>() {
            Type t = typeof(T);
            return GetCommand(t);
        }

        public static string GetCommand(Type t) {
            string name = t.Name;
            return name.Substring(0, name.Length - 8);  //"Delegate".Length == 8
        }

        public static IntPtr Load(VkInstance instance, string commandName) {
            var native = Interop.GetUTF8(commandName);
            return GetInstanceProcAddr(instance, native);
        }

        public static IntPtr Load(VkDevice device, string commandName) {
            var native = Interop.GetUTF8(commandName);
            return GetDeviceProcAddr(device, native);
        }

        public static void Load<T>(ref T del, VkInstance instance) {
            var commandName = GetCommand<T>();
            IntPtr ptr = Load(instance, commandName);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del, VkDevice device) {
            var commandName = GetCommand<T>();
            IntPtr ptr = Load(device, commandName);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del, Vulkan.VkInstance instance) {
            Load(ref del, instance.Native);
        }

        public static void Load<T>(ref T del, Vulkan.VkDevice device) {
            var command = GetCommand<T>();
            IntPtr ptr = device.GetProcAdddress(command);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
    }
}
