using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceCreateInfo {
        public List<string> extensions;
        public List<DeviceQueueCreateInfo> queueCreateInfos;
        public VkPhysicalDeviceFeatures features;

        public DeviceCreateInfo(List<String> extensions, List<DeviceQueueCreateInfo> queueCreateInfos, VkPhysicalDeviceFeatures features) {
            this.extensions = extensions;
            this.queueCreateInfos = queueCreateInfos;
            this.features = features;
        }
    }

    public partial class Device : IDisposable, INative<VkDevice> {
        VkDevice device;
        bool disposed = false;

        Dictionary<QueueID, Queue> queues;

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
            queues = new Dictionary<QueueID, Queue>();

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
            var extensionsMarshalled = new NativeStringArray(mInfo.extensions);
            MarshalledArray<VkDeviceQueueCreateInfo> queueInfos = null;
            DisposableList<NativeArray<float>> prioritiesMarshalled = null;
            Marshalled<VkPhysicalDeviceFeatures> features = new Marshalled<VkPhysicalDeviceFeatures>(mInfo.features);

            var info = new VkDeviceCreateInfo();
            info.sType = VkStructureType.DeviceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsMarshalled.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.pEnabledFeatures = features.Address;

            if (mInfo.queueCreateInfos != null) {
                int length = mInfo.queueCreateInfos.Count;
                info.queueCreateInfoCount = (uint)length;
                queueInfos = new MarshalledArray<VkDeviceQueueCreateInfo>(length);
                prioritiesMarshalled = new DisposableList<NativeArray<float>>(length);

                for (int i = 0; i < length; i++) {
                    var mi = mInfo.queueCreateInfos[i];
                    var qInfo = new VkDeviceQueueCreateInfo();
                    qInfo.sType = VkStructureType.DeviceQueueCreateInfo;

                    var priorityMarshalled = new NativeArray<float>(mi.priorities);
                    prioritiesMarshalled.Add(priorityMarshalled);
                    qInfo.pQueuePriorities = priorityMarshalled.Address;
                    qInfo.queueCount = mi.queueCount;
                    qInfo.queueFamilyIndex = mi.queueFamilyIndex;

                    queueInfos[i] = qInfo;
                }

                info.pQueueCreateInfos = queueInfos.Address;
            }

            using (extensionsMarshalled)
            using (queueInfos)
            using (features)
            using (prioritiesMarshalled) {
                var result = Instance.Commands.createDevice(PhysicalDevice.Native, ref info, Instance.AllocationCallbacks, out device);
                if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));
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
            QueueID id = new QueueID(familyIndex, index);
            if (queues.ContainsKey(id)) {
                return queues[id];
            } else {
                VkQueue temp;
                Commands.getDeviceQueue(device, familyIndex, index, out temp);

                var result = new Queue(this, temp, familyIndex);
                queues.Add(id, result);

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

        struct QueueID {
            public uint familyIndex;
            public uint index;

            public QueueID(uint familyIndex, uint index) {
                this.familyIndex = familyIndex;
                this.index = index;
            }

            public override int GetHashCode() {
                return familyIndex.GetHashCode() ^ index.GetHashCode();
            }
        }
    }

    public class DeviceException : Exception {
        public DeviceException(string message) : base(message) { }
    }
}
