using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class DeviceCreateInfo : IDisposable {
        List<string> extensions;
        List<QueueCreateInfo> queueCreateInfos;
        VkPhysicalDeviceFeatures features;
        bool featuresSet = false;

        bool disposed = false;
        bool dirty = true;

        Marshalled<VkDeviceCreateInfo> marshalled;
        MarshalledArray<VkDeviceQueueCreateInfo> queuesMarshalled;
        MarshalledStringArray extensionsMarshalled;
        Marshalled<VkPhysicalDeviceFeatures> featuresMarshalled;

        public List<string> Extensions {
            get {
                return extensions;
            }
            set {
                extensions = value;
                dirty = true;
            }
        }

        public List<QueueCreateInfo> QueuesCreateInfos {
            get {
                return queueCreateInfos;
            }
            set {
                queueCreateInfos = value;
                dirty = true;
            }
        }

        public VkPhysicalDeviceFeatures Features {
            get {
                return features;
            }
            set {
                features = value;
                dirty = true;
            }
        }

        public Marshalled<VkDeviceCreateInfo> Marshalled {
            get {
                if (dirty) {
                    Apply();
                }
                return marshalled;
            }
        }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;

            marshalled = new Marshalled<VkDeviceCreateInfo>();

            Apply();
        }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos, VkPhysicalDeviceFeatures features) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;
            Features = features;
            featuresSet = true;

            marshalled = new Marshalled<VkDeviceCreateInfo>();

            Apply();
        }

        public void Apply() {
            queuesMarshalled?.Dispose();
            featuresMarshalled?.Dispose();
            queuesMarshalled = new MarshalledArray<VkDeviceQueueCreateInfo>(queueCreateInfos.Count);

            for (int i = 0; i < queueCreateInfos.Count; i++) {
                queuesMarshalled[i] = queueCreateInfos[i].Marshalled.Value;
            }

            if (extensions == null) extensions = new List<string>();
            extensionsMarshalled = new MarshalledStringArray(extensions);

            if (featuresSet) {
                featuresMarshalled = new Marshalled<VkPhysicalDeviceFeatures>(features);
            }

            marshalled.Value = GetNative();
        }

        VkDeviceCreateInfo GetNative() {
            VkDeviceCreateInfo info = new VkDeviceCreateInfo();
            info.sType = VkStructureType.StructureTypeDeviceCreateInfo;
            info.enabledExtensionCount = (uint)extensions.Count;
            info.ppEnabledExtensionNames = extensionsMarshalled.Address;
            info.queueCreateInfoCount = (uint)QueuesCreateInfos.Count;
            info.pQueueCreateInfos = queuesMarshalled.Address;
            if (featuresSet) info.pEnabledFeatures = featuresMarshalled.Address;

            return info;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            marshalled.Dispose();
            queuesMarshalled.Dispose();
            extensionsMarshalled.Dispose();
            if (featuresSet) featuresMarshalled.Dispose();

            if (disposing) {
                extensions = null;
                queueCreateInfos = null;

                marshalled = null;
                queuesMarshalled = null;
                extensionsMarshalled = null;
                featuresMarshalled = null;
            }
        }

        ~DeviceCreateInfo() {
            Dispose(false);
        }
    }
}
