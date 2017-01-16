using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        public int waitCount = -1;
        public CommandBuffer[] commandBuffers;
        public int commandBufferCount = -1;
        public Semaphore[] signalSemaphores;
        public int signalCount = -1;
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
                    if (info.waitSemaphores != null) {
                        if (info.waitCount == -1) {
                            totalWaitSemaphores += info.waitSemaphores.Length;
                        } else {
                            totalWaitSemaphores += info.waitCount;
                        }
                    }
                    if (info.commandBuffers != null) {
                        if (info.commandBufferCount == -1) {
                            totalCommandBuffers += info.commandBuffers.Length;
                        } else {
                            totalCommandBuffers += info.commandBufferCount;
                        }
                    }
                    if (info.signalSemaphores != null) {
                        if (info.signalCount == -1) {
                            totalSignalSemaphores += info.signalSemaphores.Length;
                        } else {
                            totalSignalSemaphores += info.signalCount;
                        }
                    }
                }

                var waitSemaphoresNative = stackalloc VkSemaphore[totalWaitSemaphores];
                var waitDstNative = stackalloc int[totalWaitSemaphores];    //required to be the same length as above
                var commandBuffersNative = stackalloc VkCommandBuffer[totalCommandBuffers];
                var signalSemaphoresNative = stackalloc VkSemaphore[totalSignalSemaphores];

                uint waitSemaphoresIndex = 0;
                uint commandBuffersIndex = 0;
                uint signalSemaphoresIndex = 0;

                var infosNative = stackalloc VkSubmitInfo[infos.Length];

                for (int i = 0; i < infos.Length; i++) {
                    var info = new VkSubmitInfo();
                    info.sType = VkStructureType.SubmitInfo;


                    if (infos[i].waitSemaphores != null) {
                        uint waitCount;

                        if (infos[i].waitCount == -1) {
                            waitCount = (uint)infos[i].waitSemaphores.Length;
                        } else {
                            waitCount = (uint)infos[i].waitCount;
                        }

                        Interop.Marshal(infos[i].waitSemaphores, &waitSemaphoresNative[waitSemaphoresIndex], (int)waitCount);

                        for (int j = 0; j < waitCount; j++) {
                            //this has to copied manually because Marshal.SizeOf<> doesn't accept enums
                            waitDstNative[waitSemaphoresIndex + j] = (int)infos[i].waitDstStageMask[j];
                        }

                        info.waitSemaphoreCount = waitCount;
                        info.pWaitSemaphores = (IntPtr)(&waitSemaphoresNative[waitSemaphoresIndex]);    //get address from index
                        info.pWaitDstStageMask = (IntPtr)(&waitDstNative[waitSemaphoresIndex]);
                        waitSemaphoresIndex += waitCount;  //increment index
                    }

                    if (infos[i].commandBuffers != null) {
                        uint commandBufferCount;

                        if (infos[i].commandBufferCount == -1) {
                            commandBufferCount = (uint)infos[i].commandBuffers.Length;
                        } else {
                            commandBufferCount = (uint)infos[i].commandBufferCount;
                        }

                        Interop.Marshal(infos[i].commandBuffers, &commandBuffersNative[commandBuffersIndex], (int)commandBufferCount);
                        info.commandBufferCount = commandBufferCount;
                        info.pCommandBuffers = (IntPtr)(&commandBuffersNative[commandBuffersIndex]);    //get address from index
                        commandBuffersIndex += commandBufferCount;  //increment index
                    }

                    if (infos[i].signalSemaphores != null) {
                        uint signalCount;

                        if (infos[i].signalCount == -1) {
                            signalCount = (uint)infos[i].signalSemaphores.Length;
                        } else {
                            signalCount = (uint)infos[i].signalCount;
                        }

                        Interop.Marshal(infos[i].signalSemaphores, &signalSemaphoresNative[signalSemaphoresIndex], (int)signalCount);
                        info.signalSemaphoreCount = signalCount;
                        info.pSignalSemaphores = (IntPtr)(&signalSemaphoresNative[signalSemaphoresIndex]);  //get address from index
                        signalSemaphoresIndex += signalCount;  //increment index
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
                var waitSemaphoresNative = stackalloc VkSemaphore[info.waitSemaphores.Length];
                Interop.Marshal<VkSemaphore>(info.waitSemaphores, waitSemaphoresNative);

                var swapchainsNative = stackalloc VkSwapchainKHR[info.swapchains.Length];
                Interop.Marshal<VkSwapchainKHR>(info.swapchains, swapchainsNative);

                //info.indices is uint[], so it can be pinned and read directly
                GCHandle handle = GCHandle.Alloc(info.imageIndices, GCHandleType.Pinned);

                int resultsLength = 0;
                if (info.results != null) {
                    resultsLength = info.results.Length;
                }
                var results = stackalloc int[resultsLength];

                var infoNative = new VkPresentInfoKHR();
                infoNative.sType = VkStructureType.PresentInfoKhr;
                infoNative.waitSemaphoreCount = (uint)info.waitSemaphores.Length;
                infoNative.pWaitSemaphores = (IntPtr)waitSemaphoresNative;
                infoNative.swapchainCount = (uint)info.swapchains.Length;
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
