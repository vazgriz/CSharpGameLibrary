using System;
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
            VkDeviceCreateInfo info = new VkDeviceCreateInfo();
            info.sType = VkStructureType.StructureTypeDeviceCreateInfo;
            var marshalled = new List<IDisposable>();

            int queueInfoCount = 0;
            if (mInfo.QueuesCreateInfos != null) {
                queueInfoCount = mInfo.QueuesCreateInfos.Count;
            }

            var qInfos = new VkDeviceQueueCreateInfo[queueInfoCount];
            var prioritieAddrs = new IntPtr[queueInfoCount];

            for (int i = 0; i < queueInfoCount; i++) {
                qInfos[i].sType = VkStructureType.StructureTypeDeviceQueueCreateInfo;
                qInfos[i].queueFamilyIndex = mInfo.QueuesCreateInfos[i].QueueFamilyIndex;
                qInfos[i].queueCount = mInfo.QueuesCreateInfos[i].QueueCount;

                var prioritiesMarshalled = new PinnedArray<float>(mInfo.QueuesCreateInfos[i].Priorities);
                qInfos[i].pQueuePriorities = prioritiesMarshalled.Address;
                marshalled.Add(prioritiesMarshalled);
            }

            var qInfosMarshalled = new MarshalledArray<VkDeviceQueueCreateInfo>(qInfos);
            info.queueCreateInfoCount = (uint)queueInfoCount;
            info.pQueueCreateInfos = qInfosMarshalled.Address;

            IntPtr[] extensionsNames = new IntPtr[Extensions.Count];
            var exMarshalled = new PinnedArray<IntPtr>(extensionsNames);

            for (int i = 0; i < Extensions.Count; i++) {
                var s = new InteropString(Extensions[i]);
                extensionsNames[i] = s.Address;
                marshalled.Add(s);
            }
            info.enabledExtensionCount = (uint)Extensions.Count;
            if (Extensions.Count > 0) info.ppEnabledExtensionNames = exMarshalled.Address;

            var infoMarshalled = new Marshalled<VkDeviceCreateInfo>(info);

            var deviceMarshalled = new Marshalled<VkDevice>();

            try {
                var result = Instance.Commands.createDevice(physicalDevice.Native, infoMarshalled.Address, Instance.AllocationCallbacks, deviceMarshalled.Address);
                if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));

                device = deviceMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                deviceMarshalled.Dispose();

                qInfosMarshalled.Dispose();

                foreach (var m in marshalled) m.Dispose();
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
            
            Instance.Commands.destroyDevice(device, Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class DeviceException : Exception {
        public DeviceException(string message) : base(message) { }
    }
}
