using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Device : IDisposable {
        VkDevice device;
        bool disposed = false;

        PhysicalDevice physicalDevice;

        public Instance Instance { get; private set; }
        public List<string> Extensions { get; private set; }

        public VkDevice Native {
            get {
                return device;
            }
        }

        public Device(PhysicalDevice physicalDevice, DeviceCreateInfo info) {
            if (physicalDevice == null) throw new ArgumentNullException(string.Format("Argument '{0}' can not be null", nameof(physicalDevice)));
            if (info == null) throw new ArgumentNullException(string.Format("Argument '{0}' can not be null", nameof(info)));

            this.physicalDevice = physicalDevice;
            Instance = physicalDevice.Instance;

            if (info.Extensions == null) {
                Extensions = new List<string>();
            } else {
                Extensions = info.Extensions;
            }

            CreateDevice(info);
        }

        void CreateDevice(DeviceCreateInfo mInfo) {
            unsafe
            {
                VkDeviceCreateInfo info = new VkDeviceCreateInfo();
                info.sType = VkStructureType.StructureTypeDeviceCreateInfo;

                int queueInfoCount = 0;
                if (mInfo.QueuesCreateInfos != null) {
                    queueInfoCount = mInfo.QueuesCreateInfos.Count;
                }
                VkDeviceQueueCreateInfo* qInfos = stackalloc VkDeviceQueueCreateInfo[queueInfoCount];
                GCHandle* qpHandles = stackalloc GCHandle[queueInfoCount];

                for (int i = 0; i < queueInfoCount; i++) {
                    qInfos[i] = new VkDeviceQueueCreateInfo();
                    qInfos[i].sType = VkStructureType.StructureTypeDeviceQueueCreateInfo;
                    qInfos[i].queueFamilyIndex = mInfo.QueuesCreateInfos[i].QueueFamilyIndex;
                    qInfos[i].queueCount = mInfo.QueuesCreateInfos[i].QueueCount;
                    qpHandles[i] = GCHandle.Alloc(mInfo.QueuesCreateInfos[i].Priorities, GCHandleType.Pinned);
                    qInfos[i].pQueuePriorities = (float*)qpHandles[i].AddrOfPinnedObject();
                }
                info.queueCreateInfoCount = (uint)queueInfoCount;
                if (queueInfoCount > 0) info.pQueueCreateInfos = qInfos;

                byte** ppExtensionNames = stackalloc byte*[Extensions.Count];
                GCHandle* exHandles = stackalloc GCHandle[Extensions.Count];

                for (int i = 0; i < Extensions.Count; i++) {
                    var s = Interop.GetUTF8(Extensions[i]);
                    exHandles[i] = GCHandle.Alloc(s, GCHandleType.Pinned);
                    ppExtensionNames[i] = (byte*)exHandles[i].AddrOfPinnedObject();
                }
                info.enabledExtensionCount = (uint)Extensions.Count;
                if (Extensions.Count > 0) info.ppEnabledExtensionNames = ppExtensionNames;

                var result = Instance.CreateDevice(physicalDevice.Native, ref info, Instance.AllocationCallbacks, ref device);

                for (int i = 0; i < Extensions.Count; i++) {
                    exHandles[i].Free();
                }

                for (int i = 0; i < queueInfoCount; i++) {
                    qpHandles[i].Free();
                }

                if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            unsafe
            {
                Instance.DestroyDevice(device, Instance.AllocationCallbacks);
            }

            if (disposing) {
                Instance = null;
                physicalDevice = null;
                Extensions = null;
            }

            disposed = true;
        }

        ~Device() {
            Dispose(false);
        }
    }

    public class DeviceException : Exception {
        public DeviceException(string message) : base(message) { }
    }
}
