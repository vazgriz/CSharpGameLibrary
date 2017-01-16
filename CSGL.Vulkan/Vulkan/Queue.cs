﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class DeviceQueueCreateInfo {
        public uint queueFamilyIndex;
        public uint queueCount;
        public List<float> priorities;

        public DeviceQueueCreateInfo(uint queueFamilyIndex, uint queueCount, List<float> priorities) {
            this.queueFamilyIndex = queueFamilyIndex;
            this.queueCount = queueCount;
            this.priorities = priorities;
        }
    }

    public class SubmitInfo {
        public List<Semaphore> waitSemaphores;
        public List<VkPipelineStageFlags> waitDstStageMask;
        public List<CommandBuffer> commandBuffers;
        public List<Semaphore> signalSemaphores;
    }

    public class PresentInfo {
        public List<Semaphore> waitSemaphores;
        public List<Swapchain> swapchains;
        public List<uint> imageIndices;
        public List<VkResult> results;
    }

    public class Queue {
        VkQueue queue;

        Device device;

        public uint FamilyIndex { get; private set; }
        public QueueFamily Family { get; private set; }

        internal Queue(Device device, VkQueue queue, uint familyIndex) {
            this.device = device;
            this.queue = queue;
            FamilyIndex = familyIndex;
            Family = device.PhysicalDevice.QueueFamilies[(int)familyIndex];
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

                var infosNative = stackalloc VkSubmitInfo[infos.Length];

                for (int i = 0; i < infos.Length; i++) {
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

                var result = Device.Commands.queueSubmit(queue, (uint)infos.Length, (IntPtr)infosNative, fenceNative);

                return result;
            }
        }

        public VkResult Present(PresentInfo info) {
            unsafe
            {
                var waitSemaphoresNative = stackalloc VkSemaphore[info.waitSemaphores.Count];
                Interop.Marshal<VkSemaphore, Semaphore>(info.waitSemaphores, waitSemaphoresNative);

                var swapchainsNative = stackalloc VkSwapchainKHR[info.swapchains.Count];
                Interop.Marshal<VkSwapchainKHR, Swapchain>(info.swapchains, swapchainsNative);

                //info.indices is uint[], so it can be pinned and read directly
                GCHandle handle = GCHandle.Alloc(info.imageIndices, GCHandleType.Pinned);

                int resultsLength = 0;
                if (info.results != null) {
                    resultsLength = info.results.Count;
                }
                var results = stackalloc int[resultsLength];

                var infoNative = new VkPresentInfoKHR();
                infoNative.sType = VkStructureType.PresentInfoKhr;
                infoNative.waitSemaphoreCount = (uint)info.waitSemaphores.Count;
                infoNative.pWaitSemaphores = (IntPtr)waitSemaphoresNative;
                infoNative.swapchainCount = (uint)info.swapchains.Count;
                infoNative.pSwapchains = (IntPtr)swapchainsNative;
                infoNative.pImageIndices = handle.AddrOfPinnedObject();

                var result = Device.Commands.queuePresent(queue, ref infoNative);
                
                for (int i = 0; i < resultsLength; i++) {   //already determined if null
                    info.results[i] = (VkResult)results[i];
                }

                handle.Free();

                return result;
            }
        }
    }
}
