using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkFence : IDisposable, INative<Unmanaged.VkFence> {
        Unmanaged.VkFence fence;
        bool disposed = false;

        public Unmanaged.VkFence Native {
            get {
                return fence;
            }
        }

        public VkResult Status {
            get {
                var result = Device.Commands.getFenceStatus(Device.Native, fence);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(result, string.Format("Error getting status: {0}", result));
                return result;
            }
        }

        public VkDevice Device { get; private set; }

        public VkFence(VkDevice device, VkFenceCreateFlags flags) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateFence(flags);
        }

        public VkFence(VkDevice device) : this(device, VkFenceCreateFlags.None) { }

        void CreateFence(VkFenceCreateFlags flags) {
            var info = new Unmanaged.VkFenceCreateInfo();
            info.sType = VkStructureType.FenceCreateInfo;
            info.flags = flags;

            var result = Device.Commands.createFence(Device.Native, ref info, Device.Instance.AllocationCallbacks, out fence);
            if (result != VkResult.Success) throw new FenceException(result, string.Format("Error creating fence: {0}", result));
        }

        public void Reset() {
            unsafe {
                var fenceNative = fence;
                var result = Device.Commands.resetFences(Device.Native, 1, (IntPtr)(&fenceNative));
                if (result != VkResult.Success) throw new FenceException(result, string.Format("Error resetting fence: {0}", result));
            }
        }

        public VkResult Wait(ulong timeout) {
            unsafe {
                var fenceNative = fence;
                var result = Device.Commands.waitFences(Device.Native, 1, (IntPtr)(&fenceNative), 0, timeout);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(result, string.Format("Error waiting on fence: {0}", result));
                return result;
            }
        }

        public VkResult Wait() {
            return Wait(ulong.MaxValue);
        }

        public static void Reset(VkDevice device, IList<VkFence> fences) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc Unmanaged.VkFence[fences.Count];
                Interop.Marshal<Unmanaged.VkFence, VkFence>(fences, fencesNative);

                var result = device.Commands.resetFences(device.Native, (uint)fences.Count, (IntPtr)fencesNative);
                if (result != VkResult.Success) throw new FenceException(result, string.Format("Error resetting fences: {0}", result));
            }
        }

        public static VkResult Wait(VkDevice device, IList<VkFence> fences, bool waitAll, ulong timeout) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            unsafe {
                var fencesNative = stackalloc Unmanaged.VkFence[fences.Count];
                Interop.Marshal<Unmanaged.VkFence, VkFence>(fences, fencesNative);

                uint waitAllNative = waitAll ? 1u : 0u;
                var result = device.Commands.waitFences(device.Native, (uint)fences.Count, (IntPtr)fencesNative, waitAllNative, timeout);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(result, string.Format("Error waiting on fences: {0}", result));
                return result;
            }
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

        ~VkFence() {
            Dispose(false);
        }
    }

    public class FenceException : VulkanException {
        public FenceException(VkResult result, string message) : base(result, message) { }
    }
}
