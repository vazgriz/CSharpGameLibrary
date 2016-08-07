using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class DeviceCreateInfo {
        public List<string> Extensions { get; set; }
        public List<QueueCreateInfo> QueuesCreateInfos { get; set; }
        public VkPhysicalDeviceFeatures Features { get; set; }
        public bool FeaturesSet { get; set; }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;
        }

        public DeviceCreateInfo(List<string> extensions, List<QueueCreateInfo> queueCreateInfos, VkPhysicalDeviceFeatures features) {
            Extensions = extensions;
            QueuesCreateInfos = queueCreateInfos;
            Features = features;
            FeaturesSet = true;
        }
    }
}
