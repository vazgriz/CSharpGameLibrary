﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class DeviceCreateInfo {
        public List<string> Extensions { get; set; }
        public List<QueueCreateInfo> QueuesCreateInfos { get; set; }
        public VkPhysicalDeviceFeatures Features { get; set; }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;
        }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos, VkPhysicalDeviceFeatures features) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;
            Features = features;
        }
    }

    public partial class Device : IDisposable {
        VkDevice device;
        bool disposed = false;

        PhysicalDevice physicalDevice;

        vkGetDeviceProcAddrDelegate getDeviceProcAddr;

        public DeviceCommands Commands { get; private set; }

        public Instance Instance { get; private set; }
        public List<string> Extensions { get; private set; }

        public VkDevice Native {
            get {
                return device;
            }
        }

        public Device(PhysicalDevice physicalDevice, DeviceCreateInfo info) {
            if (physicalDevice == null) throw new ArgumentNullException(nameof(physicalDevice));
            if (info == null) throw new ArgumentNullException(nameof(info));

            this.physicalDevice = physicalDevice;
            Instance = physicalDevice.Instance;

            if (info.Extensions == null) {
                Extensions = new List<string>();
            } else {
                Extensions = info.Extensions;
            }

            ValidateExtensions();
            CreateDevice(info);

            Vulkan.Load(ref getDeviceProcAddr, Instance);
            Commands = new DeviceCommands(this);
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
                var qInfos = new VkDeviceQueueCreateInfo[queueInfoCount];
                GCHandle handle = GCHandle.Alloc(qInfos, GCHandleType.Pinned);
                var priorityHandles = new GCHandle[queueInfoCount];

                for (int i = 0; i < queueInfoCount; i++) {
                    qInfos[i].sType = VkStructureType.StructureTypeDeviceQueueCreateInfo;
                    qInfos[i].queueFamilyIndex = mInfo.QueuesCreateInfos[i].QueueFamilyIndex;
                    qInfos[i].queueCount = mInfo.QueuesCreateInfos[i].QueueCount;

                    priorityHandles[i] = GCHandle.Alloc(mInfo.QueuesCreateInfos[i].Priorities, GCHandleType.Pinned);
                    qInfos[i].pQueuePriorities = priorityHandles[i].AddrOfPinnedObject();
                }
                info.queueCreateInfoCount = (uint)queueInfoCount;
                if (queueInfoCount > 0) info.pQueueCreateInfos = handle.AddrOfPinnedObject();

                byte** ppExtensionNames = stackalloc byte*[Extensions.Count];
                GCHandle* exHandles = stackalloc GCHandle[Extensions.Count];

                for (int i = 0; i < Extensions.Count; i++) {
                    var s = Interop.GetUTF8(Extensions[i]);
                    exHandles[i] = GCHandle.Alloc(s, GCHandleType.Pinned);
                    ppExtensionNames[i] = (byte*)exHandles[i].AddrOfPinnedObject();
                }
                info.enabledExtensionCount = (uint)Extensions.Count;
                if (Extensions.Count > 0) info.ppEnabledExtensionNames = (IntPtr)ppExtensionNames;

                IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkDeviceCreateInfo>());
                Marshal.StructureToPtr<VkDeviceCreateInfo>(info, infoPtr, false);

                IntPtr devicePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkDevice>());

                try {
                    var result = Instance.Commands.createDevice(physicalDevice.Native, infoPtr, Instance.AllocationCallbacks, devicePtr);
                    if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));

                    device = Marshal.PtrToStructure<VkDevice>(devicePtr);
                }
                finally {
                    Marshal.DestroyStructure<VkDeviceCreateInfo>(infoPtr);

                    Marshal.FreeHGlobal(infoPtr);
                    Marshal.FreeHGlobal(devicePtr);

                    for (int i = 0; i < Extensions.Count; i++) {
                        exHandles[i].Free();
                    }

                    for (int i = 0; i < queueInfoCount; i++) {
                        priorityHandles[i].Free();
                    }

                    handle.Free();
                }
            }
        }

        void ValidateExtensions() {
            foreach (string ex in Extensions) {
                bool found = false;

                for (int i = 0; i < physicalDevice.AvailableExtensions.Count; i++) {
                    if (physicalDevice.AvailableExtensions[i].Name == ex) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new DeviceException(string.Format("Requested extension not available: {0}", ex));
            }
        }

        public Queue GetQueue(uint familyIndex, uint index) {
            IntPtr queuePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkQueue>());
            Commands.getDeviceQueue(device, familyIndex, index, queuePtr);
            var result = new Queue(this, Marshal.PtrToStructure<VkQueue>(queuePtr));
            Marshal.FreeHGlobal(queuePtr);

            return result;
        }

        public IntPtr GetProcAdddress(string command) {
            return getDeviceProcAddr(device, Interop.GetUTF8(command));
        }

        public void WaitIdle() {
            Commands.waitDeviceIdle(device);
        }

        public void Dispose() {
            if (disposed) return;

            unsafe
            {
                Instance.Commands.destroyDevice(device, Instance.AllocationCallbacks);
            }

            disposed = true;
        }
    }

    public class DeviceException : Exception {
        public DeviceException(string message) : base(message) { }
    }
}