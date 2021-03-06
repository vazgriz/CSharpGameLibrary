﻿using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkFramebufferCreateInfo {
        public VkRenderPass renderPass;
        public IList<VkImageView> attachments;
        public int width;
        public int height;
        public int layers;
    }

    public class VkFramebuffer : IDisposable, INative<Unmanaged.VkFramebuffer> {
        Unmanaged.VkFramebuffer framebuffer;
        bool disposed = false;

        public Unmanaged.VkFramebuffer Native {
            get {
                return framebuffer;
            }
        }

        public VkDevice Device { get; private set; }
        public VkRenderPass RenderPass { get; private set; }
        public IList<VkImageView> Attachments { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Layers { get; private set; }

        public VkFramebuffer(VkDevice device, VkFramebufferCreateInfo info) {
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

        void CreateFramebuffer(VkFramebufferCreateInfo mInfo) {
            unsafe {
                int attachmentCount = 0;
                if (mInfo.attachments != null) attachmentCount = mInfo.attachments.Count;

                var info = new Unmanaged.VkFramebufferCreateInfo();
                info.sType = VkStructureType.FramebufferCreateInfo;
                info.renderPass = mInfo.renderPass.Native;

                var attachmentsNative = stackalloc Unmanaged.VkImageView[attachmentCount];
                if (mInfo.attachments != null) Interop.Marshal<Unmanaged.VkImageView, VkImageView>(mInfo.attachments, attachmentsNative);

                info.attachmentCount = (uint)attachmentCount;
                info.pAttachments = (IntPtr)attachmentsNative;

                info.width = (uint)mInfo.width;
                info.height = (uint)mInfo.height;
                info.layers = (uint)mInfo.layers;
                
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

        ~VkFramebuffer() {
            Dispose(false);
        }
    }

    public class FramebufferException : VulkanException {
        public FramebufferException(VkResult result, string message) : base(result, message) { }
    }
}
