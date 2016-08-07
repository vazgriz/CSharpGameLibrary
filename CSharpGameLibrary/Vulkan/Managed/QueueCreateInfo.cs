using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class QueueCreateInfo {
        //public VkDeviceQueueCreateFlags Flags { get; set; }
        public uint QueueFamilyIndex { get; set; }
        public uint QueueCount { get; set; }
        public float[] Priorities { get; set; }

        public QueueCreateInfo(uint queueFamilyIndex, uint queueCount, float[] priorities) {
            //Flags = flags;
            QueueFamilyIndex = queueFamilyIndex;
            QueueCount = queueCount;
            Priorities = priorities;
        }
    }
}
