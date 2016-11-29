using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class RenderPassCreateInfo {
        public VkAttachmentDescription[] attachments;
        public SubpassDescription[] subpasses;
        public VkSubpassDependency[] dependencies;
    }

    public class SubpassDescription {
        public VkPipelineBindPoint PipelineBindPoint { get; set; }
        public VkAttachmentReference[] InputAttachments { get; set; }
        public VkAttachmentReference[] ColorAttachments { get; set; }
        public VkAttachmentReference[] ResolveAttachments { get; set; }
        public uint[] PreserveAttachments { get; set; }

        VkAttachmentReference depthStencilAttachment;
        bool hasDepthStencil = false;
        public VkAttachmentReference DepthStencilAttachment {
            get {
                return depthStencilAttachment;
            }
            set {
                hasDepthStencil = true;
                depthStencilAttachment = value;
            }
        }

        internal VkSubpassDescription GetNative(List<IDisposable> marshalled) {
            var result = new VkSubpassDescription();

            result.pipelineBindPoint = PipelineBindPoint;

            var inputMarshalled = new MarshalledArray<VkAttachmentReference>(InputAttachments);
            result.inputAttachmentCount = (uint)inputMarshalled.Count;
            result.pInputAttachments = inputMarshalled.Address;

            var colorMarshalled = new MarshalledArray<VkAttachmentReference>(ColorAttachments);
            result.colorAttachmentCount = (uint)colorMarshalled.Count;
            result.pColorAttachments = colorMarshalled.Address;

            var resolveMarshalled = new MarshalledArray<VkAttachmentReference>(ResolveAttachments);
            result.pResolveAttachments = resolveMarshalled.Address;

            if (hasDepthStencil) {
                var depthMarshalled = new Marshalled<VkAttachmentReference>(DepthStencilAttachment);
                result.pDepthStencilAttachment = depthMarshalled.Address;
                marshalled.Add(depthMarshalled);
            }

            if (PreserveAttachments != null) {
                result.preserveAttachmentCount = (uint)PreserveAttachments.Length;
                var preserveAttachmentsMarshalled = new PinnedArray<uint>(PreserveAttachments);
                result.pPreserveAttachments = preserveAttachmentsMarshalled.Address;
                marshalled.Add(preserveAttachmentsMarshalled);
            }

            marshalled.Add(inputMarshalled);
            marshalled.Add(colorMarshalled);
            marshalled.Add(resolveMarshalled);

            return result;
        }
    }

    public class RenderPass : IDisposable, INative<VkRenderPass> {
        VkRenderPass renderPass;
        bool disposed = false;

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

            CreateRenderPass(info);
        }

        void CreateRenderPass(RenderPassCreateInfo mInfo) {
            var info = new VkRenderPassCreateInfo();
            info.sType = VkStructureType.StructureTypeRenderPassCreateInfo;
            var marshalledArrays = new List<IDisposable>();

            var attachMarshalled = new MarshalledArray<VkAttachmentDescription>(mInfo.attachments);
            info.attachmentCount = (uint)attachMarshalled.Count;
            info.pAttachments = attachMarshalled.Address;

            var subpasses = new VkSubpassDescription[mInfo.subpasses.Length];
            for (int i = 0; i < subpasses.Length; i++) {
                subpasses[i] = mInfo.subpasses[i].GetNative(marshalledArrays);
            }

            var subpassMarshalled = new MarshalledArray<VkSubpassDescription>(subpasses);
            info.subpassCount = (uint)subpassMarshalled.Count;
            info.pSubpasses = subpassMarshalled.Address;

            var dependMarshalled = new MarshalledArray<VkSubpassDependency>(mInfo.dependencies);
            info.dependencyCount = (uint)dependMarshalled.Count;
            info.pDependencies = dependMarshalled.Address;

            try {
                var result = Device.Commands.createRenderPass(Device.Native, ref info, Device.Instance.AllocationCallbacks, out renderPass);
                if (result != VkResult.Success) throw new RenderPassException(string.Format("Error creating render pass: {0}"));
            }
            finally {
                foreach (var m in marshalledArrays) {
                    m.Dispose();
                }

                attachMarshalled.Dispose();
                subpassMarshalled.Dispose();
                dependMarshalled.Dispose();
            }
        }

        public void Dispose() {
            if (disposed) return;

            Device.Commands.destroyRenderPass(Device.Native, renderPass, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class RenderPassException : Exception {
        public RenderPassException(string message) : base(message) { }
    }
}
