using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceQueueCreateInfo {
        public uint queueFamilyIndex;
        public uint queueCount;
        public float[] priorities;

        public DeviceQueueCreateInfo(uint queueFamilyIndex, uint queueCount, float[] priorities) {
            this.queueFamilyIndex = queueFamilyIndex;
            this.queueCount = queueCount;
            this.priorities = priorities;
        }
    }

    public class SubmitInfo {
        public Semaphore[] waitSemaphores;
        public VkPipelineStageFlags[] waitDstStageMask;
        public CommandBuffer[] commandBuffers;
        public Semaphore[] signalSemaphores;
    }

    public class PresentInfo {
        public Semaphore[] waitSemaphores;
        public Swapchain[] swapchains;
        public uint[] imageIndices;
        public VkResult[] results;
    }

    public class Queue {
        VkQueue queue;

        Device device;

        internal Queue(Device device, VkQueue queue) {
            this.device = device;
            this.queue = queue;
        }

        public Device Device {
            get {
                return device;
            }
        }

        public void WaitIdle() {
            Device.Commands.queueWaitIdle(queue);
        }

        public VkResult Submit(SubmitInfo[] infos, Fence fence = null) {
            VkFence fenceNative = VkFence.Null;
            if (fence != null) {
                fenceNative = fence.Native;
            }

            if (infos == null || infos.Length == 0) {
                return Device.Commands.queueSubmit(queue, 0, IntPtr.Zero, fenceNative);
            }

            unsafe
            {
                int totalWaitSemaphores = 0;
                int totalCommandBuffers = 0;
                int totalSignalSemaphores = 0;

                for (int i = 0; i < infos.Length; i++) {    //get the total length needed for each array
                    var info = infos[i];
                    if (info.waitSemaphores != null) totalWaitSemaphores += info.waitSemaphores.Length;
                    if (info.commandBuffers != null) totalCommandBuffers += info.commandBuffers.Length;
                    if (info.signalSemaphores != null) totalSignalSemaphores += info.signalSemaphores.Length;
                }

                var waitSemaphoresNative = stackalloc VkSemaphore[totalWaitSemaphores];
                var waitDstNative = stackalloc int[totalWaitSemaphores];    //required to be the same length as above
                var commandBuffersNative = stackalloc VkCommandBuffer[totalCommandBuffers];
                var signalSemaphoresNative = stackalloc VkSemaphore[totalSignalSemaphores];

                int waitSemaphoresIndex = 0;
                int commandBuffersIndex = 0;
                int signalSemaphoresIndex = 0;

                var infosNative = stackalloc VkSubmitInfo[infos.Length];

                for (int i = 0; i < infos.Length; i++) {
                    var info = new VkSubmitInfo();
                    info.sType = VkStructureType.SubmitInfo;
                    
                    Interop.Marshal<VkSemaphore>(infos[i].waitSemaphores, &waitSemaphoresNative[waitSemaphoresIndex]);
                    Interop.Marshal(infos[i].waitDstStageMask, &waitDstNative[waitSemaphoresIndex]);
                    info.waitSemaphoreCount = (uint)infos[i].waitSemaphores.Length;
                    info.pWaitSemaphores = (IntPtr)(&waitSemaphoresNative[waitSemaphoresIndex]);    //get address from index
                    info.pWaitDstStageMask = (IntPtr)(&waitDstNative[waitSemaphoresIndex]);
                    waitSemaphoresIndex += infos[i].waitSemaphores.Length;  //increment index
                    
                    Interop.Marshal<VkCommandBuffer>(infos[i].commandBuffers, &commandBuffersNative[commandBuffersIndex]);
                    info.commandBufferCount = (uint)infos[i].commandBuffers.Length;
                    info.pCommandBuffers = (IntPtr)(&commandBuffersNative[commandBuffersIndex]);    //get address from index
                    commandBuffersIndex += infos[i].commandBuffers.Length;
                    
                    Interop.Marshal<VkSemaphore>(infos[i].signalSemaphores, &signalSemaphoresNative[signalSemaphoresIndex]);
                    info.signalSemaphoreCount = (uint)infos[i].signalSemaphores.Length;
                    info.pSignalSemaphores = (IntPtr)(&signalSemaphoresNative[signalSemaphoresIndex]);  //get address from index
                    signalSemaphoresIndex += infos[i].signalSemaphores.Length;

                    infosNative[i] = info;
                }

                var result = Device.Commands.queueSubmit(queue, (uint)infos.Length, (IntPtr)infosNative, fenceNative);

                return result;
            }
        }

        public VkResult Present(PresentInfo info) {
            unsafe
            {
                var waitSemaphoresNative = stackalloc VkSemaphore[info.waitSemaphores.Length];

                var waitSemaphoresMarshalled = new NativeArray<VkSemaphore>(info.waitSemaphores);
                var swapchainsMarshalled = new NativeArray<VkSwapchainKHR>(info.swapchains);
                var indicesMarshalled = new PinnedArray<uint>(info.imageIndices);
                NativeArray<int> resultsMarshalled = null;

                if (info.results != null) {
                    resultsMarshalled = new NativeArray<int>(info.results.Length);
                }

                var infoNative = new VkPresentInfoKHR();
                infoNative.sType = VkStructureType.PresentInfoKhr;
                infoNative.waitSemaphoreCount = (uint)waitSemaphoresMarshalled.Count;
                infoNative.pWaitSemaphores = waitSemaphoresMarshalled.Address;
                infoNative.swapchainCount = (uint)swapchainsMarshalled.Count;
                infoNative.pSwapchains = swapchainsMarshalled.Address;
                infoNative.pImageIndices = indicesMarshalled.Address;

                var result = Device.Commands.queuePresent(queue, ref infoNative);

                if (info.results != null) {
                    for (int i = 0; i < info.results.Length; i++) {
                        info.results[i] = (VkResult)resultsMarshalled[i];
                    }
                    resultsMarshalled.Dispose();
                }

                waitSemaphoresMarshalled.Dispose();
                swapchainsMarshalled.Dispose();
                indicesMarshalled.Dispose();

                return result;
            }
        }
    }
}
