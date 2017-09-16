﻿using System;
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

        public static void Load<T>(ref T del, VkInstance instance) {
            var command = GetCommand<T>();
            var native = Interop.GetUTF8(command);
            IntPtr ptr = VK.GetInstanceProcAddr(instance, native);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del) {
            Load(ref del, VkInstance.Null);
        }

        public static void Load<T>(ref T del, VkDevice device) {
            var command = GetCommand<T>();
            var native = Interop.GetUTF8(command);
            IntPtr ptr = VK.GetDeviceProcAddr(device, native);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del, Instance instance) {
            Load(ref del, instance.Native);
        }

        public static void Load<T>(ref T del, Device device) {
            var command = GetCommand<T>();
            IntPtr ptr = device.GetProcAdddress(command);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }
    }
}
