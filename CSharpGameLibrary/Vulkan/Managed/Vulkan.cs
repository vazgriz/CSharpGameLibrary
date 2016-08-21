using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public static class Vulkan {
        public static void Init() {
            Instance.Init();
        }

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
            IntPtr ptr = GLFW.GLFW.GetInstanceProcAddress(instance, command);
            del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        }

        public static void Load<T>(ref T del) {
            Load(ref del, VkInstance.Null);
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

    public partial class Instance { //static members
        static vkCreateInstanceDelegate CreateInstance;
        static vkDestroyInstanceDelegate DestroyInstance;
        static vkEnumerateInstanceExtensionPropertiesDelegate EnumerateExtensionProperties;
        static vkEnumerateInstanceLayerPropertiesDelegate EnumerateLayerProperties;

        public static List<Extension> AvailableExtensions { get; private set; }
        public static List<Layer> AvailableLayers { get; private set; }

        internal static void Init() {
            Vulkan.Load(ref CreateInstance);
            Vulkan.Load(ref DestroyInstance);
            Vulkan.Load(ref EnumerateExtensionProperties);
            Vulkan.Load(ref EnumerateLayerProperties);
            GetLayersAndExtensions();
        }

        static void GetLayersAndExtensions() {
            AvailableExtensions = new List<Extension>();
            AvailableLayers = new List<Layer>();
            unsafe
            {
                uint exCount = 0;
                EnumerateExtensionProperties(null, ref exCount, IntPtr.Zero);

                byte* ex = stackalloc byte[Marshal.SizeOf<VkExtensionProperties>() * (int)exCount];
                EnumerateExtensionProperties(null, ref exCount, (IntPtr)ex);

                for (int i = 0; i < exCount; i++) {
                    var extension = Marshal.PtrToStructure<VkExtensionProperties>((IntPtr)ex + Marshal.SizeOf<VkExtensionProperties>() * i);
                    AvailableExtensions.Add(new Extension(extension));
                }

                uint lCount = 0;
                EnumerateLayerProperties(ref lCount, IntPtr.Zero);

                byte* l = stackalloc byte[Marshal.SizeOf<VkLayerProperties>() * (int)lCount];
                EnumerateLayerProperties(ref lCount, (IntPtr)l);

                for (int i = 0; i < lCount; i++) {
                    var layer = Marshal.PtrToStructure<VkLayerProperties>((IntPtr)l + Marshal.SizeOf<VkLayerProperties>() * i);
                    AvailableLayers.Add(new Layer(layer));
                }

                Marshal.DestroyStructure<VkExtensionProperties>((IntPtr)ex);
                Marshal.DestroyStructure<VkLayerProperties>((IntPtr)l);
            }
        }
    }
}
