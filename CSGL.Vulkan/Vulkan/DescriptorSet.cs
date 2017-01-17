using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorSetAllocateInfo {
        public uint descriptorSetCount;
        public List<DescriptorSetLayout> setLayouts;
    }

    public class DescriptorSet : INative<VkDescriptorSet> {
        VkDescriptorSet descriptorSet;

        public Device Device { get; private set; }
        public DescriptorPool Pool { get; private set; }

        public VkDescriptorSet Native {
            get {
                return descriptorSet;
            }
        }

        internal DescriptorSet(Device device, DescriptorPool pool, VkDescriptorSet descriptorSet) {
            Device = device;
            Pool = pool;
            this.descriptorSet = descriptorSet;
        }
    }
}
