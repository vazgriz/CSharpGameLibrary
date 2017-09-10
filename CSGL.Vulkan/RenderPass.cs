using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class RenderPassCreateInfo {
        public List<AttachmentDescription> attachments;
        public List<SubpassDescription> subpasses;
        public List<SubpassDependency> dependencies;
    }

    public class AttachmentDescription {
        public VkAttachmentDescriptionFlags flags;
        public VkFormat format;
        public VkSampleCountFlags samples;
        public VkAttachmentLoadOp loadOp;
        public VkAttachmentStoreOp storeOp;
        public VkAttachmentLoadOp stencilLoadOp;
        public VkAttachmentStoreOp stencilStoreOp;
        public VkImageLayout initialLayout;
        public VkImageLayout finalLayout;
    }

    public class AttachmentReference {
        public uint attachment;
        public VkImageLayout layout;
    }

    public class SubpassDescription {
        public VkPipelineBindPoint pipelineBindPoint;
        public List<AttachmentReference> inputAttachments;
        public List<AttachmentReference> colorAttachments;
        public List<AttachmentReference> resolveAttachments;
        public List<uint> preserveAttachments;
        public AttachmentReference depthStencilAttachment;
    }

    public class SubpassDependency {
        public uint srcSubpass;
        public uint dstSubpass;
        public VkPipelineStageFlags srcStageMask;
        public VkPipelineStageFlags dstStageMask;
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public VkDependencyFlags dependencyFlags;
    }

    public class RenderPass : IDisposable, INative<VkRenderPass> {
        VkRenderPass renderPass;
        bool disposed = false;
        VkExtent2D granularity;

        public Device Device { get; private set; }

        public VkRenderPass Native {
            get {
                return renderPass;
            }
        }

        public VkExtent2D Granularity {
            get {
                return granularity;
            }
        }

        public List<AttachmentDescription> Attachments { get; private set; }
        public List<SubpassDescription> Subpasses { get; private set; }
        public List<SubpassDependency> Dependencies { get; private set; }

        public RenderPass(Device device, RenderPassCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateRenderPass(info);

            if (info.attachments != null) Attachments = new List<AttachmentDescription>(info.attachments);
            if (info.subpasses != null) Subpasses = new List<SubpassDescription>(info.subpasses);
            if (info.dependencies != null) Dependencies = new List<SubpassDependency>(info.dependencies);
        }

        void CreateRenderPass(RenderPassCreateInfo mInfo) {
            unsafe {
                var info = new VkRenderPassCreateInfo();
                info.sType = VkStructureType.RenderPassCreateInfo;

                //for CreateInfo
                int attachmentCount = 0;
                int subpassCount = 0;
                int dependencyCount = 0;

                //for CreateInfo.subpasses
                int totalInputAttachments = 0;
                int totalColorAttachments = 0;
                int totalResolveAttachments = 0;
                int totalDepthStencilAttachments = 0;
                int totalPreserveAttachments = 0;

                if (mInfo.attachments != null) attachmentCount = mInfo.attachments.Count;
                if (mInfo.subpasses != null) subpassCount = mInfo.subpasses.Count;
                if (mInfo.dependencies != null) dependencyCount = mInfo.dependencies.Count;

                for (int i = 0; i < subpassCount; i++) {
                    var subpass = mInfo.subpasses[i];
                    if (subpass.inputAttachments != null) totalInputAttachments += subpass.inputAttachments.Count;
                    if (subpass.colorAttachments != null) totalColorAttachments += subpass.colorAttachments.Count;
                    if (subpass.resolveAttachments != null) totalResolveAttachments += subpass.resolveAttachments.Count;
                    if (subpass.depthStencilAttachment != null) totalDepthStencilAttachments += 1;
                    if (subpass.preserveAttachments != null) totalPreserveAttachments += subpass.preserveAttachments.Count;
                }

                //for CreateInfo
                var attachments = stackalloc VkAttachmentDescription[attachmentCount];
                var subpasses = stackalloc VkSubpassDescription[subpassCount];
                var dependencies = stackalloc VkSubpassDependency[dependencyCount];

                //for CreateInfo.subpasses
                var inputAttachments = stackalloc VkAttachmentReference[totalInputAttachments];
                var colorAttachments = stackalloc VkAttachmentReference[totalColorAttachments];
                var resolveAttachments = stackalloc VkAttachmentReference[totalResolveAttachments];
                var depthAttachments = stackalloc VkAttachmentReference[totalDepthStencilAttachments];
                var preserveAttachments = stackalloc uint[totalPreserveAttachments];

                int inputIndex = 0;
                int colorIndex = 0;
                int resolveIndex = 0;
                int depthIndex = 0;
                int preserveIndex = 0;

                //marshal CreateInfo.attachments
                for (int i = 0; i < attachmentCount; i++) {
                    attachments[i].flags = mInfo.attachments[i].flags;
                    attachments[i].format = mInfo.attachments[i].format;
                    attachments[i].samples = mInfo.attachments[i].samples;
                    attachments[i].loadOp = mInfo.attachments[i].loadOp;
                    attachments[i].storeOp = mInfo.attachments[i].storeOp;
                    attachments[i].stencilLoadOp = mInfo.attachments[i].stencilLoadOp;
                    attachments[i].stencilStoreOp = mInfo.attachments[i].stencilStoreOp;
                    attachments[i].initialLayout = mInfo.attachments[i].initialLayout;
                    attachments[i].finalLayout = mInfo.attachments[i].finalLayout;
                }

                //marshal CreateInfo.subpasses
                for (int i = 0; i < subpassCount; i++) {
                    var subpass = mInfo.subpasses[i];
                    if (subpass.inputAttachments != null) {
                        for (int j = 0; j < subpass.inputAttachments.Count; j++) {
                            inputAttachments[j + inputIndex] = new VkAttachmentReference {
                                attachment = subpass.inputAttachments[j].attachment,
                                layout = subpass.inputAttachments[j].layout
                            };
                        }

                        subpasses[i].inputAttachmentCount = (uint)subpass.inputAttachments.Count;
                        subpasses[i].pInputAttachments = (IntPtr)(&inputAttachments[inputIndex]);
                        inputIndex += subpass.inputAttachments.Count;
                    }
                    if (subpass.colorAttachments != null) {
                        for (int j = 0; j < subpass.colorAttachments.Count; j++) {
                            colorAttachments[j + colorIndex] = new VkAttachmentReference {
                                attachment = subpass.colorAttachments[j].attachment,
                                layout = subpass.colorAttachments[j].layout
                            };
                        }

                        subpasses[i].colorAttachmentCount = (uint)subpass.colorAttachments.Count;
                        subpasses[i].pColorAttachments = (IntPtr)(&colorAttachments[colorIndex]);
                        colorIndex += subpass.colorAttachments.Count;
                    }
                    if (subpass.resolveAttachments != null) {
                        for (int j = 0; j < subpass.resolveAttachments.Count; j++) {
                            resolveAttachments[j + resolveIndex] = new VkAttachmentReference {
                                attachment = subpass.resolveAttachments[j].attachment,
                                layout = subpass.resolveAttachments[j].layout
                            };
                        }

                        subpasses[i].pResolveAttachments = (IntPtr)(&resolveAttachments[resolveIndex]);
                        resolveIndex += subpass.resolveAttachments.Count;
                    }
                    if (subpass.depthStencilAttachment != null) {
                        depthAttachments[depthIndex] = new VkAttachmentReference {
                            attachment = subpass.depthStencilAttachment.attachment,
                            layout = subpass.depthStencilAttachment.layout
                        };

                        subpasses[i].pDepthStencilAttachment = (IntPtr)(&depthAttachments[depthIndex]);
                        depthIndex += 1;
                    }
                    if (subpass.preserveAttachments != null) {
                        for (int j = 0; j < subpass.preserveAttachments.Count; j++) {
                            preserveAttachments[j + preserveIndex] = subpass.preserveAttachments[j];
                        }

                        subpasses[i].preserveAttachmentCount = (uint)subpass.preserveAttachments.Count;
                        subpasses[i].pPreserveAttachments = (IntPtr)(&preserveAttachments[preserveIndex]);
                        preserveIndex += subpass.preserveAttachments.Count;
                    }

                    subpasses[i].pipelineBindPoint = subpass.pipelineBindPoint;
                }

                //marshal CreateInfo.dependencies
                for (int i = 0; i < dependencyCount; i++) {
                    dependencies[i].srcSubpass = mInfo.dependencies[i].srcSubpass;
                    dependencies[i].dstSubpass = mInfo.dependencies[i].dstSubpass;
                    dependencies[i].srcStageMask = mInfo.dependencies[i].srcStageMask;
                    dependencies[i].dstStageMask = mInfo.dependencies[i].dstStageMask;
                    dependencies[i].srcAccessMask = mInfo.dependencies[i].srcAccessMask;
                    dependencies[i].dependencyFlags = mInfo.dependencies[i].dependencyFlags;
                }

                //marshal CreateInfo
                info.attachmentCount = (uint)attachmentCount;
                info.pAttachments = (IntPtr)attachments;
                info.subpassCount = (uint)subpassCount;
                info.pSubpasses = (IntPtr)subpasses;
                info.dependencyCount = (uint)dependencyCount;
                info.pDependencies = (IntPtr)dependencies;

                var result = Device.Commands.createRenderPass(Device.Native, ref info, Device.Instance.AllocationCallbacks, out renderPass);
                if (result != VkResult.Success) throw new RenderPassException(result, string.Format("Error creating render pass: {0}"));

                GetGranularity();
            }
        }

        void GetGranularity() {
            Device.Commands.getRenderAreaGranularity(Device.Native, renderPass, out granularity);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyRenderPass(Device.Native, renderPass, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~RenderPass() {
            Dispose(false);
        }
    }

    public class RenderPassException : VulkanException {
        public RenderPassException(VkResult result, string message) : base(result, message) { }
    }
}
