using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class PipelineLayoutCreateInfo {
        public IList<DescriptorSetLayout> setLayouts;
        public IList<VkPushConstantRange> pushConstantRanges;
    }

    public class PipelineLayout : IDisposable, INative<VkPipelineLayout> {
        VkPipelineLayout pipelineLayout;
        bool disposed = false;

        public Device Device { get; private set; }
        public IList<DescriptorSetLayout> Layouts { get; private set; }
        public IList<VkPushConstantRange> PushConstantRanges { get; private set; }

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

            if (info.setLayouts != null) Layouts = new List<DescriptorSetLayout>(info.setLayouts).AsReadOnly();
            if (info.pushConstantRanges != null) PushConstantRanges = new List<VkPushConstantRange>(info.pushConstantRanges).AsReadOnly();
        }

        void CreateLayout(PipelineLayoutCreateInfo mInfo) {
            unsafe {
                int layoutCount = 0;
                if (mInfo.setLayouts != null) layoutCount = mInfo.setLayouts.Count;

                int pushConstantsCount = 0;
                if (mInfo.pushConstantRanges != null) pushConstantsCount = mInfo.pushConstantRanges.Count;

                VkPipelineLayoutCreateInfo info = new VkPipelineLayoutCreateInfo();
                info.sType = VkStructureType.PipelineLayoutCreateInfo;

                var layoutsNative = stackalloc VkDescriptorSetLayout[layoutCount];
                if (mInfo.setLayouts != null) Interop.Marshal<VkDescriptorSetLayout, DescriptorSetLayout>(mInfo.setLayouts, layoutsNative);

                info.setLayoutCount = (uint)layoutCount;
                info.pSetLayouts = (IntPtr)layoutsNative;

                var pushConstantsNative = stackalloc VkPushConstantRange[pushConstantsCount];
                if (mInfo.pushConstantRanges != null) Interop.Copy(mInfo.pushConstantRanges, (IntPtr)pushConstantsNative);

                info.pushConstantRangeCount = (uint)pushConstantsCount;
                info.pPushConstantRanges = (IntPtr)pushConstantsNative;
                
                var result = Device.Commands.createPipelineLayout(Device.Native, ref info, Device.Instance.AllocationCallbacks, out pipelineLayout);
                if (result != VkResult.Success) throw new PipelineLayoutException(result, string.Format("Error creating pipeline layout: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;
            Device.Commands.destroyPipelineLayout(Device.Native, pipelineLayout, Device.Instance.AllocationCallbacks);
            disposed = true;
        }

        ~PipelineLayout() {
            Dispose(false);
        }
    }

    public class PipelineLayoutException : VulkanException {
        public PipelineLayoutException(VkResult result, string message) : base(result, message) { }
    }
}
