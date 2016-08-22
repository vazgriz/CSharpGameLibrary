using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan.Managed {
    public class PipelineLayoutCreateInfo {
        public VkDescriptorSetLayout[] pSetLayouts { get; set; }
        public VkPushConstantRange[] pPushConstantRanges { get; set; }
    }

    public class PipelineLayout : IDisposable {
        VkPipelineLayout pipelineLayout;

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

            info.setLayoutCount = (uint)mInfo.pSetLayouts.Length;
            info.pSetLayouts = mInfo.pSetLayouts;
            info.pushConstantRangeCount = (uint)mInfo.pPushConstantRanges.Length;
            info.pPushConstantRanges = mInfo.pPushConstantRanges;

            IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineLayoutCreateInfo>());
            Marshal.StructureToPtr(info, infoPtr, false);

            IntPtr pipelineLayoutPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkPipelineLayout>());

            try {
                var result = Device.Commands.createPipelineLayout(Device.Native, infoPtr, Device.Instance.AllocationCallbacks, pipelineLayoutPtr);
                if (result != VkResult.Success) throw new PipelineLayoutException(string.Format("Error creating pipeline layout: {0}", result));

                pipelineLayout = Marshal.PtrToStructure<VkPipelineLayout>(pipelineLayoutPtr);
            }
            finally {
                Marshal.DestroyStructure<VkPipelineLayoutCreateInfo>(infoPtr);

                Marshal.FreeHGlobal(infoPtr);
                Marshal.FreeHGlobal(pipelineLayoutPtr);
            }
        }

        public void Dispose() {
            Device.Commands.destroyPipelineLayout(Device.Native, pipelineLayout, Device.Instance.AllocationCallbacks);
        }
    }

    public class PipelineLayoutException : Exception {
        public PipelineLayoutException(string message) : base(message) { }
    }
}
