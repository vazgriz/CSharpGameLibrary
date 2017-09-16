using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkDescriptorSetLayoutCreateInfo {
        public IList<Unmanaged.VkDescriptorSetLayoutBinding> bindings;
    }

    public class VkDescriptorSetLayout : IDisposable, INative<Unmanaged.VkDescriptorSetLayout> {
        Unmanaged.VkDescriptorSetLayout descriptorSetLayout;

        bool disposed;

        public VkDevice Device { get; private set; }
        public IList<Unmanaged.VkDescriptorSetLayoutBinding> Bindings { get; private set; }

        public Unmanaged.VkDescriptorSetLayout Native {
            get {
                return descriptorSetLayout;
            }
        }

        public VkDescriptorSetLayout(VkDevice device, VkDescriptorSetLayoutCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateDescriptorSetLayout(info);

            Bindings = info.bindings.CloneReadOnly();
        }

        void CreateDescriptorSetLayout(VkDescriptorSetLayoutCreateInfo mInfo) {
            if (mInfo.bindings == null) throw new ArgumentNullException(nameof(mInfo.bindings));

            var info = new Unmanaged.VkDescriptorSetLayoutCreateInfo();
            info.sType = VkStructureType.DescriptorSetLayoutCreateInfo;

            var bindingsMarshalled = new MarshalledArray<Unmanaged.VkDescriptorSetLayoutBinding>(mInfo.bindings);
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

        ~VkDescriptorSetLayout() {
            Dispose(false);
        }
    }

    public class DescriptorSetLayoutException : VulkanException {
        public DescriptorSetLayoutException(VkResult result, string message) : base(result, message) { }
    }
}
