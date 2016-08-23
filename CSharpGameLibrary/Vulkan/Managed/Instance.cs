using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class InstanceCreateInfo {
        public ApplicationInfo ApplicationInfo { get; set; }
        public List<string> Extensions { get; set; }
        public List<string> Layers { get; set; }

        public InstanceCreateInfo(ApplicationInfo appInfo, List<string> extensions, List<string> layers) {
            ApplicationInfo = appInfo;
            Extensions = extensions;
            Layers = layers;
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
            CreateInstanceInternal(info);
        }

        public Instance(InstanceCreateInfo info, VkAllocationCallbacks callbacks) {
            if (info == null) throw new ArgumentNullException(nameof(info));

            alloc = Marshal.AllocHGlobal(Marshal.SizeOf<VkAllocationCallbacks>());
            Marshal.StructureToPtr(callbacks, alloc, false);
            
            CreateInstanceInternal(info);
        }

        void CreateInstanceInternal(InstanceCreateInfo mInfo) {
            if (!GLFW.GLFW.VulkanSupported()) throw new InstanceException("Vulkan not supported");

            if (mInfo.Extensions == null) {
                Extensions = new List<string>();
            } else {
                Extensions = mInfo.Extensions;
            }
            if (mInfo.Layers == null) {
                Layers = new List<string>();
            } else {
                Layers = mInfo.Layers;
            }

            ValidateExtensions();
            ValidateLayers();

            MakeVulkanInstance(mInfo);

            Vulkan.Load(ref getProcAddrDel);

            Commands = new InstanceCommands(this);
            
            GetPhysicalDevices();
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

        void MakeVulkanInstance(InstanceCreateInfo mInfo) {
            //the managed classes are assembled into Vulkan structs on the stack
            //they can't be members of the class without pinning or fixing them
            //allocating the callback on the unmanaged heap feels messy enough, I didn't want to do that with every struct
            //nor did I want to make InstanceCreateInfo and ApplicationInfo disposable
            
            VkApplicationInfo appInfo = new VkApplicationInfo();
            VkInstanceCreateInfo info = new VkInstanceCreateInfo();
            var marshalled = new List<IDisposable>();

            Marshalled<VkApplicationInfo> appInfoMarshalled;

            info.sType = VkStructureType.StructureTypeInstanceCreateInfo;
                
            if (mInfo.ApplicationInfo != null) {
                appInfo.sType = VkStructureType.StructureTypeApplicationInfo;
                appInfo.apiVersion = mInfo.ApplicationInfo.APIVersion;
                appInfo.engineVersion = mInfo.ApplicationInfo.EngineVersion;
                appInfo.applicationVersion = mInfo.ApplicationInfo.ApplicationVersion;
                appInfo.pApplicationName = mInfo.ApplicationInfo.ApplicationName;
                appInfo.pEngineName = mInfo.ApplicationInfo.EngineName;

                appInfoMarshalled = new Marshalled<VkApplicationInfo>(appInfo);
                marshalled.Add(appInfoMarshalled);

                info.pApplicationInfo = appInfoMarshalled.Address;
            }

            IntPtr[] extensionNames = new IntPtr[Extensions.Count];
            IntPtr[] layerNames = new IntPtr[Layers.Count];

            var exMarshalled = new PinnedArray<IntPtr>(extensionNames);
            var lMarshalled = new PinnedArray<IntPtr>(layerNames);
                
            for (int i = 0; i < Extensions.Count; i++) {
                var s = new InteropString(Extensions[i]);
                extensionNames[i] = s.Address;
                marshalled.Add(s);
            }
            info.enabledExtensionCount = (uint)Extensions.Count;
            if (Extensions.Count > 0) info.ppEnabledExtensionNames = exMarshalled.Address;
                
            for (int i = 0; i < Layers.Count; i++) {
                var s = new InteropString(Layers[i]);
                layerNames[i] = s.Address;
                marshalled.Add(s);
            }
            info.enabledLayerCount = (uint)Layers.Count;
            if (Layers.Count > 0) info.ppEnabledLayerNames = lMarshalled.Address;

            var infoMarshalled = new Marshalled<VkInstanceCreateInfo>(info);
            var instanceMarshalled = new Marshalled<VkInstance>();

            try {
                var result = createInstance(infoMarshalled.Address, alloc, instanceMarshalled.Address);
                if (result != VkResult.Success) throw new InstanceException(string.Format("Error creating instance: {0}", result));

                instance = instanceMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                instanceMarshalled.Dispose();

                foreach (var m in marshalled) m.Dispose();
            }
        }

        void ValidateLayers() {
            foreach (var s in Layers) {
                bool found = false;

                for (int i = 0; i < AvailableLayers.Count; i++) {
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

            destroyInstance(instance, alloc);

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
