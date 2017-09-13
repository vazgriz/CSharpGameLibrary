using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceQueueCreateInfo {
        public uint queueFamilyIndex;
        public uint queueCount;
        public IList<float> priorities;
    }

    public class SubmitInfo {
        public IList<Semaphore> waitSemaphores;
        public IList<VkPipelineStageFlags> waitDstStageMask;
        public IList<CommandBuffer> commandBuffers;
        public IList<Semaphore> signalSemaphores;
    }

    public class PresentInfo {
        public IList<Semaphore> waitSemaphores;
        public IList<Swapchain> swapchains;
        public IList<uint> imageIndices;
        public IList<VkResult> results;
    }

    public class SparseMemoryBind {
        public ulong resourceOffset;
        public ulong size;
        public DeviceMemory memory;
        public ulong memoryOffset;
        public VkSparseMemoryBindFlags flags;
    }

    public partial class SparseImageMemoryBind {
        public VkImageSubresource subresource;
        public VkOffset3D offset;
        public VkExtent3D extent;
        public DeviceMemory memory;
        public ulong memoryOffset;
        public VkSparseMemoryBindFlags flags;
    }

    public partial class SparseBufferMemoryBindInfo {
        public Buffer buffer;
        public IList<SparseMemoryBind> binds;
    }

    public partial class SparseImageOpaqueMemoryBindInfo {
        public Image image;
        public IList<SparseMemoryBind> binds;
    }

    public partial class SparseImageMemoryBindInfo {
        public Image image;
        public IList<SparseImageMemoryBind> binds;
    }

    public class BindSparseInfo {
        public IList<Semaphore> waitSemaphores;
        public IList<SparseBufferMemoryBindInfo> bufferBinds;
        public IList<SparseImageOpaqueMemoryBindInfo> imageOpaqueBinds;
        public IList<SparseImageMemoryBindInfo> imageBinds;
        public IList<Semaphore> signalSemaphores;
    }

    public class Queue {
        VkQueue queue;

        Device device;

        public uint FamilyIndex { get; private set; }
        public QueueFamily Family { get; private set; }
        public float Priority { get; private set; }

        internal Queue(Device device, VkQueue queue, uint familyIndex, float priority) {
            this.device = device;
            this.queue = queue;
            FamilyIndex = familyIndex;
            Family = device.PhysicalDevice.QueueFamilies[(int)familyIndex];
            Priority = priority;
        }

        public Device Device {
            get {
                return device;
            }
        }

        public void WaitIdle() {
            Device.Commands.queueWaitIdle(queue);
        }

        public void Submit(IList<SubmitInfo> infos, Fence fence = null) {
            VkFence fenceNative = VkFence.Null;
            if (fence != null) {
                fenceNative = fence.Native;
            }

            if (infos == null || infos.Count == 0) {
                Device.Commands.queueSubmit(queue, 0, IntPtr.Zero, fenceNative);
            }

            unsafe {
                int totalWaitSemaphores = 0;
                int totalCommandBuffers = 0;
                int totalSignalSemaphores = 0;

                for (int i = 0; i < infos.Count; i++) {    //get the total length needed for each array
                    var info = infos[i];
                    if (info.waitSemaphores != null) totalWaitSemaphores += info.waitSemaphores.Count;
                    if (info.commandBuffers != null) totalCommandBuffers += info.commandBuffers.Count;
                    if (info.signalSemaphores != null) totalSignalSemaphores += info.signalSemaphores.Count;
                }

                var waitSemaphoresNative = stackalloc VkSemaphore[totalWaitSemaphores];
                var waitDstNative = stackalloc int[totalWaitSemaphores];    //required to be the same length as above
                var commandBuffersNative = stackalloc VkCommandBuffer[totalCommandBuffers];
                var signalSemaphoresNative = stackalloc VkSemaphore[totalSignalSemaphores];

                int waitSemaphoresIndex = 0;
                int commandBuffersIndex = 0;
                int signalSemaphoresIndex = 0;

                var infosNative = stackalloc VkSubmitInfo[infos.Count];

                for (int i = 0; i < infos.Count; i++) {
                    var info = new VkSubmitInfo();
                    info.sType = VkStructureType.SubmitInfo;

                    if (infos[i].waitSemaphores != null) {
                        int waitCount = infos[i].waitSemaphores.Count;

                        Interop.Marshal<VkSemaphore, Semaphore>(infos[i].waitSemaphores, &waitSemaphoresNative[waitSemaphoresIndex]);

                        for (int j = 0; j < waitCount; j++) {
                            waitDstNative[waitSemaphoresIndex + j] = (int)infos[i].waitDstStageMask[j];
                        }

                        info.waitSemaphoreCount = (uint)waitCount;
                        info.pWaitSemaphores = (IntPtr)(&waitSemaphoresNative[waitSemaphoresIndex]);    //get address from index
                        info.pWaitDstStageMask = (IntPtr)(&waitDstNative[waitSemaphoresIndex]);
                        waitSemaphoresIndex += waitCount;  //increment index
                    }

                    if (infos[i].commandBuffers != null) {
                        int commandCount = infos[i].commandBuffers.Count;
                        Interop.Marshal<VkCommandBuffer, CommandBuffer>(infos[i].commandBuffers, &commandBuffersNative[commandBuffersIndex]);

                        info.commandBufferCount = (uint)infos[i].commandBuffers.Count;
                        info.pCommandBuffers = (IntPtr)(&commandBuffersNative[commandBuffersIndex]);    //get address from index
                        commandBuffersIndex += infos[i].commandBuffers.Count;  //increment index
                    }

                    if (infos[i].signalSemaphores != null) {
                        int signalCount = infos[i].signalSemaphores.Count;
                        Interop.Marshal<VkSemaphore, Semaphore>(infos[i].signalSemaphores, &signalSemaphoresNative[signalSemaphoresIndex]);

                        info.signalSemaphoreCount = (uint)infos[i].signalSemaphores.Count;
                        info.pSignalSemaphores = (IntPtr)(&signalSemaphoresNative[signalSemaphoresIndex]);  //get address from index
                        signalSemaphoresIndex += infos[i].signalSemaphores.Count;  //increment index
                    }

                    infosNative[i] = info;
                }

                var result = Device.Commands.queueSubmit(queue, (uint)infos.Count, (IntPtr)infosNative, fenceNative);
                if (result != VkResult.Success) throw new QueueException(result, string.Format("Error submitting command to queue: {0}", result));
            }
        }

        public VkResult Present(PresentInfo info) {
            unsafe {
                var waitSemaphoresNative = stackalloc VkSemaphore[info.waitSemaphores.Count];
                Interop.Marshal<VkSemaphore, Semaphore>(info.waitSemaphores, waitSemaphoresNative);

                //swapchains, indices, and results must have the same length
                int swapchainCount = info.swapchains.Count;

                var swapchainsNative = stackalloc VkSwapchainKHR[swapchainCount];
                Interop.Marshal<VkSwapchainKHR, Swapchain>(info.swapchains, swapchainsNative);

                uint* imageIndices = stackalloc uint[swapchainCount];
                Interop.Copy(info.imageIndices, (IntPtr)imageIndices);

                int resultsLength = 0;
                if (info.results != null) { //user may not request results
                    resultsLength = swapchainCount;
                }
                var results = stackalloc int[resultsLength];

                var infoNative = new VkPresentInfoKHR();
                infoNative.sType = VkStructureType.PresentInfoKhr;
                infoNative.waitSemaphoreCount = (uint)info.waitSemaphores.Count;
                infoNative.pWaitSemaphores = (IntPtr)waitSemaphoresNative;
                infoNative.swapchainCount = (uint)swapchainCount;
                infoNative.pSwapchains = (IntPtr)swapchainsNative;
                infoNative.pImageIndices = (IntPtr)imageIndices;

                var result = Device.Commands.queuePresent(queue, ref infoNative);

                for (int i = 0; i < resultsLength; i++) {   //default resultsLength is 0, safe to iterate
                    info.results[i] = (VkResult)results[i];
                }

                if (!(result == VkResult.Success || result == VkResult.SuboptimalKhr)) throw new QueueException(result, string.Format("Error presenting from queue: {0}", result));

                return result;
            }
        }

        VkSparseMemoryBind Marshal(SparseMemoryBind bind) {
            var result = new VkSparseMemoryBind();
            result.resourceOffset = bind.resourceOffset;
            result.size = bind.size;
            result.memory = bind.memory.Native;
            result.memoryOffset = bind.memoryOffset;
            result.flags = bind.flags;

            return result;
        }

        VkSparseImageMemoryBind MarshalImage(SparseImageMemoryBind bind) {
            var result = new VkSparseImageMemoryBind();
            result.subresource = bind.subresource;
            result.offset = bind.offset;
            result.extent = bind.extent;
            result.memory = bind.memory.Native;
            result.memoryOffset = bind.memoryOffset;
            result.flags = bind.flags;

            return result;
        }

        public void BindSparse(IList<BindSparseInfo> bindInfo, Fence fence) {
            VkFence fenceNative = VkFence.Null;
            if (fence != null) {
                fenceNative = fence.Native;
            }

            if (bindInfo == null || bindInfo.Count == 0) {
                Device.Commands.queueBindSparse(queue, 0, IntPtr.Zero, fenceNative);
            }

            unsafe {
                int totalWaitSemaphores = 0;
                int totalSignalSemaphores = 0;
                int totalBufferBinds = 0;
                int totalImageOpaqueBinds = 0;
                int totalImageBinds = 0;
                int totalMemoryBinds = 0;
                int totalImageMemoryBinds = 0;

                for (int i = 0; i < bindInfo.Count; i++) {
                    var info = bindInfo[i];
                    if (info.waitSemaphores != null) totalWaitSemaphores += info.waitSemaphores.Count;
                    if (info.signalSemaphores != null) totalSignalSemaphores += info.signalSemaphores.Count;

                    if (info.bufferBinds != null) {
                        totalBufferBinds += info.bufferBinds.Count;

                        for (int j = 0; j < info.bufferBinds.Count; j++) {
                            totalMemoryBinds += info.bufferBinds[j].binds.Count;
                        }
                    }

                    if (info.imageOpaqueBinds != null) {
                        totalImageOpaqueBinds += info.imageOpaqueBinds.Count;

                        for (int j = 0; j < info.imageOpaqueBinds.Count; j++) {
                            totalMemoryBinds += info.imageOpaqueBinds[j].binds.Count;
                        }
                    }

                    if (info.imageBinds != null) {
                        totalImageBinds += info.imageBinds.Count;

                        for (int j = 0; j < info.imageBinds.Count; j++) {
                            totalImageMemoryBinds += info.imageBinds[j].binds.Count;
                        }
                    }
                }

                var waitSemaphoresNative = stackalloc VkSemaphore[totalWaitSemaphores];
                var signalSemaphoresNative = stackalloc VkSemaphore[totalSignalSemaphores];
                var bufferBindsNative = stackalloc VkSparseBufferMemoryBindInfo[totalBufferBinds];
                var imageOpaqueBindsNative = stackalloc VkSparseImageOpaqueMemoryBindInfo[totalImageOpaqueBinds];
                var imageBindsNative = stackalloc VkSparseImageMemoryBindInfo[totalImageBinds];
                var memoryBindsNative = stackalloc VkSparseMemoryBind[totalMemoryBinds];
                var imageMemoryBindsNative = stackalloc VkSparseImageMemoryBind[totalImageMemoryBinds];

                int waitSemaphoresIndex = 0;
                int signalSemaphoresIndex = 0;
                int bufferBindIndex = 0;
                int imageOpaqueBindIndex = 0;
                int imageBindIndex = 0;
                int memoryBindIndex = 0;
                int imageMemoryBindIndex = 0;

                var infosNative = stackalloc VkBindSparseInfo[bindInfo.Count];

                for (int i = 0; i < bindInfo.Count; i++) {
                    var info = infosNative[i];
                    info.sType = VkStructureType.BindSparseInfo;

                    if (bindInfo[i].waitSemaphores != null) {
                        int waitCount = bindInfo[i].waitSemaphores.Count;
                        Interop.Marshal<VkSemaphore, Semaphore>(bindInfo[i].waitSemaphores, &waitSemaphoresNative[waitSemaphoresIndex]);

                        info.waitSemaphoreCount = (uint)waitCount;
                        info.pWaitSemaphores = (IntPtr)(&waitSemaphoresNative[waitSemaphoresIndex]);    //get address from index
                        waitSemaphoresIndex += waitCount;  //increment index
                    }

                    if (bindInfo[i].signalSemaphores != null) {
                        int signalCount = bindInfo[i].signalSemaphores.Count;
                        Interop.Marshal<VkSemaphore, Semaphore>(bindInfo[i].signalSemaphores, &signalSemaphoresNative[signalSemaphoresIndex]);

                        info.signalSemaphoreCount = (uint)bindInfo[i].signalSemaphores.Count;
                        info.pSignalSemaphores = (IntPtr)(&signalSemaphoresNative[signalSemaphoresIndex]);  //get address from index
                        signalSemaphoresIndex += bindInfo[i].signalSemaphores.Count;  //increment index
                    }

                    if (bindInfo[i].bufferBinds != null) {
                        info.bufferBindCount = (uint)bindInfo[i].bufferBinds.Count;
                        info.pBufferBinds = (IntPtr)(&bufferBindsNative[bufferBindIndex]);

                        for (int j = 0; j < bindInfo[i].bufferBinds.Count; j++) {
                            var bufferBind = bindInfo[i].bufferBinds[j];
                            var bufferBindNative = new VkSparseBufferMemoryBindInfo();

                            bufferBindNative.buffer = bufferBind.buffer.Native;
                            bufferBindNative.bindCount = (uint)bufferBind.binds.Count;
                            bufferBindNative.pBinds = (IntPtr)(&memoryBindsNative[memoryBindIndex]);

                            for (int k = 0; k < bufferBind.binds.Count; k++) {
                                var bind = bufferBind.binds[k];
                                memoryBindsNative[memoryBindIndex] = Marshal(bind);
                                memoryBindIndex++;
                            }

                            bufferBindsNative[bufferBindIndex] = bufferBindNative;
                            bufferBindIndex++;
                        }
                    }

                    if (bindInfo[i].imageOpaqueBinds != null) {
                        info.imageOpaqueBindCount = (uint)bindInfo[i].imageOpaqueBinds.Count;
                        info.pImageOpaqueBinds = (IntPtr)(&imageOpaqueBindsNative[imageOpaqueBindIndex]);

                        for (int j = 0; j < bindInfo[i].imageOpaqueBinds.Count; j++) {
                            var imageOpaqueBind = bindInfo[i].imageOpaqueBinds[j];
                            var imageOpaqueBindNative = new VkSparseImageOpaqueMemoryBindInfo();

                            imageOpaqueBindNative.image = imageOpaqueBind.image.Native;
                            imageOpaqueBindNative.bindCount = (uint)imageOpaqueBind.binds.Count;
                            imageOpaqueBindNative.pBinds = (IntPtr)(&memoryBindsNative[memoryBindIndex]);

                            for (int k = 0; k < imageOpaqueBind.binds.Count; k++) {
                                var bind = imageOpaqueBind.binds[k];
                                memoryBindsNative[memoryBindIndex] = Marshal(bind);
                                memoryBindIndex++;
                            }

                            imageOpaqueBindsNative[bufferBindIndex] = imageOpaqueBindNative;
                            imageOpaqueBindIndex++;
                        }
                    }

                    if (bindInfo[i].imageBinds != null) {
                        info.imageBindCount = (uint)bindInfo[i].imageBinds.Count;
                        info.pImageBinds = (IntPtr)(&imageBindsNative[imageBindIndex]);

                        for (int j = 0; j < bindInfo[i].imageBinds.Count; j++) {
                            var imageBind = bindInfo[i].imageBinds[j];
                            var imageBindNative = new VkSparseImageMemoryBindInfo();

                            imageBindNative.image = imageBind.image.Native;
                            imageBindNative.bindCount = (uint)imageBind.binds.Count;
                            imageBindNative.pBinds = (IntPtr)(&imageMemoryBindsNative[imageMemoryBindIndex]);

                            for (int k = 0; k < imageBind.binds.Count; k++) {
                                var bind = imageBind.binds[k];
                                imageMemoryBindsNative[memoryBindIndex] = MarshalImage(bind);
                                imageMemoryBindIndex++;
                            }

                            imageBindsNative[bufferBindIndex] = imageBindNative;
                            imageBindIndex++;
                        }
                    }

                    infosNative[i] = info;
                }

                var result = Device.Commands.queueBindSparse(queue, (uint)bindInfo.Count, (IntPtr)infosNative, fenceNative);
                if (result != VkResult.Success) throw new QueueException(result, string.Format("Error binding to queue: {0}", result));
            }
        }
    }

    public class QueueException : VulkanException {
        public QueueException(VkResult result, string message) : base(result, message) { }
    }
}
