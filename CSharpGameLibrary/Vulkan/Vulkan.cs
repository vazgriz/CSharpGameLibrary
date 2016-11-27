using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public static class Vulkan {
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

    public partial class Instance { //static members
        static vkCreateInstanceDelegate createInstance;
        static vkEnumerateInstanceExtensionPropertiesDelegate enumerateExtensionProperties;
        static vkEnumerateInstanceLayerPropertiesDelegate enumerateLayerProperties;

        static List<Extension> availableExtensions;
        static ReadOnlyCollection<Extension> extensionsReadOnly;
        static List<Layer> availableLayers;
        static ReadOnlyCollection<Layer> layersReadOnly;

        static bool initialized = false;

        public static IList<Extension> AvailableExtensions {
            get {
                if (!initialized) Init();
                return extensionsReadOnly;
            }
        }

        public static IList<Layer> AvailableLayers {
            get {
                if (!initialized) Init();
                return layersReadOnly;
            }
        }

        internal static void Init() {
            Vulkan.Load(ref createInstance);
            Vulkan.Load(ref enumerateExtensionProperties);
            Vulkan.Load(ref enumerateLayerProperties);
            GetLayersAndExtensions();
            initialized = true;
        }

        static void GetLayersAndExtensions() {
            availableExtensions = new List<Extension>();
            availableLayers = new List<Layer>();

            uint exCount = 0;
            enumerateExtensionProperties(null, ref exCount, IntPtr.Zero);

            var ex = new MarshalledArray<VkExtensionProperties>((int)exCount);
            enumerateExtensionProperties(null, ref exCount, ex.Address);

            for (int i = 0; i < exCount; i++) {
                var extension = ex[i];
                availableExtensions.Add(new Extension(extension));
            }

            uint lCount = 0;
            enumerateLayerProperties(ref lCount, IntPtr.Zero);

            var l = new MarshalledArray<VkLayerProperties>((int)lCount);
            enumerateLayerProperties(ref lCount, l.Address);

            for (int i = 0; i < lCount; i++) {
                var layer = l[i];
                availableLayers.Add(new Layer(layer));
            }

            extensionsReadOnly = availableExtensions.AsReadOnly();
            layersReadOnly = availableLayers.AsReadOnly();

            ex.Dispose();
            l.Dispose();
        }
    }
}
