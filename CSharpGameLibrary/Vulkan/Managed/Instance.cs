using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL;
//using static CSGL.Vulkan.Unmanaged.VK;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Instance : IDisposable {
        VkInstance instance;
        unsafe VkAllocationCallbacks* alloc = null;
        bool callbacksSet = false;
        bool disposed = false;

        public List<string> Extensions { get; private set; }
        public List<string> Layers { get; private set; }

        static vkCreateInstanceDelegate CreateInstance;
        static vkDestroyInstanceDelegate DestroyInstance;
        static vkEnumerateInstanceExtensionPropertiesDelegate EnumerateExtensionProperties;
        static vkEnumerateInstanceLayerPropertiesDelegate EnumerateLayerProperties;

        public static List<string> AvailableExtensions { get; private set; }
        public static List<string> AvailableLayers { get; private set; }

        public unsafe VkAllocationCallbacks* AllocationCallbacks {
            get {
                return alloc;
            }
        }

        internal static void Init() {
            Vulkan.Load(ref CreateInstance);
            Vulkan.Load(ref DestroyInstance);
            Vulkan.Load(ref EnumerateExtensionProperties);
            Vulkan.Load(ref EnumerateLayerProperties);
            GetLayersAndExtensions();
        }

        static void GetLayersAndExtensions() {
            AvailableExtensions = new List<string>();
            AvailableLayers = new List<string>();
            unsafe
            {
                uint exCount = 0;
                VkExtensionProperties* exTemp = null;
                EnumerateExtensionProperties(null, ref exCount, ref *exTemp);
                VkExtensionProperties* ex = stackalloc VkExtensionProperties[(int)exCount];
                EnumerateExtensionProperties(null, ref exCount, ref ex[0]);

                for (int i = 0; i < exCount; i++) {
                    var p = ex[i];
                    AvailableExtensions.Add(Interop.GetString(p.extensionName));
                }

                uint lCount = 0;
                VkLayerProperties* lTemp = null;
                EnumerateLayerProperties(ref lCount, ref *lTemp);
                VkLayerProperties* l = stackalloc VkLayerProperties[(int)lCount];
                EnumerateLayerProperties(ref lCount, ref l[0]);

                for (int i = 0; i < lCount; i++) {
                    var p = l[i];
                    AvailableLayers.Add(Interop.GetString(p.layerName));
                }
            }
        }

        public Instance(InstanceCreateInfo info) {
            CreateInstanceInternal(info);
        }

        public Instance(InstanceCreateInfo info, VkAllocationCallbacks callbacks) {
            unsafe
            {
                alloc = (VkAllocationCallbacks*)Marshal.AllocHGlobal(Marshal.SizeOf<VkAllocationCallbacks>());
                *alloc = callbacks;
            }
            callbacksSet = true;
            CreateInstanceInternal(info);
        }

        void CreateInstanceInternal(InstanceCreateInfo mInfo) {
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

            //the managed classes are assembled into Vulkan structs on the stack
            //they can't be members of the class without pinning or fixing them
            //allocating the callback on the unmanaged heap feels messy enough, I didn't want to do that with every struct

            unsafe
            {
                VkApplicationInfo appInfo = new VkApplicationInfo();
                VkInstanceCreateInfo info = new VkInstanceCreateInfo();
                info.ppEnabledExtensionNames = null;
                info.ppEnabledLayerNames = null;

                info.sType = VkStructureType.StructureTypeInstanceCreateInfo;

                GCHandle appNameHandle = new GCHandle();
                GCHandle engNameHandle = new GCHandle();
                if (mInfo.ApplicationInfo != null) {
                    appInfo.sType = VkStructureType.StructureTypeApplicationInfo;
                    appInfo.apiVersion = mInfo.ApplicationInfo.APIVersion;
                    appInfo.engineVersion = mInfo.ApplicationInfo.EngineVersion;
                    appInfo.applicationVersion = mInfo.ApplicationInfo.ApplicationVersion;
                    info.pApplicationInfo = &appInfo;

                    var appName = Interop.GetUTF8(mInfo.ApplicationInfo.ApplicationName);
                    var engName = Interop.GetUTF8(mInfo.ApplicationInfo.EngineName);
                    appNameHandle = GCHandle.Alloc(appName, GCHandleType.Pinned);
                    engNameHandle = GCHandle.Alloc(engName, GCHandleType.Pinned);
                    appInfo.pApplicationName = (byte*)appNameHandle.AddrOfPinnedObject();
                    appInfo.pEngineName = (byte*)engNameHandle.AddrOfPinnedObject();
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
                if (Extensions.Count > 0) info.ppEnabledExtensionNames = ppExtensionNames;
                
                for (int i = 0; i < Layers.Count; i++) {
                    var s = Interop.GetUTF8(Layers[i]);
                    lHandles[i] = GCHandle.Alloc(s, GCHandleType.Pinned);
                    ppLayerNames[i] = (byte*)lHandles[i].AddrOfPinnedObject();
                }
                info.enabledLayerCount = (uint)Layers.Count;
                if (Layers.Count > 0) info.ppEnabledLayerNames = ppLayerNames;

                var result = CreateInstance(ref info, alloc, ref instance);

                for (int i = 0; i < Extensions.Count; i++) {
                    exHandles[i].Free();
                }
                for (int i = 0; i < Layers.Count; i++) {
                    lHandles[i].Free();
                }

                if (mInfo.ApplicationInfo != null) {
                    appNameHandle.Free();
                    engNameHandle.Free();
                }

                if (result != VkResult.Success) throw new InstanceException(string.Format("Error creating instance: {0}", result));
            }
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    Extensions = null;
                    Layers = null;
                }
                unsafe
                {
                    DestroyInstance(instance, alloc);
                    if (callbacksSet) {
                        Marshal.FreeHGlobal((IntPtr)alloc);
                    }
                }
                disposed = true;
                Console.WriteLine("Disposed");
            }
        }

        ~Instance() {
            Dispose(false);
        }
    }

    public class InstanceException : Exception {
        public InstanceException(string message) : base(message) { }
    }
}
