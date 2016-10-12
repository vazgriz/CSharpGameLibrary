using System;
using System.Collections.Generic;
using System.IO;

using CSGL.GLFW;
using CSGL.Vulkan;
using CSGL.Vulkan.Managed;

namespace VK_Test {
    class Program : IDisposable {
        static void Main(string[] args) {
            using (var p = new Program()) {
                p.Run();
            }
            Console.ReadLine();
        }

        string[] layers = {
            "VK_LAYER_LUNARG_standard_validation",
            //"VK_LAYER_LUNARG_api_dump"
        };

        string[] deviceExtensions = {
            "VK_KHR_swapchain"
        };

        int width = 800;
        int height = 600;
        WindowPtr window;

        uint graphicsIndex;
        uint presentIndex;
        Queue graphicsQueue;
        Queue presentQueue;

        VkFormat swapchainImageFormat;
        VkExtent2D swapchainExtent;

        Instance instance;
        Surface surface;
        PhysicalDevice physicalDevice;
        Device device;
        Swapchain swapchain;
        List<CSGL.Vulkan.Managed.Image> swapchainImages;
        List<ImageView> swapchainImageViews;
        RenderPass renderPass;
        PipelineLayout pipelineLayout;
        Pipeline pipeline;
        List<Framebuffer> swapchainFramebuffers;
        CommandPool commandPool;
        List<CommandBuffer> commandBuffers;
        Semaphore imageAvailableSemaphore;
        Semaphore renderFinishedSemaphore;

        bool recreateSwapchainFlag;

        void Run() {
            GLFW.Init();
            Vulkan.Init();
            CreateWindow();
            CreateInstance();
            PickPhysicalDevice();
            CreateSurface();
            PickQueues();
            CreateDevice();
            CreateSwapchain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateFramebuffers();
            CreateCommandPool();
            CreateCommandBuffers();
            CreateSemaphores();

            MainLoop();
        }

        public void Dispose() {
            imageAvailableSemaphore.Dispose();
            renderFinishedSemaphore.Dispose();
            commandPool.Dispose();
            foreach (var fb in swapchainFramebuffers) fb.Dispose();
            pipeline.Dispose();
            pipelineLayout.Dispose();
            renderPass.Dispose();
            foreach (var iv in swapchainImageViews) iv.Dispose();
            swapchain.Dispose();
            device.Dispose();
            surface.Dispose();
            instance.Dispose();
            GLFW.DestroyWindow(window);
            GLFW.Terminate();
        }

        void MainLoop() {
            var waitSemaphores = new Semaphore[] { imageAvailableSemaphore };
            var waitStages = new VkPipelineStageFlags[] { VkPipelineStageFlags.PipelineStageColorAttachmentOutputBit };
            var signalSemaphores = new Semaphore[] { renderFinishedSemaphore };
            var swapchains = new Swapchain[] { swapchain };

            var commandBuffer = new CommandBuffer[1];
            var index = new uint[1];

            var submitInfo = new SubmitInfo();
            submitInfo.waitSemaphores = waitSemaphores;
            submitInfo.waitDstStageMask = waitStages;
            submitInfo.commandBuffers = commandBuffer;
            submitInfo.signalSemaphores = signalSemaphores;

            var presentInfo = new PresentInfo();
            presentInfo.waitSemaphores = signalSemaphores;
            presentInfo.swapchains = swapchains;
            presentInfo.imageIndices = index;

            var submitInfos = new SubmitInfo[] { submitInfo };

            while (true) {
                GLFW.PollEvents();
                if (GLFW.GetKey(window, CSGL.Input.KeyCode.Enter) == CSGL.Input.KeyAction.Press) {
                    break;
                }
                if (GLFW.WindowShouldClose(window)) break;

                if (recreateSwapchainFlag) {
                    recreateSwapchainFlag = false;
                    RecreateSwapchain();
                }

                uint imageIndex;
                var result = swapchain.AcquireNextImage(ulong.MaxValue, imageAvailableSemaphore, out imageIndex);

                if (result == VkResult.ErrorOutOfDateKhr || result == VkResult.SuboptimalKhr) {
                    RecreateSwapchain();
                    continue;
                }

                commandBuffer[0] = commandBuffers[(int)imageIndex];
                swapchains[0] = swapchain;
                index[0] = imageIndex;

                graphicsQueue.Submit(submitInfos, null);
                result = presentQueue.Present(presentInfo);

                if (result == VkResult.ErrorOutOfDateKhr || result == VkResult.SuboptimalKhr) {
                    RecreateSwapchain();
                }
            }

            device.WaitIdle();
        }

