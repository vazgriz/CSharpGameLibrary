using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public static class Vulkan {
        public static void Init() {
            Instance.Init();
        }

        static string GetCommand<T>() {
            Type t = typeof(T);
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
                VkExtensionProperties* exTemp = null;
                EnumerateExtensionProperties(null, ref exCount, ref *exTemp);
                var ex = stackalloc VkExtensionProperties[(int)exCount];
                EnumerateExtensionProperties(null, ref exCount, ref ex[0]);

                for (int i = 0; i < exCount; i++) {
                    var p = ex[i];
                    AvailableExtensions.Add(new Extension(Interop.GetString(p.extensionName), p.specVersion));
                }

                uint lCount = 0;
                VkLayerProperties* lTemp = null;
                EnumerateLayerProperties(ref lCount, ref *lTemp);
                var l = stackalloc VkLayerProperties[(int)lCount];
                EnumerateLayerProperties(ref lCount, ref l[0]);

                for (int i = 0; i < lCount; i++) {
                    var p = l[i];
                    var name = Interop.GetString(p.layerName);
                    var desc = Interop.GetString(p.description);
                    var spec = p.specVersion;
                    var impl = p.implementationVersion;
                    var layer = new Layer(name, desc, spec, impl);
                    AvailableLayers.Add(layer);
                }
            }
        }
    }
}
