using System;
using System.Runtime.InteropServices;

namespace CSGL.Vulkan.Managed {
    public class RenderPassCreateInfo {
        public VkAttachmentDescription[] Attachments;
        public VkSubpassDescription[] Subpasses;
        public VkSubpassDependency[] Dependencies;
    }

    public class RenderPass {
        VkRenderPass renderPass;

        public Device Device { get; private set; }

        public VkRenderPass Native {
            get {
                return renderPass;
            }
        }

        public RenderPass(Device device, RenderPassCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;


        }

        void CreateRenderPass(RenderPassCreateInfo mInfo) {
            var info = new VkRenderPassCreateInfo();
            info.sType = VkStructureType.StructureTypeRenderPassCreateInfo;
            info.attachmentCount = (uint)mInfo.Attachments.Length;
            info.subpassCount = (uint)mInfo.Subpasses.Length;
            info.dependencyCount = (uint)mInfo.Dependencies.Length;
            info.pAttachments = mInfo.Attachments;
            info.pSubpasses = mInfo.Subpasses;
            info.pDependencies = mInfo.Dependencies;

            IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkRenderPassCreateInfo>());
            Marshal.StructureToPtr(info, infoPtr,false);

            IntPtr renderPassPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkRenderPass>());

            try {
                var result = Device.Commands.createRenderPass(Device.Native, infoPtr, Device.Instance.AllocationCallbacks, renderPassPtr);
                if (result != VkResult.Success) throw new RenderPassException(string.Format("Error creating render pass: {0}"));

                renderPass = Marshal.PtrToStructure<VkRenderPass>(renderPassPtr);
            }
            finally {
                Marshal.DestroyStructure<VkRenderPassCreateInfo>(infoPtr);

                Marshal.FreeHGlobal(infoPtr);
                Marshal.FreeHGlobal(renderPassPtr);
            }
        }
    }

    public class RenderPassException : Exception {
        public RenderPassException(string message) : base(message) { }
    }
}