        void RecreateSwapchain() {
            device.WaitIdle();
            CreateSwapchain();
            CreateImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateFramebuffers();
            CreateCommandBuffers();
        }

        void OnWindowResized(WindowPtr window, int width, int height) {
            if (width == 0 || height == 0) return;
            recreateSwapchainFlag = true;
        }

        void CreateWindow() {
            GLFW.WindowHint(WindowHint.ClientAPI, (int)ContextAPI.NoAPI);
            window = GLFW.CreateWindow(width, height, "Vulkan Test", MonitorPtr.Null, WindowPtr.Null);
        }

        void CreateInstance() {
            var extensions = GLFW.GetRequiredInstanceExceptions();

            var appInfo = new ApplicationInfo(
                new VkVersion(1, 0, 0),
                new VkVersion(1, 0, 0),
                new VkVersion(1, 0, 0),
                "Vulkan Test",
                null
            );

            var info = new InstanceCreateInfo(appInfo, extensions, layers);
            instance = new Instance(info);
        }

        void PickPhysicalDevice() {
            physicalDevice = instance.PhysicalDevices[0];
        }

        void CreateSurface() {
            surface = new Surface(physicalDevice, window);
        }

        void PickQueues() {
            int g = -1;
            int p = -1;

            for (int i = 0; i < physicalDevice.QueueFamilies.Count; i++) {
                var family = physicalDevice.QueueFamilies[i];
                if ((family.Flags & VkQueueFlags.QueueGraphicsBit) != 0) {
                    g = i;
                }

                if (family.SurfaceSupported(surface)) {
                    p = i;
                }
            }

            graphicsIndex = (uint)g;
            presentIndex = (uint)p;
        }

        void CreateDevice() {
            var features = physicalDevice.Features;

            HashSet<uint> uniqueIndices = new HashSet<uint> { graphicsIndex, presentIndex };
            float[] priorities = new float[] { 1f };
            DeviceQueueCreateInfo[] queueInfos = new DeviceQueueCreateInfo[uniqueIndices.Count];

            int i = 0;
            foreach (var ind in uniqueIndices) {
                var queueInfo = new DeviceQueueCreateInfo(ind, 1, priorities);
                queueInfos[i] = queueInfo;
                i++;
            }

            var info = new DeviceCreateInfo(deviceExtensions, queueInfos, features);
            device = new Device(physicalDevice, info);

            graphicsQueue = device.GetQueue(graphicsIndex, 0);
            presentQueue = device.GetQueue(presentIndex, 0);
        }

        SwapchainSupport GetSwapchainSupport(PhysicalDevice physicalDevice) {
            var cap = surface.Capabilities;
            var formats = surface.Formats;
            var modes = surface.PresentModes;

            return new SwapchainSupport(cap, formats, modes);
        }

        VkSurfaceFormatKHR ChooseSwapSurfaceFormat(List<VkSurfaceFormatKHR> formats) {
            if (formats.Count == 1 && formats[0].format == VkFormat.FormatUndefined) {
                var result = new VkSurfaceFormatKHR();
                result.format = VkFormat.FormatB8g8r8a8Unorm;
                result.colorSpace = VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr;
                return result;
            }

            foreach (var f in formats) {
                if (f.format == VkFormat.FormatB8g8r8a8Unorm && f.colorSpace == VkColorSpaceKHR.ColorSpaceSrgbNonlinearKhr) {
                    return f;
                }
            }

            return formats[0];
        }

        VkPresentModeKHR ChooseSwapPresentMode(List<VkPresentModeKHR> modes) {
            foreach (var m in modes) {
                if (m == VkPresentModeKHR.PresentModeMailboxKhr) {
                    return m;
                }
            }

            return VkPresentModeKHR.PresentModeFifoKhr;
        }

