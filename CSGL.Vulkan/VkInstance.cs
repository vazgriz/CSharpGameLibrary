﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class VkApplicationInfo {
        public VkVersion apiVersion;
        public VkVersion engineVersion;
        public VkVersion applicationVersion;
        public string engineName;
        public string applicationName;
    }

    public class VkInstanceCreateInfo {
        public VkApplicationInfo applicationInfo;
        public IList<string> extensions;
        public IList<string> layers;
    }

    public partial class VkInstance : IDisposable, INative<Unmanaged.VkInstance> {
        Unmanaged.VkInstance instance;
        IntPtr alloc = IntPtr.Zero;
        bool disposed = false;
        
        public InstanceCommands Commands { get; private set; }
        public IList<string> Extensions { get; private set; }
        public IList<string> Layers { get; private set; }
        public IList<VkPhysicalDevice> PhysicalDevices { get; private set; }

        public Unmanaged.VkInstance Native {
            get {
                return instance;
            }
        }

        public IntPtr AllocationCallbacks {
            get {
                return alloc;
            }
        }

        static Unmanaged.vkCreateInstanceDelegate createInstance;
        static Unmanaged.vkEnumerateInstanceExtensionPropertiesDelegate enumerateExtensionProperties;
        static Unmanaged.vkEnumerateInstanceLayerPropertiesDelegate enumerateLayerProperties;

        static ReadOnlyCollection<VkExtension> extensionsReadOnly;
        static ReadOnlyCollection<VkLayer> layersReadOnly;

        static bool initialized = false;

        public static IList<VkExtension> AvailableExtensions {
            get {
                if (extensionsReadOnly == null) {
                    if (!initialized) Init();
                    GetExtensions();
                }
                return extensionsReadOnly;
            }
        }

        public static IList<VkLayer> AvailableLayers {
            get {
                if (layersReadOnly == null) {
                    if (!initialized) Init();
                    GetLayers();
                }
                return layersReadOnly;
            }
        }

        public VkInstance(VkInstanceCreateInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Init(info);
        }

        public VkInstance(VkInstanceCreateInfo info, Unmanaged.VkAllocationCallbacks callbacks) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            alloc = Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged.VkAllocationCallbacks>());
            Marshal.StructureToPtr(callbacks, alloc, false);

            Init(info);
        }

        void Init(VkInstanceCreateInfo mInfo) {
            if (!GLFW.GLFW.VulkanSupported()) throw new InstanceException("Vulkan not supported");
            if (!initialized) Init();

            Extensions = mInfo.extensions.CloneReadOnly();
            Layers = mInfo.layers.CloneReadOnly();

            ValidateExtensions();
            ValidateLayers();

            CreateInstance(mInfo);

            Commands = new InstanceCommands(this);
            
            GetPhysicalDevices();
        }

        void CreateInstance(VkInstanceCreateInfo mInfo) {
            InteropString appName = null;
            InteropString engineName = null;
            Native<Unmanaged.VkApplicationInfo> appInfoNative = null;

            var extensionsNative = new NativeStringArray(mInfo.extensions);
            var layersNative = new NativeStringArray(mInfo.layers);

            var info = new Unmanaged.VkInstanceCreateInfo();
            info.sType = VkStructureType.InstanceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsNative.Count;
            info.ppEnabledExtensionNames = extensionsNative.Address;
            info.enabledLayerCount = (uint)layersNative.Count;
            info.ppEnabledLayerNames = layersNative.Address;

            if (mInfo.applicationInfo != null) {
                var appInfo = new Unmanaged.VkApplicationInfo();
                appInfo.sType = VkStructureType.ApplicationInfo;
                appInfo.apiVersion = mInfo.applicationInfo.apiVersion;
                appInfo.engineVersion = mInfo.applicationInfo.engineVersion;
                appInfo.applicationVersion = mInfo.applicationInfo.applicationVersion;

                appName = new InteropString(mInfo.applicationInfo.applicationName);
                appInfo.pApplicationName = appName.Address;

                engineName = new InteropString(mInfo.applicationInfo.engineName);
                appInfo.pEngineName = engineName.Address;

                appInfoNative = new Native<Unmanaged.VkApplicationInfo>(appInfo);
                info.pApplicationInfo = appInfoNative.Address;
            }

            using (appName) //appName, engineName, and appInfoMarshalled may be null
            using (engineName)
            using (appInfoNative)
            using (extensionsNative)
            using (layersNative) {
                var result = createInstance(ref info, alloc, out instance);
                if (result != VkResult.Success) throw new InstanceException(result, string.Format("Error creating instance: {0}", result));
            }
        }

        void GetPhysicalDevices() {
            uint count = 0;
            Commands.enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
            var devices = new NativeArray<Unmanaged.VkPhysicalDevice>((int)count);
            Commands.enumeratePhysicalDevices(instance, ref count, devices.Address);

            var physicalDevices = new List<VkPhysicalDevice>();
            for (int i = 0; i < count; i++) {
                physicalDevices.Add(new VkPhysicalDevice(this, devices[i]));
            }

            PhysicalDevices = physicalDevices.AsReadOnly();
        }

        void ValidateExtensions() {
            foreach (var ex in Extensions) {
                bool found = false;

                foreach (var available in AvailableExtensions) {
                    if (available.Name == ex) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new InstanceException(string.Format("Requested extension not available: {0}", ex));
            }
        }

        void ValidateLayers() {
            foreach (var layer in Layers) {
                bool found = false;

                foreach (var available in AvailableLayers) {
                    if (available.Name == layer) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new InstanceException(string.Format("Requested layer not available: {0}", layer));
            }
        }

        public IntPtr GetProcAddress(string command) {
            return Commands.getProcAddr(instance, Interop.GetUTF8(command));
        }

        internal static void Init() {
            Unmanaged.VK.Load(ref createInstance);
            Unmanaged.VK.Load(ref enumerateExtensionProperties);
            Unmanaged.VK.Load(ref enumerateLayerProperties);
            initialized = true;
        }

        static void GetLayers() {
            var availableLayers = new List<VkLayer>();

            uint lCount = 0;
            enumerateLayerProperties(ref lCount, IntPtr.Zero);

            using (var layersNative = new NativeArray<Unmanaged.VkLayerProperties>((int)lCount)) {
                enumerateLayerProperties(ref lCount, layersNative.Address);

                for (int i = 0; i < lCount; i++) {
                    var layer = layersNative[i];
                    availableLayers.Add(new VkLayer(layer));
                }

            }
            layersReadOnly = availableLayers.AsReadOnly();
        }

        static void GetExtensions() {
            var availableExtensions = new List<VkExtension>();

            uint exCount = 0;
            enumerateExtensionProperties(null, ref exCount, IntPtr.Zero);

            using (var extensionsNative = new NativeArray<Unmanaged.VkExtensionProperties>((int)exCount)) {
                enumerateExtensionProperties(null, ref exCount, extensionsNative.Address);

                for (int i = 0; i < exCount; i++) {
                    var extension = extensionsNative[i];
                    availableExtensions.Add(new VkExtension(extension));
                }
            }
            extensionsReadOnly = availableExtensions.AsReadOnly();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Unmanaged.VK.DestroyInstance(instance, alloc);
            Marshal.FreeHGlobal(alloc);

            disposed = true;
        }

        ~VkInstance() {
            Dispose(false);
        }
    }

    public class InstanceException : VulkanException {
        public InstanceException(string message) : base(message) { }
        public InstanceException(VkResult result, string message) : base(result, message) { }
    }
}
