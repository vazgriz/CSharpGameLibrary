using System;

namespace CSGL.Vulkan.Managed {
    public class FramebufferCreateInfo {
        public RenderPass RenderPass { get; set; }
        public ImageView[] Attachments { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Layers { get; set; }
    }

    public class Framebuffer : IDisposable {
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
        }

        void CreateFramebuffer(FramebufferCreateInfo mInfo) {
            VkFramebufferCreateInfo info = new VkFramebufferCreateInfo();
            info.sType = VkStructureType.StructureTypeFramebufferCreateInfo;
            info.renderPass = mInfo.RenderPass.Native;
            info.attachmentCount = (uint)mInfo.Attachments.Length;
            
            var attachmentsMarshalled = new MarshalledArray<VkImageView>(mInfo.Attachments.Length);
            for (int i = 0; i < attachmentsMarshalled.Count; i++) {
                attachmentsMarshalled[i] = mInfo.Attachments[i].Native;
            }
            info.pAttachments = attachmentsMarshalled.Address;
            info.width = mInfo.Width;
            info.height = mInfo.Height;
            info.layers = mInfo.Layers;

            var infoMarshalled = new Marshalled<VkFramebufferCreateInfo>(info);
            var framebufferMarshalled = new Marshalled<VkFramebuffer>();

            try {
                var result = Device.Commands.createFramebuffer(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, framebufferMarshalled.Address);
                if (result != VkResult.Success) throw new FramebufferException(string.Format("Error creating framebuffer: {0}", result));
                framebuffer = framebufferMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                framebufferMarshalled.Dispose();
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
