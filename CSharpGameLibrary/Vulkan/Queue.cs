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

        public VkResult Submit(SubmitInfo[] infos, Fence fence) {
            VkFence temp = VkFence.Null;
            if (fence != null) {
                temp = fence.Native;
            }

            var infosMarshalled = new MarshalledArray<VkSubmitInfo>(infos.Length);
            IDisposable[] disposables = new IDisposable[infos.Length * 4];

            for (int i = 0; i < infos.Length; i++) {
                var info = new VkSubmitInfo();
                info.sType = VkStructureType.SubmitInfo;

                var waitMarshalled = new NativeArray<VkSemaphore>(infos[i].waitSemaphores);
                info.waitSemaphoreCount = (uint)waitMarshalled.Count;
                info.pWaitSemaphores = waitMarshalled.Address;

                var waitDstMarshalled = new NativeArray<int>(waitMarshalled.Count);     //waitDstStageMask is an array of enums, so it can't be used as int[]
                for (int j = 0; j < waitDstMarshalled.Count; j++) {                     //luckily, this is required to be the same length as waitSemaphores
                    waitDstMarshalled[j] = (int)infos[i].waitDstStageMask[j];
                }
                info.pWaitDstStageMask = waitDstMarshalled.Address;

                var commandBuffersMarshalled = new NativeArray<VkCommandBuffer>(infos[i].commandBuffers);
                info.commandBufferCount = (uint)commandBuffersMarshalled.Count;
                info.pCommandBuffers = commandBuffersMarshalled.Address;

                var signalMarshalled = new NativeArray<VkSemaphore>(infos[i].signalSemaphores);
                info.signalSemaphoreCount = (uint)signalMarshalled.Count;
                info.pSignalSemaphores = signalMarshalled.Address;

                disposables[(i * 4) + 0] = waitMarshalled;
                disposables[(i * 4) + 1] = waitDstMarshalled;
                disposables[(i * 4) + 2] = commandBuffersMarshalled;
                disposables[(i * 4) + 3] = signalMarshalled;

                infosMarshalled[i] = info;
            }

            var result = Device.Commands.queueSubmit(queue, (uint)infos.Length, infosMarshalled.Address, temp);

            for (int i = 0; i < disposables.Length; i++) {
                disposables[i].Dispose();
            }

            return result;
        }

        public VkResult Present(PresentInfo info) {
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
