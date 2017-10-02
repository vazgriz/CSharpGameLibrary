using System;

namespace CSGL.Vulkan {
    public class VkSemaphore : IDisposable, INative<Unmanaged.VkSemaphore> {
        Unmanaged.VkSemaphore semaphore;
        bool disposed = false;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkSemaphore Native {
            get {
                return semaphore;
            }
        }

        public VkSemaphore(VkDevice device) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateSemaphore();
        }

        void CreateSemaphore() {
            var info = new Unmanaged.VkSemaphoreCreateInfo();
            info.sType = VkStructureType.SemaphoreCreateInfo;

            var result = Device.Commands.createSemaphore(Device.Native, ref info, Device.Instance.AllocationCallbacks, out semaphore);
            if (result != VkResult.Success) throw new SemaphoreException(result, string.Format("Error creating semaphore: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroySemaphore(Device.Native, semaphore, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkSemaphore() {
            Dispose(false);
        }
    }

    public class SemaphoreException : VulkanException {
        public SemaphoreException(VkResult result, string message) : base(result, message) { }
    }
}
