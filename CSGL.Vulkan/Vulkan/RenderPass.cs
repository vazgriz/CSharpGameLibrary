using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class RenderPassCreateInfo {
        public List<VkAttachmentDescription> attachments;
        public List<SubpassDescription> subpasses;
        public List<VkSubpassDependency> dependencies;
    }

    public class SubpassDescription {
        public VkPipelineBindPoint PipelineBindPoint { get; set; }
        public List<VkAttachmentReference> InputAttachments { get; set; }
        public List<VkAttachmentReference> ColorAttachments { get; set; }
        public List<VkAttachmentReference> ResolveAttachments { get; set; }
        public List<uint> PreserveAttachments { get; set; }

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

        internal VkSubpassDescription GetNative(DisposableList<IDisposable> marshalled) {
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
                result.preserveAttachmentCount = (uint)PreserveAttachments.Count;
                var preserveAttachmentsMarshalled = new NativeArray<uint>(PreserveAttachments);
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
            info.sType = VkStructureType.RenderPassCreateInfo;
            var marshalledArrays = new DisposableList<IDisposable>();

            var attachMarshalled = new MarshalledArray<VkAttachmentDescription>(mInfo.attachments);
            info.attachmentCount = (uint)attachMarshalled.Count;
            info.pAttachments = attachMarshalled.Address;

            var subpasses = new VkSubpassDescription[mInfo.subpasses.Count];
            for (int i = 0; i < subpasses.Length; i++) {
                subpasses[i] = mInfo.subpasses[i].GetNative(marshalledArrays);
            }

            var subpassMarshalled = new MarshalledArray<VkSubpassDescription>(subpasses);
            info.subpassCount = (uint)subpassMarshalled.Count;
            info.pSubpasses = subpassMarshalled.Address;

            var dependMarshalled = new MarshalledArray<VkSubpassDependency>(mInfo.dependencies);
            info.dependencyCount = (uint)dependMarshalled.Count;
            info.pDependencies = dependMarshalled.Address;

            using (attachMarshalled)
            using (subpassMarshalled)
            using (dependMarshalled)
            using (marshalledArrays)  {
                var result = Device.Commands.createRenderPass(Device.Native, ref info, Device.Instance.AllocationCallbacks, out renderPass);
                if (result != VkResult.Success) throw new RenderPassException(string.Format("Error creating render pass: {0}"));
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
