using System;

namespace CSGL.Vulkan {
    public class FramebufferCreateInfo {
        public RenderPass renderPass;
        public ImageView[] attachments;
        public uint width;
        public uint height;
        public uint layers;
    }

    public class Framebuffer : IDisposable, INative<VkFramebuffer> {
        VkFramebuffer framebuffer;
        bool disposed = false;

        public VkFramebuffer Native {
            get {
                return framebuffer;
            }
        }

        public Device Device { get; private set; }

        public Framebuffer(Device device, FramebufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateFramebuffer(info);
        }

        void CreateFramebuffer(FramebufferCreateInfo mInfo) {
            VkFramebufferCreateInfo info = new VkFramebufferCreateInfo();
            info.sType = VkStructureType.StructureTypeFramebufferCreateInfo;
            info.renderPass = mInfo.renderPass.Native;
           
            var attachmentsMarshalled = new MarshalledArray<VkImageView>(mInfo.attachments.Length);
            for (int i = 0; i < attachmentsMarshalled.Count; i++) {
                attachmentsMarshalled[i] = mInfo.attachments[i].Native;
            }
            info.attachmentCount = (uint)mInfo.attachments.Length; 
            info.pAttachments = attachmentsMarshalled.Address;

            info.width = mInfo.width;
            info.height = mInfo.height;
            info.layers = mInfo.layers;

            try {
                var result = Device.Commands.createFramebuffer(Device.Native, ref info, Device.Instance.AllocationCallbacks, out framebuffer);
                if (result != VkResult.Success) throw new FramebufferException(string.Format("Error creating framebuffer: {0}", result));
            }
            finally {
                attachmentsMarshalled.Dispose();
            }
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyFramebuffer(Device.Native, framebuffer, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class FramebufferException : Exception {
        public FramebufferException(string message) : base(message) { }
    }
}
