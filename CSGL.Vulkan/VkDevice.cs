using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkDeviceCreateInfo {
        public IList<string> extensions;
        public IList<VkDeviceQueueCreateInfo> queueCreateInfos;
        public VkPhysicalDeviceFeatures features;
    }

    public partial class VkDevice : IDisposable, INative<Unmanaged.VkDevice> {
        Unmanaged.VkDevice device;
        bool disposed = false;

        Dictionary<QueueID, VkQueue> queues;

        public DeviceCommands Commands { get; private set; }

        public VkInstance Instance { get; private set; }
        public VkPhysicalDevice PhysicalDevice { get; private set; }
        public VkPhysicalDeviceFeatures Features { get; private set; }

        public IList<VkExtension> Extensions { get; private set; }

        public Unmanaged.VkDevice Native {
            get {
                return device;
            }
        }

        public VkDevice(VkPhysicalDevice physicalDevice, VkDeviceCreateInfo info) {
            if (physicalDevice == null) throw new ArgumentNullException(nameof(physicalDevice));
            if (info == null) throw new ArgumentNullException(nameof(info));

            PhysicalDevice = physicalDevice;
            Instance = physicalDevice.Instance;
            queues = new Dictionary<QueueID, VkQueue>();
            
            ValidateExtensions(info.extensions);

            CreateDevice(info);
            
            Commands = new DeviceCommands(this);

            GetQueues(info);
            if (info.features != null) Features = new VkPhysicalDeviceFeatures(info.features);
        }

        void CreateDevice(VkDeviceCreateInfo mInfo) {
            if (mInfo.queueCreateInfos == null) throw new ArgumentNullException(nameof(mInfo.queueCreateInfos));

            var extensionsNative = new NativeStringArray(mInfo.extensions);
            Native<Unmanaged.VkPhysicalDeviceFeatures> featuresNative = null;

            var info = new Unmanaged.VkDeviceCreateInfo();
            info.sType = VkStructureType.DeviceCreateInfo;
            info.enabledExtensionCount = (uint)extensionsNative.Count;
            info.ppEnabledExtensionNames = extensionsNative.Address;

            if (mInfo.features != null) {
                featuresNative = new Native<Unmanaged.VkPhysicalDeviceFeatures>(mInfo.features.GetNative());
                info.pEnabledFeatures = featuresNative.Address;
            }
            
            int length = mInfo.queueCreateInfos.Count;
            info.queueCreateInfoCount = (uint)length;
            var queueInfosNative = new NativeArray<Unmanaged.VkDeviceQueueCreateInfo>(length);
            var prioritiesNative = new DisposableList<NativeArray<float>>(length);

            for (int i = 0; i < length; i++) {
                var mi = mInfo.queueCreateInfos[i];
                var qInfo = new Unmanaged.VkDeviceQueueCreateInfo();
                qInfo.sType = VkStructureType.DeviceQueueCreateInfo;

                var priorityNative = new NativeArray<float>(mi.priorities);
                prioritiesNative.Add(priorityNative);
                qInfo.pQueuePriorities = priorityNative.Address;
                qInfo.queueCount = (uint)mi.queueCount;
                qInfo.queueFamilyIndex = (uint)mi.queueFamilyIndex;

                queueInfosNative[i] = qInfo;
            }

            info.pQueueCreateInfos = queueInfosNative.Address;

            using (extensionsNative)
            using (queueInfosNative)
            using (featuresNative)
            using (prioritiesNative) {
                var result = Instance.Commands.createDevice(PhysicalDevice.Native, ref info, Instance.AllocationCallbacks, out device);
                if (result != VkResult.Success) throw new DeviceException(result, string.Format("Error creating device: {0}", result));
            }
        }

        void ValidateExtensions(IList<string> requested) {
            var extensions = new List<VkExtension>();

            if (requested != null) {
                foreach (string ex in requested) {
                    bool found = false;

                    foreach (var available in PhysicalDevice.AvailableExtensions) {
                        if (available.Name == ex) {
                            found = true;
                            extensions.Add(available);
                            break;
                        }
                    }

                    if (!found) throw new DeviceException(string.Format("Requested extension not available: {0}", ex));
                }
            }

            Extensions = extensions.AsReadOnly();
        }

        void GetQueues(VkDeviceCreateInfo info) {
            for (int i = 0; i < info.queueCreateInfos.Count; i++) {
                var queueInfo = info.queueCreateInfos[i];
                for (int j = 0; j < queueInfo.queueCount; j++) {
                    QueueID id = new QueueID((uint)queueInfo.queueFamilyIndex, (uint)j);
                    Unmanaged.VkQueue temp;
                    Commands.getDeviceQueue(device, id.familyIndex, id.index, out temp);

                    var queue = new VkQueue(this, temp, id.familyIndex, queueInfo.priorities[j]);
                    queues.Add(id, queue);
                }
            }
        }

        public VkQueue GetQueue(uint familyIndex, uint index) {
            QueueID id = new QueueID(familyIndex, index);
            if (queues.ContainsKey(id)) {
                return queues[id];
            }
            throw new DeviceException("Requested queue does not exist");
        }

        public IntPtr GetProcAdddress(string command) {
            return Commands.getProcAddr(device, Interop.GetUTF8(command));
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

        ~VkDevice() {
            Dispose(false);
        }

        struct QueueID : IEquatable<QueueID> {
            public uint familyIndex;
            public uint index;

            public QueueID(uint familyIndex, uint index) {
                this.familyIndex = familyIndex;
                this.index = index;
            }

            public override int GetHashCode() {
                return familyIndex.GetHashCode() ^ index.GetHashCode();
            }

            public bool Equals(QueueID other) {
                return familyIndex == other.familyIndex && index == other.index;
            }
        }
    }

    public class DeviceException : VulkanException {
        public DeviceException(string message) : base(message) { }
        public DeviceException(VkResult result, string message) : base(result, message) { }
    }
}
