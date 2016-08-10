using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class QueueCreateInfo {
        public uint QueueFamilyIndex { get; set; }
        public uint QueueCount { get; set; }
        public float[] Priorities { get; set; }

        public QueueCreateInfo(uint queueFamilyIndex, uint queueCount, float[] priorities) {
            QueueFamilyIndex = queueFamilyIndex;
            QueueCount = queueCount;
            Priorities = priorities;
        }
    }

    public class Queue {
        VkQueue queue;

        Device device;

        internal Queue(Device device, VkQueue queue) {
            this.device = device;
            this.queue = queue;
        }

        public Device Device {
            get {
                return device;
            }
        }
    }
}
