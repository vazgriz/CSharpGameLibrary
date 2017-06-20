using System;
using System.Reflection;
using System.Runtime.InteropServices;

using CSGL.Vulkan1.Unmanaged;

namespace CSGL.Vulkan1 {
    public partial class Device {
        public class DeviceCommands {
            public vkDeviceWaitIdleDelegate waitDeviceIdle;

            public vkGetDeviceQueueDelegate getDeviceQueue;
            public vkQueueSubmitDelegate queueSubmit;
            public vkQueuePresentKHRDelegate queuePresent;
            public vkQueueWaitIdleDelegate queueWaitIdle;
            public vkQueueBindSparseDelegate queueBindSparse;

            public vkCreateSwapchainKHRDelegate createSwapchain;
            public vkDestroySwapchainKHRDelegate destroySwapchain;
            public vkGetSwapchainImagesKHRDelegate getSwapchainImages;

            public vkCreateImageViewDelegate createImageView;
            public vkDestroyImageViewDelegate destroyImageView;

            public vkCreateImageDelegate createImage;
            public vkDestroyImageDelegate destroyImage;
            public vkGetImageMemoryRequirementsDelegate getImageMemoryRequirements;
            public vkBindImageMemoryDelegate bindImageMemory;
            public vkGetImageSparseMemoryRequirementsDelegate getImageSparseRequirements;
            public vkGetImageSubresourceLayoutDelegate getSubresourceLayout;

            public vkCreateShaderModuleDelegate createShaderModule;
            public vkDestroyShaderModuleDelegate destroyShaderModule;

            public vkCreateGraphicsPipelinesDelegate createGraphicsPiplines;
            public vkCreateComputePipelinesDelegate createComputePipelines;
            public vkDestroyPipelineDelegate destroyPipeline;

            public vkCreatePipelineLayoutDelegate createPipelineLayout;
            public vkDestroyPipelineLayoutDelegate destroyPipelineLayout;

            public vkCreatePipelineCacheDelegate createPipelineCache;
            public vkDestroyPipelineCacheDelegate destroyPipelineCache;
            public vkMergePipelineCachesDelegate mergePipelineCache;
            public vkGetPipelineCacheDataDelegate getPipelineCacheData;

            public vkCreateRenderPassDelegate createRenderPass;
            public vkDestroyRenderPassDelegate destroyRenderPass;
            public vkGetRenderAreaGranularityDelegate getRenderAreaGranularity;

            public vkCreateFramebufferDelegate createFramebuffer;
            public vkDestroyFramebufferDelegate destroyFramebuffer;

            public vkCreateSemaphoreDelegate createSemaphore;
            public vkDestroySemaphoreDelegate destroySemaphore;
            public vkAcquireNextImageKHRDelegate acquireNextImage;

            public vkCreateFenceDelegate createFence;
            public vkDestroyFenceDelegate destroyFence;
            public vkResetFencesDelegate resetFences;
            public vkWaitForFencesDelegate waitFences;
            public vkGetFenceStatusDelegate getFenceStatus;

            public vkCreateEventDelegate createEvent;
            public vkDestroyEventDelegate destroyEvent;
            public vkGetEventStatusDelegate getEventStatus;
            public vkSetEventDelegate setEvent;
            public vkResetEventDelegate resetEvent;

            public vkCreateCommandPoolDelegate createCommandPool;
            public vkDestroyCommandPoolDelegate destroyCommandPool;
            public vkResetCommandPoolDelegate resetCommandPool;
            public vkAllocateCommandBuffersDelegate allocateCommandBuffers;
            public vkFreeCommandBuffersDelegate freeCommandBuffers;
            public vkResetCommandBufferDelegate resetCommandBuffers;

            public vkBeginCommandBufferDelegate beginCommandBuffer;
            public vkEndCommandBufferDelegate endCommandBuffer;
            public vkCmdBeginRenderPassDelegate cmdBeginRenderPass;
            public vkCmdNextSubpassDelegate cmdNextSubpass;
            public vkCmdEndRenderPassDelegate cmdEndRenderPass;
            public vkCmdBindPipelineDelegate cmdBindPipeline;
            public vkCmdDrawDelegate cmdDraw;
            public vkCmdDrawIndexedDelegate cmdDrawIndexed;
            public vkCmdBindVertexBuffersDelegate cmdBindVertexBuffers;
            public vkCmdBindIndexBufferDelegate cmdBindIndexBuffer;
            public vkCmdBindDescriptorSetsDelegate cmdBindDescriptorSets;
            public vkCmdCopyBufferDelegate cmdCopyBuffer;
            public vkCmdPipelineBarrierDelegate cmdPipelineBarrier;
            public vkCmdCopyImageDelegate cmdCopyImage;
            public vkCmdClearColorImageDelegate cmdClearColorImage;
            public vkCmdExecuteCommandsDelegate cmdExecuteCommands;
            public vkCmdPushConstantsDelegate cmdPushConstants;
            public vkCmdSetEventDelegate cmdSetEvent;
            public vkCmdResetEventDelegate cmdResetEvent;
            public vkCmdWaitEventsDelegate cmdWaitEvents;
            public vkCmdSetViewportDelegate cmdSetViewports;
            public vkCmdSetScissorDelegate cmdSetScissor;
            public vkCmdSetLineWidthDelegate cmdSetLineWidth;
            public vkCmdSetDepthBiasDelegate cmdSetDepthBias;
            public vkCmdSetBlendConstantsDelegate cmdSetBlendConstants;
            public vkCmdSetDepthBoundsDelegate cmdSetDepthBounds;
            public vkCmdSetStencilCompareMaskDelegate cmdSetStencilCompareMask;
            public vkCmdSetStencilWriteMaskDelegate cmdSetStencilWriteMask;
            public vkCmdSetStencilReferenceDelegate cmdSetStencilReference;
            public vkCmdDrawIndirectDelegate cmdDrawIndirect;
            public vkCmdDrawIndexedIndirectDelegate cmdDrawIndexedIndirect;
            public vkCmdUpdateBufferDelegate cmdUpdateBuffer;
            public vkCmdFillBufferDelegate cmdFillBuffer;
            public vkCmdBlitImageDelegate cmdBlitImage;
            public vkCmdCopyBufferToImageDelegate cmdCopyBufferToImage;
            public vkCmdCopyImageToBufferDelegate cmdCopyImageToBuffer;
            public vkCmdClearAttachmentsDelegate cmdClearAttachments;
            public vkCmdResolveImageDelegate cmdResolveImage;
            public vkCmdResetQueryPoolDelegate cmdResetQueryPool;
            public vkCmdBeginQueryDelegate cmdBeginQuery;
            public vkCmdEndQueryDelegate cmdEndQuery;
            public vkCmdCopyQueryPoolResultsDelegate cmdCopyQueryPoolResults;
            public vkCmdWriteTimestampDelegate cmdWriteTimestamp;
            public vkCmdDispatchDelegate cmdDispatch;
            public vkCmdDispatchIndirectDelegate cmdDispatchIndirect;

