using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class QueueCreateInfo : IDisposable {
        uint queueFamilyIndex;
        uint queueCount;
        float[] priorities;

        bool disposed = false;
        bool dirty = false;

        Marshalled<VkDeviceQueueCreateInfo> marshalled;
        MarshalledArray<float> prioritiesMarshalled;

        public uint QueueFamilyIndex {
            get {
                return queueFamilyIndex;
            }
            set {
                queueFamilyIndex = value;
                dirty = true;
            }
        }

        public uint QueueCount {
            get {
                return queueCount;
            }
            set {
                queueCount = value;
                dirty = true;
            }
        }

        public float[] Priorities {
            get {
                return priorities;
            }
            set {
                priorities = value;
                dirty = true;
            }
        }

        public Marshalled<VkDeviceQueueCreateInfo> Marshalled {
            get {
                if (dirty) {
                    Apply();
                }
                return marshalled;
            }
        }

        public QueueCreateInfo(uint queueFamilyIndex, uint queueCount, float[] priorities) {
            QueueFamilyIndex = queueFamilyIndex;
            QueueCount = queueCount;
            Priorities = priorities;

            marshalled = new Marshalled<VkDeviceQueueCreateInfo>();

            Apply();
        }

        public void Apply() {
            prioritiesMarshalled?.Dispose();
            prioritiesMarshalled = new MarshalledArray<float>(priorities);

            marshalled.Value = GetNative();
        }

        VkDeviceQueueCreateInfo GetNative() {
            VkDeviceQueueCreateInfo info = new VkDeviceQueueCreateInfo();
            info.sType = VkStructureType.StructureTypeDeviceQueueCreateInfo;
            info.queueCount = queueCount;
            info.queueFamilyIndex = queueFamilyIndex;
            info.pQueuePriorities = prioritiesMarshalled.Address;

            return info;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            marshalled.Dispose();
            prioritiesMarshalled.Dispose();

            if (disposing) {
                marshalled = null;
                prioritiesMarshalled = null;
                priorities = null;
            }
        }

        ~QueueCreateInfo() {
            Dispose(false);
        }
    }

    public class SubmitInfo {
        public Semaphore[] WaitSemaphores { get; set; }
        public VkPipelineStageFlags[] WaitDstStageMask { get; set; }
        public CommandBuffer[] CommandBuffers { get; set; }
        public Semaphore[] SignalSemaphores { get; set; }

        internal VkSubmitInfo GetNative(List<IDisposable> marshalled) {
            VkSubmitInfo info = new VkSubmitInfo();
            info.sType = VkStructureType.StructureTypeSubmitInfo;
            
            if (WaitSemaphores != null) {
                info.waitSemaphoreCount = (uint)WaitSemaphores.Length;
                var waitMarshalled = new PinnedArray<VkSemaphore>(WaitSemaphores.Length);

                for (int i = 0; i < WaitSemaphores.Length; i++) {
                    waitMarshalled[i] = WaitSemaphores[i].Native;
                }
                
                info.pWaitSemaphores = waitMarshalled.Address;

                marshalled.Add(waitMarshalled);
            }

            if (WaitDstStageMask != null) {
                var waitDstMarshalled = new PinnedArray<int>(WaitDstStageMask.Length);

                for (int i = 0; i < WaitDstStageMask.Length; i++) {
                    waitDstMarshalled[i] = (int)WaitDstStageMask[i];
                }

                info.pWaitDstStageMask = waitDstMarshalled.Address;

                marshalled.Add(waitDstMarshalled);
            }

            if (CommandBuffers != null) {
                info.commandBufferCount = (uint)CommandBuffers.Length;
                var commandBuffersMarshalled = new PinnedArray<VkCommandBuffer>(CommandBuffers.Length);

                for (int i = 0; i < CommandBuffers.Length; i++) {
                    commandBuffersMarshalled[i] = CommandBuffers[i].Native;
                }

                info.pCommandBuffers = commandBuffersMarshalled.Address;

                marshalled.Add(commandBuffersMarshalled);
            }

            if (SignalSemaphores != null) {
                info.signalSemaphoreCount = (uint)SignalSemaphores.Length;
                var signalMarshalled = new PinnedArray<VkSemaphore>(SignalSemaphores.Length);

                for (int i = 0; i < SignalSemaphores.Length; i++) {
                    signalMarshalled[i] = SignalSemaphores[i].Native;
                }

                info.pSignalSemaphores = signalMarshalled.Address;

                marshalled.Add(signalMarshalled);
            }

            return info;
        }
    }

    public class PresentInfo {
        public Semaphore[] WaitSemaphores { get; set; }
        public Swapchain[] Swapchains { get; set; }
        public uint[] ImageIndices { get; set; }
        public IntPtr Results { get; set; }

        internal VkPresentInfoKHR GetNative(List<IDisposable> marshalled) {
            VkPresentInfoKHR info = new VkPresentInfoKHR();
            info.sType = VkStructureType.StructureTypePresentInfoKhr;
            
            info.waitSemaphoreCount = (uint)WaitSemaphores.Length;
            var waitMarshalled = new PinnedArray<VkSemaphore>(WaitSemaphores.Length);

            for (int i = 0; i < WaitSemaphores.Length; i++) {
                waitMarshalled[i] = WaitSemaphores[i].Native;
            }

            info.pWaitSemaphores = waitMarshalled.Address;
            marshalled.Add(waitMarshalled);
            
            info.swapchainCount = (uint)Swapchains.Length;
            var swapchainsMarshalled = new PinnedArray<VkSwapchainKHR>(Swapchains.Length);

            for (int i = 0; i < swapchainsMarshalled.Length; i++) {
                swapchainsMarshalled[i] = Swapchains[i].Native;
            }

            info.pSwapchains = swapchainsMarshalled.Address;
            marshalled.Add(swapchainsMarshalled);
            
            var indicesMarshalled = new PinnedArray<uint>(ImageIndices);
            info.pImageIndices = indicesMarshalled.Address;
            marshalled.Add(indicesMarshalled);

            info.pResults = Results;

            return info;
        }
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
            List<IDisposable> marshalled = new List<IDisposable>();

            var infosMarshalled = new PinnedArray<VkSubmitInfo>(infos.Length);

            for (int i = 0; i < infos.Length; i++) {
                infosMarshalled[i] = infos[i].GetNative(marshalled);
            }

            var result = Device.Commands.queueSubmit(queue, (uint)infos.Length, infosMarshalled.Address, temp);

            foreach (var m in marshalled) m.Dispose();

            return result;
        }

        public VkResult Present(PresentInfo info) {
            List<IDisposable> marshalled = new List<IDisposable>();
            var infoMarshalled = new Marshalled<VkPresentInfoKHR>(info.GetNative(marshalled));

            var result = Device.Commands.queuePresent(queue, infoMarshalled.Address);

            foreach (var m in marshalled) m.Dispose();

            return result;
        }
    }
}
