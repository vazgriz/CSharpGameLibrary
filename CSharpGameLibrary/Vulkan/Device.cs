using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceCreateInfo {
        public string[] extensions;
        public DeviceQueueCreateInfo[] queueCreateInfos;
        public VkPhysicalDeviceFeatures features;

        public DeviceCreateInfo(string[] extensions, DeviceQueueCreateInfo[] queueCreateInfos, VkPhysicalDeviceFeatures features) {
            this.extensions = extensions;
            this.queueCreateInfos = queueCreateInfos;
            this.features = features;
        }
    }

    public partial class Device : IDisposable, INative<VkDevice> {
        VkDevice device;
        bool disposed = false;

        Dictionary<VkQueue, Queue> queues;

        vkGetDeviceProcAddrDelegate getDeviceProcAddr;

        public DeviceCommands Commands { get; private set; }

        public Instance Instance { get; private set; }
        public PhysicalDevice PhysicalDevice { get; set; }

        public List<string> Extensions { get; private set; }

        public VkDevice Native {
            get {
                return device;
            }
        }

        public Device(PhysicalDevice physicalDevice, DeviceCreateInfo info) {
            if (physicalDevice == null) throw new ArgumentNullException(nameof(physicalDevice));
            if (info == null) throw new ArgumentNullException(nameof(info));

            PhysicalDevice = physicalDevice;
            Instance = physicalDevice.Instance;
            queues = new Dictionary<VkQueue, Queue>();

            if (info.extensions == null) {
                Extensions = new List<string>();
            } else {
                Extensions = new List<string>(info.extensions);
            }

            ValidateExtensions();

            CreateDevice(info);

            Vulkan.Load(ref getDeviceProcAddr, Instance);
            Commands = new DeviceCommands(this);
        }

        void CreateDevice(DeviceCreateInfo mInfo) {
            var extensionsMarshalled = new MarshalledStringArray(mInfo.extensions);
            MarshalledArray<VkDeviceQueueCreateInfo> queueInfos = null;
            PinnedArray<float>[] prioritiesMarshalled = null;
            Marshalled<VkPhysicalDeviceFeatures> features = new Marshalled<VkPhysicalDeviceFeatures>(mInfo.features);

            var info = new VkDeviceCreateInfo();
            info.sType = VkStructureType.StructureTypeDeviceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsMarshalled.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.pEnabledFeatures = features.Address;

            if (mInfo.queueCreateInfos != null) {
                int length = mInfo.queueCreateInfos.Length;
                info.queueCreateInfoCount = (uint)length;
                queueInfos = new MarshalledArray<VkDeviceQueueCreateInfo>(length);
                prioritiesMarshalled = new PinnedArray<float>[length];

                for (int i = 0; i < length; i++) {
                    var mi = mInfo.queueCreateInfos[i];
                    var qInfo = new VkDeviceQueueCreateInfo();
                    qInfo.sType = VkStructureType.StructureTypeDeviceQueueCreateInfo;

                    prioritiesMarshalled[i] = new PinnedArray<float>(mi.priorities);
                    qInfo.pQueuePriorities = prioritiesMarshalled[i].Address;
                    qInfo.queueCount = mi.queueCount;
                    qInfo.queueFamilyIndex = mi.queueFamilyIndex;

                    queueInfos[i] = qInfo;
                }

                info.pQueueCreateInfos = queueInfos.Address;
            }

            try {
                var result = Instance.Commands.createDevice(PhysicalDevice.Native, ref info, Instance.AllocationCallbacks, out device);
                if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));
            }
            finally {
                extensionsMarshalled.Dispose();
                queueInfos?.Dispose();
                features.Dispose();

                for (int i = 0; i < prioritiesMarshalled.Length; i++) {
                    prioritiesMarshalled[i].Dispose();
                }
            }
        }

        void ValidateExtensions() {
            foreach (string ex in Extensions) {
                bool found = false;

                for (int i = 0; i < PhysicalDevice.AvailableExtensions.Count; i++) {
                    if (PhysicalDevice.AvailableExtensions[i].Name == ex) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new DeviceException(string.Format("Requested extension not available: {0}", ex));
            }
        }

        public Queue GetQueue(uint familyIndex, uint index) {
            VkQueue temp;
            Commands.getDeviceQueue(device, familyIndex, index, out temp);
            if (queues.ContainsKey(temp)) {
                return queues[temp];
            } else {
                var result = new Queue(this, temp);
                queues.Add(temp, result);

                return result;
            }
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