        VkExtent2D ChooseSwapExtent(ref VkSurfaceCapabilitiesKHR cap) {
            if (cap.currentExtent.width != uint.MaxValue) {
                return cap.currentExtent;
            } else {
                var extent = new VkExtent2D();
                extent.width = (uint)width;
                extent.height = (uint)height;

                extent.width = Math.Max(cap.minImageExtent.width, Math.Min(cap.maxImageExtent.width, extent.width));
                extent.height = Math.Max(cap.minImageExtent.height, Math.Min(cap.maxImageExtent.height, extent.height));

                return extent;
            }
        }

        void CreateSwapchain() {
            var support = GetSwapchainSupport(physicalDevice);
            var cap = support.cap;

            var surfaceFormat = ChooseSwapSurfaceFormat(support.formats);
            var mode = ChooseSwapPresentMode(support.modes);
            var extent = ChooseSwapExtent(ref cap);

            uint imageCount = cap.minImageCount + 1;
            if (cap.maxImageCount > 0 && imageCount > cap.maxImageCount) {
                imageCount = cap.maxImageCount;
            }

            var oldSwapchain = swapchain;
            var info = new SwapchainCreateInfo(surface, oldSwapchain);
            info.minImageCount = imageCount;
            info.imageFormat = surfaceFormat.format;
            info.imageColorSpace = surfaceFormat.colorSpace;
            info.imageExtent = extent;
            info.imageArrayLayers = 1;
            info.imageUsage = VkImageUsageFlags.ImageUsageColorAttachmentBit;

            var queueFamilyIndices = new uint[] { graphicsIndex, presentIndex };

            if (graphicsIndex != presentIndex) {
                info.imageSharingMode = VkSharingMode.SharingModeConcurrent;
                info.queueFamilyIndices = queueFamilyIndices;
            } else {
                info.imageSharingMode = VkSharingMode.SharingModeExclusive;
            }

            info.preTransform = cap.currentTransform;
            info.compositeAlpha = VkCompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr;
            info.presentMode = mode;
            info.clipped = true;

            swapchain = new Swapchain(device, info);
            oldSwapchain?.Dispose();

            swapchainImages = swapchain.Images;

            swapchainImageFormat = surfaceFormat.format;
            swapchainExtent = extent;
        }

        void CreateImageViews() {
            if (swapchainImageViews != null) {
                foreach (var iv in swapchainImageViews) iv.Dispose();
            }

            swapchainImageViews = new List<ImageView>();
            foreach (var image in swapchainImages) {
                var info = new ImageViewCreateInfo(image);
                info.viewType = VkImageViewType.ImageViewType2d;
                info.format = swapchainImageFormat;
                info.components.r = VkComponentSwizzle.ComponentSwizzleIdentity;
                info.components.g = VkComponentSwizzle.ComponentSwizzleIdentity;
                info.components.b = VkComponentSwizzle.ComponentSwizzleIdentity;
                info.components.a = VkComponentSwizzle.ComponentSwizzleIdentity;
                info.subresourceRange.aspectMask = VkImageAspectFlags.ImageAspectColorBit;
                info.subresourceRange.baseMipLevel = 0;
                info.subresourceRange.levelCount = 1;
                info.subresourceRange.baseArrayLayer = 0;
                info.subresourceRange.layerCount = 1;

                swapchainImageViews.Add(new ImageView(device, info));
            }
        }

