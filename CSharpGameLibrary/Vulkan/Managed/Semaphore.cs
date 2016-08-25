using System;

namespace CSGL.Vulkan.Managed {
    public class Semaphore : IDisposable {
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
            info.sType = VkStructureType.StructureTypeSemaphoreCreateInfo;

            var infoMarshalled = new Marshalled<VkSemaphoreCreateInfo>(info);
            var semaphoreMarshalled = new Marshalled<VkSemaphore>();

            try {
                var result = Device.Commands.createSemaphore(Device.Native, infoMarshalled.Address, Device.Instance.AllocationCallbacks, semaphoreMarshalled.Address);
                if (result != VkResult.Success) throw new SemaphoreException(string.Format("Error creating semaphore: {0}", result));
                semaphore = semaphoreMarshalled.Value;
            }
            finally {
                infoMarshalled.Dispose();
                semaphoreMarshalled.Dispose();
            }
        }

        public void Dispose() {
            if (disposed) return;

            disposed = true;
        }
    }

    public class SemaphoreException : Exception {
        public SemaphoreException(string message) : base(message) { }
    }
}
