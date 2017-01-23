using System;

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

		public Device Device { get; private set; }

		public static VkResult Reset(Device device, Fence[] fences) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            var fencesNative = new VkFence[fences.Length];

			for (int i = 0; i < fencesNative.Length; i++) {
                fencesNative[i] = fences[i].Native;
            }

            using (var fencesMarshalled = new PinnedArray<VkFence>(fencesNative)) {
                var result = device.Commands.resetFences(device.Native, (uint)fences.Length, fencesMarshalled.Address);
                if (result != VkResult.Success) throw new FenceException(string.Format("Error resetting fences: {0}", result));
                return result;
            }
        }

		public static VkResult Wait(Device device, Fence[] fences, bool waitAll, ulong timeout) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (fences == null) throw new ArgumentNullException(nameof(fences));

            var fencesNative = new VkFence[fences.Length];

            for (int i = 0; i < fencesNative.Length; i++) {
                fencesNative[i] = fences[i].Native;
            }

            using (var fencesMarshalled = new PinnedArray<VkFence>(fencesNative)) {
                uint waitAllNative = waitAll ? 1u : 0u;
                var result = device.Commands.waitFences(device.Native, (uint)fences.Length, fencesMarshalled.Address, waitAllNative, timeout);
                if (!(result == VkResult.Success || result == VkResult.Timeout)) throw new FenceException(string.Format("Error waiting on fences: {0}", result));
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
            if (result != VkResult.Success) throw new FenceException(string.Format("Error creating fence: {0}", result));
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
            if (disposed) return;

            Device.Commands.destroyFence(Device.Native, fence, Device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

	public class FenceException : Exception {
		public FenceException(string message) : base(message) { }
    }
}
