using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class PipelineLayoutCreateInfo {
        public IList<VkDescriptorSetLayout> setLayouts;
        public IList<Unmanaged.VkPushConstantRange> pushConstantRanges;
    }

    public class VkPipelineLayout : IDisposable, INative<Unmanaged.VkPipelineLayout> {
        Unmanaged.VkPipelineLayout pipelineLayout;
        bool disposed = false;

        public VkDevice Device { get; private set; }
        public IList<VkDescriptorSetLayout> Layouts { get; private set; }
        public IList<Unmanaged.VkPushConstantRange> PushConstantRanges { get; private set; }

        public Unmanaged.VkPipelineLayout Native {
            get {
                return pipelineLayout;
            }
        }

        public VkPipelineLayout(VkDevice device, PipelineLayoutCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;
            CreateLayout(info);

            Layouts = info.setLayouts.CloneReadOnly();
            PushConstantRanges = info.pushConstantRanges.CloneReadOnly();
        }

        void CreateLayout(PipelineLayoutCreateInfo mInfo) {
            unsafe {
                int layoutCount = 0;
                if (mInfo.setLayouts != null) layoutCount = mInfo.setLayouts.Count;

                int pushConstantsCount = 0;
                if (mInfo.pushConstantRanges != null) pushConstantsCount = mInfo.pushConstantRanges.Count;

                var info = new Unmanaged.VkPipelineLayoutCreateInfo();
                info.sType = VkStructureType.PipelineLayoutCreateInfo;

                var layoutsNative = stackalloc Unmanaged.VkDescriptorSetLayout[layoutCount];
                if (mInfo.setLayouts != null) Interop.Marshal<Unmanaged.VkDescriptorSetLayout, VkDescriptorSetLayout>(mInfo.setLayouts, layoutsNative);

                info.setLayoutCount = (uint)layoutCount;
                info.pSetLayouts = (IntPtr)layoutsNative;

                var pushConstantsNative = stackalloc Unmanaged.VkPushConstantRange[pushConstantsCount];
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

        ~VkPipelineLayout() {
            Dispose(false);
        }
    }

    public class PipelineLayoutException : VulkanException {
        public PipelineLayoutException(VkResult result, string message) : base(result, message) { }
    }
}