        void CreateRenderPass() {
            var colorAttachment = new VkAttachmentDescription();
            colorAttachment.format = swapchainImageFormat;
            colorAttachment.samples = VkSampleCountFlags.SampleCount1Bit;
            colorAttachment.loadOp = VkAttachmentLoadOp.AttachmentLoadOpClear;
            colorAttachment.storeOp = VkAttachmentStoreOp.AttachmentStoreOpStore;
            colorAttachment.stencilLoadOp = VkAttachmentLoadOp.AttachmentLoadOpDontCare;
            colorAttachment.stencilStoreOp = VkAttachmentStoreOp.AttachmentStoreOpDontCare;
            colorAttachment.initialLayout = VkImageLayout.ImageLayoutUndefined;
            colorAttachment.finalLayout = VkImageLayout.ImageLayoutPresentSrcKhr;

            var colorAttachmentRef = new VkAttachmentReference();
            colorAttachmentRef.attachment = 0;
            colorAttachmentRef.layout = VkImageLayout.ImageLayoutColorAttachmentOptimal;

            var subpass = new SubpassDescription();
            subpass.PipelineBindPoint = VkPipelineBindPoint.PipelineBindPointGraphics;
            subpass.ColorAttachments = new VkAttachmentReference[] { colorAttachmentRef };

            var dependency = new VkSubpassDependency();
            dependency.srcSubpass = uint.MaxValue;  //VK_SUBPASS_EXTERNAL
            dependency.dstSubpass = 0;
            dependency.srcStageMask = VkPipelineStageFlags.PipelineStageBottomOfPipeBit;
            dependency.srcAccessMask = VkAccessFlags.AccessMemoryReadBit;
            dependency.dstStageMask = VkPipelineStageFlags.PipelineStageColorAttachmentOutputBit;
            dependency.dstAccessMask = VkAccessFlags.AccessColorAttachmentReadBit
                                    | VkAccessFlags.AccessColorAttachmentWriteBit;

            var info = new RenderPassCreateInfo();
            info.Attachments = new VkAttachmentDescription[] { colorAttachment };
            info.Subpasses = new SubpassDescription[] { subpass };
            info.Dependencies = new VkSubpassDependency[] { dependency };

            renderPass?.Dispose();
            renderPass = new RenderPass(device, info);
        }

        public ShaderModule CreateShaderModule(byte[] code) {
            var info = new ShaderModuleCreateInfo(code);
            return new ShaderModule(device, info);
        }

        void CreateGraphicsPipeline() {
            var vert = CreateShaderModule(File.ReadAllBytes("vert.spv"));
            var frag = CreateShaderModule(File.ReadAllBytes("frag.spv"));

            var vertInfo = new PipelineShaderStageCreateInfo();
            vertInfo.stage = VkShaderStageFlags.ShaderStageVertexBit;
            vertInfo.module = vert;
            vertInfo.name = "main";

            var fragInfo = new PipelineShaderStageCreateInfo();
            fragInfo.stage = VkShaderStageFlags.ShaderStageFragmentBit;
            fragInfo.module = frag;
            fragInfo.name = "main";

            var shaderStages = new PipelineShaderStageCreateInfo[] { vertInfo, fragInfo };

            var vertexInputInfo = new PipelineVertexInputStateCreateInfo();

            var inputAssembly = new PipelineInputAssemblyStateCreateInfo();
            inputAssembly.topology = VkPrimitiveTopology.PrimitiveTopologyTriangleList;

            var viewport = new VkViewport();
            viewport.width = swapchainExtent.width;
            viewport.height = swapchainExtent.height;
            viewport.minDepth = 0f;
            viewport.maxDepth = 1f;

            var scissor = new VkRect2D();
            scissor.extent = swapchainExtent;

            var viewportState = new PipelineViewportStateCreateInfo();
            viewportState.viewports = new VkViewport[] { viewport };
            viewportState.scissors = new VkRect2D[] { scissor };

            var rasterizer = new PipelineRasterizationStateCreateInfo();
            rasterizer.polygonMode = VkPolygonMode.PolygonModeFill;
            rasterizer.lineWidth = 1f;
            rasterizer.cullMode = VkCullModeFlags.CullModeBackBit;
            rasterizer.frontFace = VkFrontFace.FrontFaceClockwise;

            var multisampling = new PipelineMultisampleStateCreateInfo();
            multisampling.rasterizationSamples = VkSampleCountFlags.SampleCount1Bit;
            multisampling.minSampleShading = 1f;

            var colorBlendAttachment = new PipelineColorBlendAttachmentState();
            colorBlendAttachment.colorWriteMask = VkColorComponentFlags.ColorComponentRBit
                                                | VkColorComponentFlags.ColorComponentGBit
                                                | VkColorComponentFlags.ColorComponentBBit
                                                | VkColorComponentFlags.ColorComponentABit;
            colorBlendAttachment.srcColorBlendFactor = VkBlendFactor.BlendFactorOne;
            colorBlendAttachment.dstColorBlendFactor = VkBlendFactor.BlendFactorZero;
            colorBlendAttachment.colorBlendOp = VkBlendOp.BlendOpAdd;
            colorBlendAttachment.srcAlphaBlendFactor = VkBlendFactor.BlendFactorOne;
            colorBlendAttachment.dstAlphaBlendFactor = VkBlendFactor.BlendFactorZero;
            colorBlendAttachment.alphaBlendOp = VkBlendOp.BlendOpAdd;

            var colorBlending = new PipelineColorBlendStateCreateInfo();
            colorBlending.logicOp = VkLogicOp.LogicOpCopy;
            colorBlending.attachments = new PipelineColorBlendAttachmentState[] { colorBlendAttachment };

            var pipelineLayoutInfo = new PipelineLayoutCreateInfo();

            pipelineLayout?.Dispose();

            pipelineLayout = new PipelineLayout(device, pipelineLayoutInfo);

            var info = new GraphicsPipelineCreateInfo();
            info.stages = shaderStages;
            info.vertexInputState = vertexInputInfo;
            info.inputAssemblyState = inputAssembly;
            info.viewportState = viewportState;
            info.rasterizationState = rasterizer;
            info.multisampleState = multisampling;
            info.colorBlendState = colorBlending;
            info.layout = pipelineLayout;
            info.renderPass = renderPass;
            info.subpass = 0;
            info.basePipeline = null;
            info.basePipelineIndex = -1;

            pipeline?.Dispose();

            pipeline = new Pipeline(device, info, null);

            vert.Dispose();
            frag.Dispose();
        }