            public vkCreateBufferDelegate createBuffer;
            public vkDestroyBufferDelegate destroyBuffer;
            public vkGetBufferMemoryRequirementsDelegate getMemoryRequirements;
            public vkGetPhysicalDeviceMemoryPropertiesDelegate getMemoryProperties;
            public vkBindBufferMemoryDelegate bindBuffer;

            public vkCreateBufferViewDelegate createBufferView;
            public vkDestroyBufferViewDelegate destroyBufferView;

            public vkAllocateMemoryDelegate allocateMemory;
            public vkFreeMemoryDelegate freeMemory;
            public vkMapMemoryDelegate mapMemory;
            public vkUnmapMemoryDelegate unmapMemory;
            public vkFlushMappedMemoryRangesDelegate flushMemory;
            public vkInvalidateMappedMemoryRangesDelegate invalidateMemory;
            public vkGetDeviceMemoryCommitmentDelegate getCommitedMemory;

            public vkCreateDescriptorSetLayoutDelegate createDescriptorSetLayout;
            public vkDestroyDescriptorSetLayoutDelegate destroyDescriptorSetLayout;

            public vkCreateDescriptorPoolDelegate createDescriptorPool;
            public vkDestroyDescriptorPoolDelegate destroyDescriptorPool;
            public vkResetDescriptorPoolDelegate resetDescriptorPool;
            public vkAllocateDescriptorSetsDelegate allocateDescriptorSets;
            public vkFreeDescriptorSetsDelegate freeDescriptorSets;
            public vkUpdateDescriptorSetsDelegate updateDescriptorSets;

            public vkCreateSamplerDelegate createSampler;
            public vkDestroySamplerDelegate destroySampler;

            public vkCreateQueryPoolDelegate createQueryPool;
            public vkDestroyQueryPoolDelegate destroyQueryPool;
            public vkGetQueryPoolResultsDelegate getQueryPoolResults;

            internal DeviceCommands(Device device) {
                Type t = typeof(DeviceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string command = Vulkan.GetCommand(fields[i].FieldType);
                    IntPtr ptr = device.GetProcAdddress(command);
                    fields[i].SetValue(this, Marshal.GetDelegateForFunctionPointer(ptr, fields[i].FieldType));
                }
            }
        }
    }

    public partial class Instance {
        public class InstanceCommands {
            public vkCreateDeviceDelegate createDevice;
            public vkDestroyDeviceDelegate destroyDevice;
            public vkGetDeviceProcAddrDelegate getDeviceProcAddr;

            public vkGetPhysicalDevicePropertiesDelegate getProperties;
            public vkGetPhysicalDeviceQueueFamilyPropertiesDelegate getQueueFamilyProperties;
            public vkGetPhysicalDeviceFeaturesDelegate getFeatures;
            public vkEnumerateDeviceExtensionPropertiesDelegate getExtensions;
            public vkGetPhysicalDeviceSurfaceSupportKHRDelegate getPresentationSupport;
            public vkGetPhysicalDeviceMemoryPropertiesDelegate getMemoryProperties;

            public vkEnumeratePhysicalDevicesDelegate enumeratePhysicalDevices;
            public vkGetPhysicalDevicePropertiesDelegate getPhysicalDeviceProperties;
            public vkGetPhysicalDeviceFormatPropertiesDelegate getPhysicalDeviceFormatProperties;
            public vkGetPhysicalDeviceImageFormatPropertiesDelegate getPhysicalDeviceImageFormatProperties;
            public vkGetPhysicalDeviceSparseImageFormatPropertiesDelegate getPhysicalDeviceSparseImageFormatProperties;

            public vkDestroySurfaceKHRDelegate destroySurface;
            public vkGetPhysicalDeviceSurfaceCapabilitiesKHRDelegate getCapabilities;
            public vkGetPhysicalDeviceSurfaceFormatsKHRDelegate getFormats;
            public vkGetPhysicalDeviceSurfacePresentModesKHRDelegate getModes;

            internal InstanceCommands(Instance instance) {
                Type t = typeof(InstanceCommands);
                FieldInfo[] fields = t.GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    string command = Vulkan.GetCommand(fields[i].FieldType);
                    IntPtr ptr = instance.GetProcAddress(command);
                    fields[i].SetValue(this, Marshal.GetDelegateForFunctionPointer(ptr, fields[i].FieldType));
                }
            }
        }
    }
}
