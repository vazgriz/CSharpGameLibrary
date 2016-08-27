using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public partial class Device : IDisposable {
        VkDevice device;
        bool disposed = false;

        PhysicalDevice physicalDevice;

        vkGetDeviceProcAddrDelegate getDeviceProcAddr;

        public DeviceCommands Commands { get; private set; }

        public Instance Instance { get; private set; }
        public List<string> Extensions { get; private set; }

        public VkDevice Native {
            get {
                return device;
            }
        }

        public Device(PhysicalDevice physicalDevice, DeviceCreateInfo info) {
            if (physicalDevice == null) throw new ArgumentNullException(nameof(physicalDevice));
            if (info == null) throw new ArgumentNullException(nameof(info));

            this.physicalDevice = physicalDevice;
            Instance = physicalDevice.Instance;

            if (info.Extensions == null) {
                Extensions = new List<string>();
            } else {
                Extensions = info.Extensions;
            }

            ValidateExtensions();
            CreateDevice(info);

            Vulkan.Load(ref getDeviceProcAddr, Instance);
            Commands = new DeviceCommands(this);
        }

        void CreateDevice(DeviceCreateInfo mInfo) {
            var deviceMarshalled = new Marshalled<VkDevice>();

            try {
                var result = Instance.Commands.createDevice(physicalDevice.Native, mInfo.Marshalled.Address, Instance.AllocationCallbacks, deviceMarshalled.Address);
                if (result != VkResult.Success) throw new DeviceException(string.Format("Error creating device: {0}", result));

                device = deviceMarshalled.Value;
            }
            finally {
                deviceMarshalled.Dispose();
            }
        }

        void ValidateExtensions() {
            foreach (string ex in Extensions) {
                bool found = false;

                for (int i = 0; i < physicalDevice.AvailableExtensions.Count; i++) {
                    if (physicalDevice.AvailableExtensions[i].Name == ex) {
                        found = true;
                        break;
                    }
                }

                if (!found) throw new DeviceException(string.Format("Requested extension not available: {0}", ex));
            }
        }

        public Queue GetQueue(uint familyIndex, uint index) {
            var queueMarshalled = new Marshalled<VkQueue>();
            Commands.getDeviceQueue(device, familyIndex, index, queueMarshalled.Address);
            var result = new Queue(this, queueMarshalled.Value);

            queueMarshalled.Dispose();

            return result;
        }

        public IntPtr GetProcAdddress(string command) {
            return getDeviceProcAddr(device, Interop.GetUTF8(command));
        }

        public void WaitIdle() {
            Commands.waitDeviceIdle(device);
        }

        public void Dispose() {
            if (disposed) return;
            
            Instance.Commands.destroyDevice(device, Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class DeviceException : Exception {
        public DeviceException(string message) : base(message) { }
    }
}
