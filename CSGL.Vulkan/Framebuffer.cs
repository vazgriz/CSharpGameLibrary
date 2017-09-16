using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class FramebufferCreateInfo {
        public RenderPass renderPass;
        public IList<ImageView> attachments;
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
        public RenderPass RenderPass { get; private set; }
        public IList<ImageView> Attachments { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint Layers { get; private set; }

        public Framebuffer(Device device, FramebufferCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateFramebuffer(info);

            RenderPass = info.renderPass;
            Attachments = info.attachments.CloneReadOnly();
            Width = info.width;
            Height = info.height;
            Layers = info.layers;
        }

        void CreateFramebuffer(FramebufferCreateInfo mInfo) {
            unsafe {
                int attachmentCount = 0;
                if (mInfo.attachments != null) attachmentCount = mInfo.attachments.Count;

                VkFramebufferCreateInfo info = new VkFramebufferCreateInfo();
                info.sType = VkStructureType.FramebufferCreateInfo;
                info.renderPass = mInfo.renderPass.Native;

                var attachmentsNative = stackalloc VkImageView[attachmentCount];
                if (mInfo.attachments != null) Interop.Marshal<VkImageView, ImageView>(mInfo.attachments, attachmentsNative);

                info.attachmentCount = (uint)attachmentCount;
                info.pAttachments = (IntPtr)attachmentsNative;

                info.width = mInfo.width;
                info.height = mInfo.height;
                info.layers = mInfo.layers;
                
                var result = Device.Commands.createFramebuffer(Device.Native, ref info, Device.Instance.AllocationCallbacks, out framebuffer);
                if (result != VkResult.Success) throw new FramebufferException(result, string.Format("Error creating framebuffer: {0}", result));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyFramebuffer(Device.Native, framebuffer, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~Framebuffer() {
            Dispose(false);
        }
    }

    public class FramebufferException : VulkanException {
        public FramebufferException(VkResult result, string message) : base(result, message) { }
    }
}
