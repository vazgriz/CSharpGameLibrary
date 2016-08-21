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
            unsafe
            {
                uint count = 0;
                Commands.enumeratePhysicalDevices(instance, ref count, IntPtr.Zero);
                var devices = stackalloc byte[Marshal.SizeOf<VkPhysicalDevice>() * (int)count];
                Commands.enumeratePhysicalDevices(instance, ref count, (IntPtr)devices);

                for (int i = 0; i < count; i++) {
                    PhysicalDevices.Add(new PhysicalDevice(this,
                    Marshal.PtrToStructure<VkPhysicalDevice>((IntPtr)devices + Marshal.SizeOf<VkPhysicalDevice>() * i)));
                }
            }
        }

        void MakeVulkanInstance(InstanceCreateInfo mInfo) {
            //the managed classes are assembled into Vulkan structs on the stack
            //they can't be members of the class without pinning or fixing them
            //allocating the callback on the unmanaged heap feels messy enough, I didn't want to do that with every struct
            //nor did I want to make InstanceCreateInfo and ApplicationInfo disposable

            unsafe
            {
                VkApplicationInfo appInfo = new VkApplicationInfo();
                VkInstanceCreateInfo info = new VkInstanceCreateInfo();

                var appInfoMarshalled = stackalloc byte[Marshal.SizeOf<VkApplicationInfo>()];

                info.sType = VkStructureType.StructureTypeInstanceCreateInfo;
                
                if (mInfo.ApplicationInfo != null) {
                    appInfo.sType = VkStructureType.StructureTypeApplicationInfo;
                    appInfo.apiVersion = mInfo.ApplicationInfo.APIVersion;
                    appInfo.engineVersion = mInfo.ApplicationInfo.EngineVersion;
                    appInfo.applicationVersion = mInfo.ApplicationInfo.ApplicationVersion;
                    appInfo.pApplicationName = mInfo.ApplicationInfo.ApplicationName;
                    appInfo.pEngineName = mInfo.ApplicationInfo.EngineName;

                    Marshal.StructureToPtr(appInfo, (IntPtr)appInfoMarshalled, false);

                    info.pApplicationInfo = (IntPtr)appInfoMarshalled;
                }

                byte** ppExtensionNames = stackalloc byte*[Extensions.Count];
                byte** ppLayerNames = stackalloc byte*[Layers.Count];
                GCHandle* exHandles = stackalloc GCHandle[Extensions.Count];
                GCHandle* lHandles = stackalloc GCHandle[Layers.Count];
                
                for (int i = 0; i < Extensions.Count; i++) {
                    var s = Interop.GetUTF8(Extensions[i]);
                    exHandles[i] = GCHandle.Alloc(s, GCHandleType.Pinned);
                    ppExtensionNames[i] = (byte*)exHandles[i].AddrOfPinnedObject();
                }
                info.enabledExtensionCount = (uint)Extensions.Count;
                if (Extensions.Count > 0) info.ppEnabledExtensionNames = (IntPtr)ppExtensionNames;
                
                for (int i = 0; i < Layers.Count; i++) {
                    var s = Interop.GetUTF8(Layers[i]);
                    lHandles[i] = GCHandle.Alloc(s, GCHandleType.Pinned);
                    ppLayerNames[i] = (byte*)lHandles[i].AddrOfPinnedObject();
                }
                info.enabledLayerCount = (uint)Layers.Count;
                if (Layers.Count > 0) info.ppEnabledLayerNames = (IntPtr)ppLayerNames;

                IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkInstanceCreateInfo>());
                Marshal.StructureToPtr(info, infoPtr, false);

                IntPtr instancePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkInstance>());

                try {
                    var result = CreateInstance(infoPtr, alloc, instancePtr);
                    if (result != VkResult.Success) throw new InstanceException(string.Format("Error creating instance: {0}", result));

                    instance = Marshal.PtrToStructure<VkInstance>(instancePtr);
                }
                finally {
                    Marshal.DestroyStructure<VkInstanceCreateInfo>(infoPtr);
                    Marshal.DestroyStructure<VkInstance>(instancePtr);

                    Marshal.FreeHGlobal(infoPtr);
                    Marshal.FreeHGlobal(instancePtr);

                    for (int i = 0; i < Extensions.Count; i++) {
                        exHandles[i].Free();
                    }
                    for (int i = 0; i < Layers.Count; i++) {
                        lHandles[i].Free();
                    }

                    if (mInfo.ApplicationInfo != null) {
                        Marshal.DestroyStructure<VkApplicationInfo>((IntPtr)appInfoMarshalled);
                    }
                }
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

            DestroyInstance(instance, alloc);

            if (alloc != IntPtr.Zero) {
                Marshal.FreeHGlobal(alloc);
            }

            disposed = true;
        }
    }

    public class InstanceException : Exception {
        public InstanceException(string message) : base(message) { }
    }
}
