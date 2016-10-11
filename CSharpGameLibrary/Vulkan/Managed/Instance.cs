using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class ApplicationInfo {
        public VkVersion apiVersion;
        public VkVersion engineVersion;
        public VkVersion applicationVersion;
        public string engineName;
        public string applicationName;

        public ApplicationInfo(VkVersion apiVersion, string applicationName, string engineName, VkVersion applicationVersion, VkVersion engineVersion) {
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

    public partial class Instance : IDisposable {
        VkInstance instance;
        IntPtr alloc = IntPtr.Zero;
        bool disposed = false;

        vkGetInstanceProcAddrDelegate getProcAddrDel;
        public InstanceCommands Commands { get; private set; }
        public List<string> Extensions { get; private set; }
        public List<string> Layers { get; private set; }
        public List<PhysicalDevice> PhysicalDevices { get; private set; }

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
            if (!initialized) throw new InstanceException("Vulkan was not initialized (make sure to call Vulkan.Init())");

            Extensions = new List<string>(mInfo.extensions);
            Layers = new List<string>(mInfo.layers);

            ValidateExtensions();
            ValidateLayers();

            CreateInstanceInternal(mInfo);

            Vulkan.Load(ref getProcAddrDel, instance);

            Commands = new InstanceCommands(this);
            
            GetPhysicalDevices();
        }

        void CreateInstanceInternal(InstanceCreateInfo mInfo) {
            InteropString appName = null;
            InteropString engineName = null;
            Marshalled<VkApplicationInfo> appInfoMarshalled = null;

            var extensionsMarshalled = new MarshalledStringArray(mInfo.extensions);
            var layersMarshalled = new MarshalledStringArray(mInfo.layers);

            var info = new VkInstanceCreateInfo();
            info.sType = VkStructureType.StructureTypeInstanceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsMarshalled.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.enabledLayerCount = (uint)layersMarshalled.Count;
            info.ppEnabledLayerNames = layersMarshalled.Address;

            var appInfo = new VkApplicationInfo();
            appInfo.sType = VkStructureType.StructureTypeApplicationInfo;
            if (mInfo.applicationInfo != null) {
                appInfo.apiVersion = mInfo.applicationInfo.apiVersion;
                appInfo.engineVersion = mInfo.applicationInfo.engineVersion;
                appInfo.applicationVersion = mInfo.applicationInfo.applicationVersion;
                appName = new InteropString(mInfo.applicationInfo.applicationName);
                engineName = new InteropString(mInfo.applicationInfo.engineName);

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
            PhysicalDevices = new List<PhysicalDevice>();
            uint count = 0;
            Commands.enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
            var devices = new MarshalledArray<VkPhysicalDevice>((int)count);
            Commands.enumeratePhysicalDevices(instance, ref count, devices.Address);

            for (int i = 0; i < count; i++) {
                PhysicalDevices.Add(new PhysicalDevice(this, devices[i]));
            }
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