        void CreateFramebuffers() {
            if (swapchainFramebuffers != null) {
                foreach (var fb in swapchainFramebuffers) fb.Dispose();
            }

            swapchainFramebuffers = new List<Framebuffer>(swapchainImageViews.Count);

            for (int i = 0; i < swapchainImageViews.Count; i++) {
                var attachments = new ImageView[] { swapchainImageViews[i] };

                var info = new FramebufferCreateInfo();
                info.renderPass = renderPass;
                info.attachments = attachments;
                info.width = swapchainExtent.width;
                info.height = swapchainExtent.height;
                info.layers = 1;

                swapchainFramebuffers.Add(new Framebuffer(device, info));
            }
        }

        void CreateCommandPool() {
            var info = new CommandPoolCreateInfo();
            info.QueueFamilyIndex = graphicsIndex;

            commandPool = new CommandPool(device, info);
        }

        void CreateCommandBuffers() {
            if (commandBuffers != null) {
                commandPool.Free(commandBuffers);
            }

            var info = new CommandBufferAllocateInfo();
            info.commandPool = commandPool;
            info.level = VkCommandBufferLevel.CommandBufferLevelPrimary;
            info.count = (uint)swapchainFramebuffers.Count;

            commandBuffers = new List<CommandBuffer>(commandPool.Allocate(info));

            for (int i = 0; i < commandBuffers.Count; i++) {
                var buffer = commandBuffers[i];
                var beginInfo = new CommandBufferBeginInfo();
                beginInfo.flags = VkCommandBufferUsageFlags.CommandBufferUsageSimultaneousUseBit;

                buffer.Begin(beginInfo);

                var renderPassInfo = new RenderPassBeginInfo();
                renderPassInfo.renderPass = renderPass;
                renderPassInfo.framebuffer = swapchainFramebuffers[i];
                renderPassInfo.renderArea.extent = swapchainExtent;

                VkClearValue clearColor = new VkClearValue();
                clearColor.color.float32 = 0;
                clearColor.color.float32_1 = 0;
                clearColor.color.float32_2 = 0;
                clearColor.color.float32_3 = 1f;

                renderPassInfo.clearValues = new VkClearValue[] { clearColor };

                buffer.BeginRenderPass(renderPassInfo, VkSubpassContents.SubpassContentsInline);
                buffer.BindPipeline(VkPipelineBindPoint.PipelineBindPointGraphics, pipeline);
                buffer.Draw(3, 1, 0, 0);
                buffer.EndRenderPass();
                buffer.End();
            }
        }

        void CreateSemaphores() {
            imageAvailableSemaphore = new Semaphore(device);
            renderFinishedSemaphore = new Semaphore(device);
        }
    }

    struct SwapchainSupport {
        public VkSurfaceCapabilitiesKHR cap;
        public List<VkSurfaceFormatKHR> formats;
        public List<VkPresentModeKHR> modes;

        public SwapchainSupport(VkSurfaceCapabilitiesKHR cap, List<VkSurfaceFormatKHR> formats, List<VkPresentModeKHR> modes) {
            this.cap = cap;
            this.formats = formats;
            this.modes = modes;
        }
    }
}
