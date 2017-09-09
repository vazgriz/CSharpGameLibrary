using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class FenceCreateInfo {
        public VkFenceCreateFlags Flags { get; set; }
    }

    public class Fence : IDisposable, INative<VkFence> {
        VkFence fence;
        bool disposed = false;

        public VkFence Native {
            get {
                return fence;
            }
        }

        public VkResult Status {
            get {
                return Device.Commands.getFenceStatus(Device.Native, fence);
            }
        }

        public Device Device { get; private set; }

        public static VkResult Reset(Device device, Fence[] fences) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc VkFence[fences.Length];
                Interop.Marshal(fences, fencesNative);

                var result = device.Commands.resetFences(device.Native, (uint)fences.Length, (IntPtr)fencesNative);
                if (result != VkResult.Success) throw new FenceException(result, string.Format("Error resetting fences: {0}", result));
                return result;
            }
        }

        public static VkResult Reset(Device device, List<Fence> fences) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc VkFence[fences.Count];
                Interop.Marshal<VkFence, Fence>(fences, fencesNative);

                var result = device.Commands.resetFences(device.Native, (uint)fences.Count, (IntPtr)fencesNative);
                if (result != VkResult.Success) throw new FenceException(result, string.Format("Error resetting fences: {0}", result));
                return result;
            }
        }

        public static VkResult Wait(Device device, Fence[] fences, bool waitAll, ulong timeout) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc VkFence[fences.Length];
                Interop.Marshal(fences, fencesNative);

                uint waitAllNative = waitAll ? 1u : 0u;
                var result = device.Commands.waitFences(device.Native, (uint)fences.Length, (IntPtr)fencesNative, waitAllNative, timeout);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(result, string.Format("Error waiting on fences: {0}", result));
                return result;
            }
        }

        public static VkResult Wait(Device device, List<Fence> fences, bool waitAll, ulong timeout) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc VkFence[fences.Count];
                Interop.Marshal<VkFence, Fence>(fences, fencesNative);

                uint waitAllNative = waitAll ? 1u : 0u;
                var result = device.Commands.waitFences(device.Native, (uint)fences.Count, (IntPtr)fencesNative, waitAllNative, timeout);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(result, string.Format("Error waiting on fences: {0}", result));
                return result;
            }
        }

        public Fence(Device device, FenceCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateFence(info);
        }

        void CreateFence(FenceCreateInfo mInfo) {
            VkFenceCreateInfo info = new VkFenceCreateInfo();
            info.sType = VkStructureType.FenceCreateInfo;
            info.flags = mInfo.Flags;

            var result = Device.Commands.createFence(Device.Native, ref info, Device.Instance.AllocationCallbacks, out fence);
            if (result != VkResult.Success) throw new FenceException(result, string.Format("Error creating fence: {0}", result));
        }

        public VkResult Reset() {
            return Reset(Device, new Fence[] { this });
        }

        public VkResult Wait(ulong timeout) {
            return Wait(Device, new Fence[] { this }, false, timeout);
        }

        public VkResult Wait() {
            return Wait(ulong.MaxValue);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyFence(Device.Native, fence, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~Fence() {
            Dispose(false);
        }
    }

    public class FenceException : VulkanException {
        public FenceException(VkResult result, string message) : base(result, message) { }
    }
}
