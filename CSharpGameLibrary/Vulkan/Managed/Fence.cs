using System;

namespace CSGL.Vulkan.Managed {
	public class FenceCreateInfo {
        public VkFenceCreateFlags Flags { get; set; }
    }

    public class Fence : IDisposable {
        VkFence fence;
        bool disposed = false;

		public VkFence Native {
            get {
                return fence;
            }
        }

		public Device Device { get; private set; }

		public Fence(Device device, FenceCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            CreateFence(info);
        }

		void CreateFence(FenceCreateInfo mInfo) {
            VkFenceCreateInfo info = new VkFenceCreateInfo();
            info.sType = VkStructureType.StructureTypeFenceCreateInfo;
            info.flags = mInfo.Flags;

            var infoMarshalled = new Marshalled<VkFenceCreateInfo>(info);
            var fenceMarshalled = new Marshalled<VkFence>();

            try {
                var result = Device.Commands.createFence(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, fenceMarshalled.Address);
                if (result != VkResult.Success) throw new FenceException(string.Format("Error creating fence: {0}", result));
                fence = fenceMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                fenceMarshalled.Dispose();
            }
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
