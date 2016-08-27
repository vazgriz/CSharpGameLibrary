using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL;
using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
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

            Extensions = mInfo.Extensions;
            Layers = mInfo.Layers;

            ValidateExtensions();
            ValidateLayers();

            CreateInstanceInternal(mInfo);

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

        void CreateInstanceInternal(InstanceCreateInfo mInfo) {
            var instanceMarshalled = new Marshalled<VkInstance>();

            try {
                var result = createInstance(mInfo.Marshalled.Address, alloc, instanceMarshalled.Address);
                if (result != VkResult.Success) throw new InstanceException(string.Format("Error creating instance: {0}", result));

                instance = instanceMarshalled.Value;
            }
            finally {
                instanceMarshalled.Dispose();
            }
        }

        void ValidateLayers() {
            foreach (var s in Layers) {
                bool found = false;

                for (int i = 0; i < AvailableLayers.Count; i++) {
                    if (s == null) throw new ArgumentNullException(string.Format("Requested layer {0} is null", i));
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
                    if (s == null) throw new ArgumentNullException(string.Format("Requested extension {0} is null", i));
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
