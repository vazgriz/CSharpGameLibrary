using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceCreateInfo {
        public IList<string> extensions;
        public IList<DeviceQueueCreateInfo> queueCreateInfos;
        public VkPhysicalDeviceFeatures features;
    }

    public partial class Device : IDisposable, INative<VkDevice> {
        VkDevice device;
        bool disposed = false;

        Dictionary<QueueID, Queue> queues;

        vkGetDeviceProcAddrDelegate getDeviceProcAddr;

        public DeviceCommands Commands { get; private set; }

        public Instance Instance { get; private set; }
        public PhysicalDevice PhysicalDevice { get; set; }

        public IList<string> Extensions { get; private set; }

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
                Extensions = new List<string>().AsReadOnly();
            } else {
                Extensions = new List<string>(info.extensions).AsReadOnly();
            }

            ValidateExtensions();

            CreateDevice(info);

            Vulkan.Load(ref getDeviceProcAddr, Instance);
            Commands = new DeviceCommands(this);

            GetQueues(info);
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
                if (result != VkResult.Success) throw new DeviceException(result, string.Format("Error creating device: {0}", result));
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

        void GetQueues(DeviceCreateInfo info) {
            for (int i = 0; i < info.queueCreateInfos.Count; i++) {
                var queueInfo = info.queueCreateInfos[i];
                for (int j = 0; j < (int)queueInfo.queueCount; j++) {
                    QueueID id = new QueueID(queueInfo.queueFamilyIndex, (uint)j);
                    VkQueue temp;
                    Commands.getDeviceQueue(device, id.familyIndex, id.index, out temp);

                    var queue = new Queue(this, temp, id.familyIndex, queueInfo.priorities[j]);
                    queues.Add(id, queue);
                }
            }
        }

        public Queue GetQueue(uint familyIndex, uint index) {
            QueueID id = new QueueID(familyIndex, index);
            if (queues.ContainsKey(id)) {
                return queues[id];
            }
            throw new DeviceException("Requested queue does not exist");
        }

        public IntPtr GetProcAdddress(string command) {
            return getDeviceProcAddr(device, Interop.GetUTF8(command));
        }

        public void WaitIdle() {
            Commands.waitDeviceIdle(device);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Instance.Commands.destroyDevice(device, Instance.AllocationCallbacks);

            disposed = true;
        }

        ~Device() {
            Dispose(false);
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

    public class DeviceException : VulkanException {
        public DeviceException(string message) : base(message) { }
        public DeviceException(VkResult result, string message) : base(result, message) { }
    }
}
