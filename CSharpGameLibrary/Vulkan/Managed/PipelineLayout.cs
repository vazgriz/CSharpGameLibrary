using System;

namespace CSGL.Vulkan.Managed {
    public class PipelineLayoutCreateInfo {
        public VkDescriptorSetLayout[] SetLayouts { get; set; }
        public VkPushConstantRange[] PushConstantRanges { get; set; }
    }

    public class PipelineLayout : IDisposable {
        VkPipelineLayout pipelineLayout;
        bool disposed = false;

        public Device Device { get; private set; }

        public VkPipelineLayout Native {
            get {
                return pipelineLayout;
            }
        }

        public PipelineLayout(Device device, PipelineLayoutCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;
            CreateLayout(info);
        }

        void CreateLayout(PipelineLayoutCreateInfo mInfo) {
            VkPipelineLayoutCreateInfo info = new VkPipelineLayoutCreateInfo();
            info.sType = VkStructureType.StructureTypePipelineLayoutCreateInfo;

            var layoutsMarshalled = new MarshalledArray<VkDescriptorSetLayout>(mInfo.SetLayouts);
            info.setLayoutCount = (uint)layoutsMarshalled.Count;
            info.pSetLayouts = layoutsMarshalled.Address;

            var pushConstantsMarshalled = new MarshalledArray<VkPushConstantRange>(mInfo.PushConstantRanges);
            info.pushConstantRangeCount = (uint)pushConstantsMarshalled.Count;
            info.pPushConstantRanges = pushConstantsMarshalled.Address;
            
            var infoMarshalled = new Marshalled<VkPipelineLayoutCreateInfo>(info);
            var pipelineLayoutMarshalled = new Marshalled<VkPipelineLayout>();

            try {
                var result = Device.Commands.createPipelineLayout(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, pipelineLayoutMarshalled.Address);
                if (result != VkResult.Success) throw new PipelineLayoutException(string.Format("Error creating pipeline layout: {0}", result));

                pipelineLayout = pipelineLayoutMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                pipelineLayoutMarshalled.Dispose();
                layoutsMarshalled.Dispose();
                pushConstantsMarshalled.Dispose();
            }
        }

        public void Dispose() {
            if (disposed) return;
            Device.Commands.destroyPipelineLayout(Device.Native, pipelineLayout, Device.Instance.AllocationCallbacks);
            disposed = true;
        }
    }

    public class PipelineLayoutException : Exception {
        public PipelineLayoutException(string message) : base(message) { }
    }
}
