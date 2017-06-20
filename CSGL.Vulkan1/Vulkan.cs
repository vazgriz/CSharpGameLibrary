using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using CSGL.Vulkan1.Unmanaged;

namespace CSGL.Vulkan1 {
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
                if (extensionsReadOnly == null) {
                    if (!initialized) Init();
                    GetExtensions();
                }
                return extensionsReadOnly;
            }
        }

        public static IList<Layer> AvailableLayers {
            get {
                if (layersReadOnly == null) {
                    if (!initialized) Init();
                    GetLayers();
                }
                return layersReadOnly;
            }
        }

        internal static void Init() {
            Vulkan.Load(ref createInstance);
            Vulkan.Load(ref enumerateExtensionProperties);
            Vulkan.Load(ref enumerateLayerProperties);
            initialized = true;
        }

        static void GetLayers() {
            availableLayers = new List<Layer>();

            uint lCount = 0;
            enumerateLayerProperties(ref lCount, IntPtr.Zero);

            using (var layersMarshalled = new MarshalledArray<VkLayerProperties>((int)lCount)) {
                enumerateLayerProperties(ref lCount, layersMarshalled.Address);

                for (int i = 0; i < lCount; i++) {
                    var layer = layersMarshalled[i];
                    availableLayers.Add(new Layer(layer));
                }

            }
            layersReadOnly = availableLayers.AsReadOnly();
        }

        static void GetExtensions() {
            availableExtensions = new List<Extension>();

            uint exCount = 0;
            enumerateExtensionProperties(null, ref exCount, IntPtr.Zero);

            using (var extensionsMarshalled = new MarshalledArray<VkExtensionProperties>((int)exCount)) {
                enumerateExtensionProperties(null, ref exCount, extensionsMarshalled.Address);

                for (int i = 0; i < exCount; i++) {
                    var extension = extensionsMarshalled[i];
                    availableExtensions.Add(new Extension(extension));
                }
            }
            extensionsReadOnly = availableExtensions.AsReadOnly();
        }
    }
}
