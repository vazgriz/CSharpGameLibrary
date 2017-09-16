using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkEvent : IDisposable, INative<Unmanaged.VkEvent> {
        Unmanaged.VkEvent _event;
        bool disposed;

        public Unmanaged.VkEvent Native {
            get {
                return _event;
            }
        }

        public VkDevice Device { get; private set; }

        public VkEvent(VkDevice device) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateEvent();
        }

        void CreateEvent() {
            var info = new Unmanaged.VkEventCreateInfo();
            info.sType = VkStructureType.EventCreateInfo;

            var result = Device.Commands.createEvent(Device.Native, ref info, Device.Instance.AllocationCallbacks, out _event);
            if (result != VkResult.Success) throw new EventException(result, string.Format("Error creating event: {0}", result));
        }

        public VkResult GetStatus() {
            return Device.Commands.getEventStatus(Device.Native, _event);
        }

        public void Set() {
            var result = Device.Commands.setEvent(Device.Native, _event);
            if (result != VkResult.Success) throw new EventException(result, string.Format("Error setting event: {0}", result));
        }

        public void Reset() {
            var result = Device.Commands.resetEvent(Device.Native, _event);
            if (result != VkResult.Success) throw new EventException(result, string.Format("Error resetting event: {0}", result));
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyEvent(Device.Native, _event, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkEvent() {
            Dispose(false);
        }
    }

    public class EventException : VulkanException {
        public EventException(VkResult result, string message) : base(result, message) { }
    }
}
