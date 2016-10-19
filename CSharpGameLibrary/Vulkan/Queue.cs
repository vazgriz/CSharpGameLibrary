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

        public VkResult Submit(SubmitInfo[] infos, Fence fence) {
            VkFence temp = VkFence.Null;
            if (fence != null) {
                temp = fence.Native;
            }

            var infosMarshalled = new MarshalledArray<VkSubmitInfo>(infos.Length);
            IDisposable[] disposables = new IDisposable[infos.Length * 4];

            for (int i = 0; i < infos.Length; i++) {
                var info = new VkSubmitInfo();
                info.sType = VkStructureType.StructureTypeSubmitInfo;

                var waitMarshalled = new PinnedArray<VkSemaphore>(infos[i].waitSemaphores.Length);
                info.waitSemaphoreCount = (uint)waitMarshalled.Length;
                for (int j = 0; j < waitMarshalled.Length; j++) {
                    waitMarshalled[j] = infos[i].waitSemaphores[j].Native;
                }
                info.pWaitSemaphores = waitMarshalled.Address;

                var waitDstMarshalled = new PinnedArray<int>(infos[i].waitDstStageMask.Length);
                for (int j = 0; j < waitDstMarshalled.Length; j++) {
                    waitDstMarshalled[j] = (int)infos[i].waitDstStageMask[j];
                }
                info.pWaitDstStageMask = waitDstMarshalled.Address;

                var commandBuffersMarshalled = new PinnedArray<VkCommandBuffer>(infos[i].commandBuffers.Length);
                info.commandBufferCount = (uint)commandBuffersMarshalled.Length;
                for (int j = 0; j < commandBuffersMarshalled.Length; j++) {
                    commandBuffersMarshalled[j] = infos[i].commandBuffers[j].Native;
                }
                info.pCommandBuffers = commandBuffersMarshalled.Address;

                var signalMarshalled = new PinnedArray<VkSemaphore>(infos[i].signalSemaphores.Length);
                info.signalSemaphoreCount = (uint)signalMarshalled.Length;
                for (int j = 0; j < signalMarshalled.Length; j++) {
                    signalMarshalled[j] = infos[i].signalSemaphores[j].Native;
                }
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
            var waitSemaphoresMarshalled = new MarshalledArray<VkSemaphore>(info.waitSemaphores.Length);
            var swapchainsMarshalled = new MarshalledArray<VkSwapchainKHR>(info.swapchains.Length);
            var indicesMarshalled = new PinnedArray<uint>(info.imageIndices);
            MarshalledArray<int> resultsMarshalled = null;

            for (int i = 0; i < waitSemaphoresMarshalled.Count; i++) {
                waitSemaphoresMarshalled[i] = info.waitSemaphores[i].Native;
            }

            for (int i = 0; i < swapchainsMarshalled.Count; i++) {
                swapchainsMarshalled[i] = info.swapchains[i].Native;
            }

            if (info.results != null) {
                resultsMarshalled = new MarshalledArray<int>(info.results.Length);
            }

            var infoNative = new VkPresentInfoKHR();
            infoNative.sType = VkStructureType.StructureTypePresentInfoKhr;
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
