using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class ApplicationInfo {
        public VkVersion apiVersion;
        public VkVersion engineVersion;
        public VkVersion applicationVersion;
        public string engineName;
        public string applicationName;

        public ApplicationInfo(VkVersion apiVersion, VkVersion applicationVersion, VkVersion engineVersion, string applicationName, string engineName) {
            this.apiVersion = apiVersion;
            this.applicationName = applicationName;
            this.engineName = engineName;
            this.applicationVersion = applicationVersion;
            this.engineVersion = engineVersion;
        }
    }

    public class InstanceCreateInfo {
        public ApplicationInfo applicationInfo;
        public string[] extensions;
        public string[] layers;

        public InstanceCreateInfo(ApplicationInfo applicationInfo, string[] extensions, string[] layers) {
            this.applicationInfo = applicationInfo;
            this.extensions = extensions;
            this.layers = layers;
        }
    }

    public partial class Instance : IDisposable, INative<VkInstance> {
        VkInstance instance;
        IntPtr alloc = IntPtr.Zero;
        bool disposed = false;

        List<string> extensions;
        List<string> layers;
        List<PhysicalDevice> physicalDevices;

        vkGetInstanceProcAddrDelegate getProcAddrDel;
        public InstanceCommands Commands { get; private set; }
        public IList<string> Extensions { get; private set; }
        public IList<string> Layers { get; private set; }
        public IList<PhysicalDevice> PhysicalDevices { get; private set; }

        public VkInstance Native {
            get {
                return instance;
            }
        }


        public IntPtr AllocationCallbacks {
            get {
                return alloc;
            }
        }

        public Instance(InstanceCreateInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Init(info);
        }

        public Instance(InstanceCreateInfo info, VkAllocationCallbacks callbacks) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            alloc = Marshal.AllocHGlobal(Marshal.SizeOf<VkAllocationCallbacks>());
            Marshal.StructureToPtr(callbacks, alloc, false);
            
            Init(info);
        }

        void Init(InstanceCreateInfo mInfo) {
            if (!GLFW.GLFW.VulkanSupported()) throw new InstanceException("Vulkan not supported");
            if (!initialized) Init();

            if (mInfo.extensions == null) {
                extensions = new List<string>();
            } else {
                extensions = new List<string>(mInfo.extensions);
            }

            if (mInfo.layers == null) {
                layers = new List<string>();
            } else {
                layers = new List<string>(mInfo.layers);
            }

            Extensions = extensions.AsReadOnly();
            Layers = layers.AsReadOnly();

            ValidateExtensions();
            ValidateLayers();

            CreateInstance(mInfo);

            Vulkan.Load(ref getProcAddrDel, instance);

            Commands = new InstanceCommands(this);
            
            GetPhysicalDevices();
        }

        void CreateInstance(InstanceCreateInfo mInfo) {
            InteropString appName = null;
            InteropString engineName = null;
            Marshalled<VkApplicationInfo> appInfoMarshalled = null;

            var extensionsMarshalled = new NativeStringArray(mInfo.extensions);
            var layersMarshalled = new NativeStringArray(mInfo.layers);

            var info = new VkInstanceCreateInfo();
            info.sType = VkStructureType.InstanceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsMarshalled.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.enabledLayerCount = (uint)layersMarshalled.Count;
            info.ppEnabledLayerNames = layersMarshalled.Address;

            var appInfo = new VkApplicationInfo();
            appInfo.sType = VkStructureType.ApplicationInfo;
            if (mInfo.applicationInfo != null) {
                appInfo.apiVersion = mInfo.applicationInfo.apiVersion;
                appInfo.engineVersion = mInfo.applicationInfo.engineVersion;
                appInfo.applicationVersion = mInfo.applicationInfo.applicationVersion;

                appName = new InteropString(mInfo.applicationInfo.applicationName);
                appInfo.pApplicationName = appName.Address;

                engineName = new InteropString(mInfo.applicationInfo.engineName);
                appInfo.pEngineName = engineName.Address;

                appInfoMarshalled = new Marshalled<VkApplicationInfo>(appInfo);
                info.pApplicationInfo = appInfoMarshalled.Address;
            }

            try {
                var result = createInstance(ref info, alloc, out instance);
                if (result != VkResult.Success) throw new InstanceException(string.Format("Error creating instance: {0}", result));
            }
            finally {
                appName?.Dispose();
                engineName?.Dispose();
                appInfoMarshalled?.Dispose();

                extensionsMarshalled.Dispose();
                layersMarshalled.Dispose();
            }
        }

        void GetPhysicalDevices() {
            uint count = 0;
            Commands.enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
            var devices = new NativeArray<VkPhysicalDevice>((int)count);
            Commands.enumeratePhysicalDevices(instance, ref count, devices.Address);

            physicalDevices = new List<PhysicalDevice>();
            for (int i = 0; i < count; i++) {
                physicalDevices.Add(new PhysicalDevice(this, devices[i]));
            }

            PhysicalDevices = physicalDevices.AsReadOnly();
        }

        void ValidateLayers() {
            foreach (var s in Layers) {
                bool found = false;

                for (int i = 0; i < AvailableLayers.Count; i++) {
                    if (s == null) throw new ArgumentNullException(string.Format("Requested layer {0} is null", s));
                    if (AvailableLayers[i].Name == s) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new InstanceException(string.Format("Requested layer '{0}' is not available", s));
            }
        }

        void ValidateExtensions() {
            foreach (var s in Extensions) {
                bool found = false;

                for (int i = 0; i < AvailableExtensions.Count; i++) {
                    if (s == null) throw new ArgumentNullException(string.Format("Requested extension {0} is null", s));
                    if (AvailableExtensions[i].Name == s) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new InstanceException(string.Format("Requested extension '{0}' is not available", s));
            }
        }

        public IntPtr GetProcAddress(string command) {
            return getProcAddrDel(instance, Interop.GetUTF8(command));
        }

        public void Dispose() {
            if (disposed) return;

            VK.DestroyInstance(instance, alloc);

            if (alloc != IntPtr.Zero) {
                Marshal.DestroyStructure<VkAllocationCallbacks>(alloc);
                Marshal.FreeHGlobal(alloc);
            }

            disposed = true;
        }
    }

    public class InstanceException : Exception {
        public InstanceException(string message) : base(message) { }
    }
}
