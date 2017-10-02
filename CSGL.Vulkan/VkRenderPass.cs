using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkRenderPassCreateInfo {
        public IList<VkAttachmentDescription> attachments;
        public IList<VkSubpassDescription> subpasses;
        public IList<VkSubpassDependency> dependencies;
    }

    public class VkAttachmentDescription {
        public VkAttachmentDescriptionFlags flags;
        public VkFormat format;
        public VkSampleCountFlags samples;
        public VkAttachmentLoadOp loadOp;
        public VkAttachmentStoreOp storeOp;
        public VkAttachmentLoadOp stencilLoadOp;
        public VkAttachmentStoreOp stencilStoreOp;
        public VkImageLayout initialLayout;
        public VkImageLayout finalLayout;

        public VkAttachmentDescription() { }

        internal VkAttachmentDescription(VkAttachmentDescription other) {
            flags = other.flags;
            format = other.format;
            samples = other.samples;
            loadOp = other.loadOp;
            storeOp = other.storeOp;
            stencilLoadOp = other.stencilLoadOp;
            stencilStoreOp = other.stencilStoreOp;
            initialLayout = other.initialLayout;
            finalLayout = other.finalLayout;
        }
    }

    public class VkAttachmentReference {
        public int attachment;
        public VkImageLayout layout;

        public VkAttachmentReference() { }

        internal VkAttachmentReference(VkAttachmentReference other) {
            attachment = other.attachment;
            layout = other.layout;
        }
    }

    public class VkSubpassDescription {
        public VkPipelineBindPoint pipelineBindPoint;
        public IList<VkAttachmentReference> inputAttachments;
        public IList<VkAttachmentReference> colorAttachments;
        public IList<VkAttachmentReference> resolveAttachments;
        public IList<int> preserveAttachments;
        public VkAttachmentReference depthStencilAttachment;

        public VkSubpassDescription() { }

        internal VkSubpassDescription(VkSubpassDescription other) {
            pipelineBindPoint = other.pipelineBindPoint;
            if (other.inputAttachments != null) {
                var inputAttachments = new List<VkAttachmentReference>(other.inputAttachments.Count);
                foreach (var input in other.inputAttachments) {
                    inputAttachments.Add(new VkAttachmentReference(input));
                }
                this.inputAttachments = inputAttachments.AsReadOnly();
            }
            if (other.colorAttachments != null) {
                var colorAttachments = new List<VkAttachmentReference>(other.colorAttachments.Count);
                foreach (var color in other.colorAttachments) {
                    colorAttachments.Add(new VkAttachmentReference(color));
                }
                this.colorAttachments = colorAttachments.AsReadOnly();
            }
            if (other.resolveAttachments != null) {
                var resolveAttachments = new List<VkAttachmentReference>(other.resolveAttachments);
                foreach (var resolve in other.resolveAttachments) {
                    resolveAttachments.Add(new VkAttachmentReference(resolve));
                }
                this.resolveAttachments = resolveAttachments.AsReadOnly();
            }
            preserveAttachments = other.preserveAttachments.CloneReadOnly();
            if (other.depthStencilAttachment != null) depthStencilAttachment = new VkAttachmentReference(other.depthStencilAttachment);
        }
    }

    public class VkSubpassDependency {
        public int srcSubpass;
        public int dstSubpass;
        public VkPipelineStageFlags srcStageMask;
        public VkPipelineStageFlags dstStageMask;
        public VkAccessFlags srcAccessMask;
        public VkAccessFlags dstAccessMask;
        public VkDependencyFlags dependencyFlags;

        public VkSubpassDependency() { }

        internal VkSubpassDependency(VkSubpassDependency other) {
            srcSubpass = other.srcSubpass;
            dstSubpass = other.dstSubpass;
            srcStageMask = other.srcStageMask;
            dstStageMask = other.dstStageMask;
            srcAccessMask = other.srcAccessMask;
            dstAccessMask = other.dstAccessMask;
            dependencyFlags = other.dependencyFlags;
        }
    }

    public class VkRenderPass : IDisposable, INative<Unmanaged.VkRenderPass> {
        Unmanaged.VkRenderPass renderPass;
        bool disposed = false;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkRenderPass Native {
            get {
                return renderPass;
            }
        }

        public VkExtent2D Granularity { get; private set; }

        public IList<VkAttachmentDescription> Attachments { get; private set; }
        public IList<VkSubpassDescription> Subpasses { get; private set; }
        public IList<VkSubpassDependency> Dependencies { get; private set; }

        public VkRenderPass(VkDevice device, VkRenderPassCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateRenderPass(info);

            if (info.attachments != null) {
                var attachments = new List<VkAttachmentDescription>(info.attachments.Count);
                foreach (var attachment in info.attachments) {
                    attachments.Add(new VkAttachmentDescription(attachment));
                }
                Attachments = attachments.AsReadOnly();
            }
            if (info.subpasses != null) {
                var subpasses = new List<VkSubpassDescription>(info.subpasses.Count);
                foreach (var subpass in info.subpasses) {
                    subpasses.Add(new VkSubpassDescription(subpass));
                }

                Subpasses = subpasses.AsReadOnly();
            }
            if (info.dependencies != null) {
                var dependencies = new List<VkSubpassDependency>(info.dependencies.Count);
                foreach (var dependency in  info.dependencies) {
                    dependencies.Add(new VkSubpassDependency(dependency));
                }

                Dependencies = dependencies.AsReadOnly();
            }
        }

        void CreateRenderPass(VkRenderPassCreateInfo mInfo) {
            if (mInfo.subpasses == null) throw new ArgumentNullException(nameof(mInfo.subpasses));

            unsafe {
                var info = new Unmanaged.VkRenderPassCreateInfo();
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
                var attachments = stackalloc Unmanaged.VkAttachmentDescription[attachmentCount];
                var subpasses = stackalloc Unmanaged.VkSubpassDescription[subpassCount];
                var dependencies = stackalloc Unmanaged.VkSubpassDependency[dependencyCount];

                //for CreateInfo.subpasses
                var inputAttachments = stackalloc Unmanaged.VkAttachmentReference[totalInputAttachments];
                var colorAttachments = stackalloc Unmanaged.VkAttachmentReference[totalColorAttachments];
                var resolveAttachments = stackalloc Unmanaged.VkAttachmentReference[totalResolveAttachments];
                var depthAttachments = stackalloc Unmanaged.VkAttachmentReference[totalDepthStencilAttachments];
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
                            inputAttachments[j + inputIndex] = new Unmanaged.VkAttachmentReference {
                                attachment = (uint)subpass.inputAttachments[j].attachment,
                                layout = subpass.inputAttachments[j].layout
                            };
                        }

                        subpasses[i].inputAttachmentCount = (uint)subpass.inputAttachments.Count;
                        subpasses[i].pInputAttachments = (IntPtr)(&inputAttachments[inputIndex]);
                        inputIndex += subpass.inputAttachments.Count;
                    }
                    if (subpass.colorAttachments != null) {
                        for (int j = 0; j < subpass.colorAttachments.Count; j++) {
                            colorAttachments[j + colorIndex] = new Unmanaged.VkAttachmentReference {
                                attachment = (uint)subpass.colorAttachments[j].attachment,
                                layout = subpass.colorAttachments[j].layout
                            };
                        }

                        subpasses[i].colorAttachmentCount = (uint)subpass.colorAttachments.Count;
                        subpasses[i].pColorAttachments = (IntPtr)(&colorAttachments[colorIndex]);
                        colorIndex += subpass.colorAttachments.Count;
                    }
                    if (subpass.resolveAttachments != null) {
                        for (int j = 0; j < subpass.resolveAttachments.Count; j++) {
                            resolveAttachments[j + resolveIndex] = new Unmanaged.VkAttachmentReference {
                                attachment = (uint)subpass.resolveAttachments[j].attachment,
                                layout = subpass.resolveAttachments[j].layout
                            };
                        }

                        subpasses[i].pResolveAttachments = (IntPtr)(&resolveAttachments[resolveIndex]);
                        resolveIndex += subpass.resolveAttachments.Count;
                    }
                    if (subpass.depthStencilAttachment != null) {
                        depthAttachments[depthIndex] = new Unmanaged.VkAttachmentReference {
                            attachment = (uint)subpass.depthStencilAttachment.attachment,
                            layout = subpass.depthStencilAttachment.layout
                        };

                        subpasses[i].pDepthStencilAttachment = (IntPtr)(&depthAttachments[depthIndex]);
                        depthIndex += 1;
                    }
                    if (subpass.preserveAttachments != null) {
                        for (int j = 0; j < subpass.preserveAttachments.Count; j++) {
                            preserveAttachments[j + preserveIndex] = (uint)subpass.preserveAttachments[j];
                        }

                        subpasses[i].preserveAttachmentCount = (uint)subpass.preserveAttachments.Count;
                        subpasses[i].pPreserveAttachments = (IntPtr)(&preserveAttachments[preserveIndex]);
                        preserveIndex += subpass.preserveAttachments.Count;
                    }

                    subpasses[i].pipelineBindPoint = subpass.pipelineBindPoint;
                }

                //marshal CreateInfo.dependencies
                for (int i = 0; i < dependencyCount; i++) {
                    dependencies[i].srcSubpass = (uint)mInfo.dependencies[i].srcSubpass;
                    dependencies[i].dstSubpass = (uint)mInfo.dependencies[i].dstSubpass;
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
            Unmanaged.VkExtent2D granularityNative;
            Device.Commands.getRenderAreaGranularity(Device.Native, renderPass, out granularityNative);
            Granularity = new VkExtent2D(granularityNative);
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

        ~VkRenderPass() {
            Dispose(false);
        }
    }

    public class RenderPassException : VulkanException {
        public RenderPassException(VkResult result, string message) : base(result, message) { }
    }
}
