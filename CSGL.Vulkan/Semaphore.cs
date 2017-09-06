using System;

namespace CSGL.Vulkan {
    public class Semaphore : IDisposable, INative<VkSemaphore> {
        VkSemaphore semaphore;
        bool disposed = false;

        public Device Device { get; private set; }

        public VkSemaphore Native {
            get {
                return semaphore;
            }
        }

        public Semaphore(Device device) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateSemaphore();
        }

        void CreateSemaphore() {
            VkSemaphoreCreateInfo info = new VkSemaphoreCreateInfo();
            info.sType = VkStructureType.SemaphoreCreateInfo;

            var result = Device.Commands.createSemaphore(Device.Native, ref info, Device.Instance.AllocationCallbacks, out semaphore);
            if (result != VkResult.Success) throw new SemaphoreException(string.Format("Error creating semaphore: {0}", result));
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

        ~Semaphore() {
            Dispose(false);
        }
    }

    public class SemaphoreException : Exception {
        public SemaphoreException(string message) : base(message) { }
    }
}
