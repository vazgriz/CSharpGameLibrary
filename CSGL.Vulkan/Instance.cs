using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan {
    public class ApplicationInfo {
        public VkVersion apiVersion;
        public VkVersion engineVersion;
        public VkVersion applicationVersion;
        public string engineName;
        public string applicationName;
    }

    public class InstanceCreateInfo {
        public ApplicationInfo applicationInfo;
        public IList<string> extensions;
        public IList<string> layers;
    }

    public partial class Instance : IDisposable, INative<Unmanaged.VkInstance> {
        Unmanaged.VkInstance instance;
        IntPtr alloc = IntPtr.Zero;
        bool disposed = false;

        Unmanaged.vkGetInstanceProcAddrDelegate getProcAddrDel;
        public InstanceCommands Commands { get; private set; }
        public IList<string> Extensions { get; private set; }
        public IList<string> Layers { get; private set; }
        public IList<PhysicalDevice> PhysicalDevices { get; private set; }

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

        public Instance(InstanceCreateInfo info) {
            if (info == null) throw new ArgumentNullException(nameof(info));
            Init(info);
        }

        public Instance(InstanceCreateInfo info, Unmanaged.VkAllocationCallbacks callbacks) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            alloc = Marshal.AllocHGlobal(Marshal.SizeOf<Unmanaged.VkAllocationCallbacks>());
            Marshal.StructureToPtr(callbacks, alloc, false);

            Init(info);
        }

        void Init(InstanceCreateInfo mInfo) {
            if (!GLFW.GLFW.VulkanSupported()) throw new InstanceException("Vulkan not supported");
            if (!initialized) Init();

            Extensions = mInfo.extensions.CloneReadOnly();
            Layers = mInfo.layers.CloneReadOnly();

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
            Marshalled<Unmanaged.VkApplicationInfo> appInfoMarshalled = null;

            var extensionsMarshalled = new NativeStringArray(mInfo.extensions);
            var layersMarshalled = new NativeStringArray(mInfo.layers);

            var info = new Unmanaged.VkInstanceCreateInfo();
            info.sType = VkStructureType.InstanceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsMarshalled.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.enabledLayerCount = (uint)layersMarshalled.Count;
            info.ppEnabledLayerNames = layersMarshalled.Address;

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

                appInfoMarshalled = new Marshalled<Unmanaged.VkApplicationInfo>(appInfo);
                info.pApplicationInfo = appInfoMarshalled.Address;
            }

            using (appName) //appName, engineName, and appInfoMarshalled may be null
            using (engineName)
            using (appInfoMarshalled)
            using (extensionsMarshalled)
            using (layersMarshalled) {
                var result = createInstance(ref info, alloc, out instance);
                if (result != VkResult.Success) throw new InstanceException(result, string.Format("Error creating instance: {0}", result));
            }
        }

        void GetPhysicalDevices() {
            uint count = 0;
            Commands.enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
            var devices = new NativeArray<Unmanaged.VkPhysicalDevice>((int)count);
            Commands.enumeratePhysicalDevices(instance, ref count, devices.Address);

            var physicalDevices = new List<PhysicalDevice>();
            for (int i = 0; i < count; i++) {
                physicalDevices.Add(new PhysicalDevice(this, devices[i]));
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
            return getProcAddrDel(instance, Interop.GetUTF8(command));
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

        ~Instance() {
            Dispose(false);
        }
    }

    public class InstanceException : VulkanException {
        public InstanceException(string message) : base(message) { }
        public InstanceException(VkResult result, string message) : base(result, message) { }
    }
}
