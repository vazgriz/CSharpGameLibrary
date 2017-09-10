using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

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
        public List<string> extensions;
        public List<string> layers;
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

            CreateInstance(mInfo);

            Extensions = extensions.AsReadOnly();
            Layers = layers.AsReadOnly();

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

            if (mInfo.applicationInfo != null) {
                var appInfo = new VkApplicationInfo();
                appInfo.sType = VkStructureType.ApplicationInfo;
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
            var devices = new NativeArray<VkPhysicalDevice>((int)count);
            Commands.enumeratePhysicalDevices(instance, ref count, devices.Address);

            physicalDevices = new List<PhysicalDevice>();
            for (int i = 0; i < count; i++) {
                physicalDevices.Add(new PhysicalDevice(this, devices[i]));
            }

            PhysicalDevices = physicalDevices.AsReadOnly();
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

            VK.DestroyInstance(instance, alloc);
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
