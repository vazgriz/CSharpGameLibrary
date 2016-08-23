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
        static vkCreateInstanceDelegate createInstance;
        static vkDestroyInstanceDelegate destroyInstance;
        static vkEnumerateInstanceExtensionPropertiesDelegate enumerateExtensionProperties;
        static vkEnumerateInstanceLayerPropertiesDelegate enumerateLayerProperties;

        public static List<Extension> AvailableExtensions { get; private set; }
        public static List<Layer> AvailableLayers { get; private set; }

        internal static void Init() {
            Vulkan.Load(ref createInstance);
            Vulkan.Load(ref destroyInstance);
            Vulkan.Load(ref enumerateExtensionProperties);
            Vulkan.Load(ref enumerateLayerProperties);
            GetLayersAndExtensions();
        }

        static void GetLayersAndExtensions() {
            AvailableExtensions = new List<Extension>();
            AvailableLayers = new List<Layer>();

            uint exCount = 0;
            enumerateExtensionProperties(null, ref exCount, IntPtr.Zero);

            var ex = new MarshalledArray<VkExtensionProperties>((int)exCount);
            enumerateExtensionProperties(null, ref exCount, ex.Address);

            for (int i = 0; i < exCount; i++) {
                var extension = ex[i];
                AvailableExtensions.Add(new Extension(extension));
            }

            uint lCount = 0;
            enumerateLayerProperties(ref lCount, IntPtr.Zero);

            var l = new MarshalledArray<VkLayerProperties>((int)lCount);
            enumerateLayerProperties(ref lCount, l.Address);

            for (int i = 0; i < lCount; i++) {
                var layer = l[i];
                AvailableLayers.Add(new Layer(layer));
            }

            ex.Dispose();
            l.Dispose();
        }
    }
}
