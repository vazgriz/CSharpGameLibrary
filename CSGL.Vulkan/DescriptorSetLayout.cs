using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class DescriptorSetLayoutCreateInfo {
        public IList<VkDescriptorSetLayoutBinding> bindings;
    }

    public class DescriptorSetLayout : IDisposable, INative<VkDescriptorSetLayout> {
        VkDescriptorSetLayout descriptorSetLayout;

        bool disposed;

        public Device Device { get; private set; }
        public IList<VkDescriptorSetLayoutBinding> Bindings { get; private set; }

        public VkDescriptorSetLayout Native {
            get {
                return descriptorSetLayout;
            }
        }

        public DescriptorSetLayout(Device device, DescriptorSetLayoutCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorSetLayout(info);

            if (info.bindings != null) Bindings = new List<VkDescriptorSetLayoutBinding>(info.bindings).AsReadOnly();
        }

        void CreateDescriptorSetLayout(DescriptorSetLayoutCreateInfo mInfo) {
            if (mInfo.bindings == null) throw new ArgumentNullException(nameof(mInfo.bindings));

            var info = new VkDescriptorSetLayoutCreateInfo();
            info.sType = VkStructureType.DescriptorSetLayoutCreateInfo;

            var bindingsMarshalled = new MarshalledArray<VkDescriptorSetLayoutBinding>(mInfo.bindings);
            info.bindingCount = (uint)bindingsMarshalled.Count;
            info.pBindings = bindingsMarshalled.Address;

            using (bindingsMarshalled) {
                var result = Device.Commands.createDescriptorSetLayout(Device.Native, ref info, Device.Instance.AllocationCallbacks, out descriptorSetLayout);
                if (result != VkResult.Success) throw new DescriptorSetLayoutException(result, string.Format("Error creating description set layout: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyDescriptorSetLayout(Device.Native, descriptorSetLayout, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~DescriptorSetLayout() {
            Dispose(false);
        }
    }

    public class DescriptorSetLayoutException : VulkanException {
        public DescriptorSetLayoutException(VkResult result, string message) : base(result, message) { }
    }
}
