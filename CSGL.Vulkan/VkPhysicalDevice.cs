using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkPhysicalDevice : INative<Unmanaged.VkPhysicalDevice> {
        Unmanaged.VkPhysicalDevice physicalDevice;

        public string Name { get; private set; }
        public VkInstance Instance { get; private set; }
        public VkPhysicalDeviceProperties Properties { get; private set; }
        public IList<VkQueueFamily> QueueFamilies { get; private set; }
        public IList<VkExtension> AvailableExtensions { get; private set; }

        public Unmanaged.VkPhysicalDevice Native {
            get {
                return physicalDevice;
            }
        }

        Unmanaged.VkPhysicalDeviceFeatures features;
        public Unmanaged.VkPhysicalDeviceFeatures Features {
            get {
                return features;
            }
        }

        Unmanaged.VkPhysicalDeviceMemoryProperties memoryProperties;
        public Unmanaged.VkPhysicalDeviceMemoryProperties MemoryProperties {
            get {
                return memoryProperties;
            }
        }

        internal VkPhysicalDevice(VkInstance instance, Unmanaged.VkPhysicalDevice physicalDevice) {
            Instance = instance;
            this.physicalDevice = physicalDevice;

            GetDeviceProperties();
            GetQueueProperties();
            GetDeviceFeatures();
            GetDeviceExtensions();
            GetMemoryProperties();

            Name = Properties.Name;
        }

        void GetDeviceProperties() {
            using (var prop = new Native<Unmanaged.VkPhysicalDeviceProperties>()) {
                Instance.Commands.getProperties(physicalDevice, prop.Address);
                Properties = new VkPhysicalDeviceProperties(prop.Value);
            }
        }

        void GetQueueProperties() {
            List<VkQueueFamily> queueFamilies = new List<VkQueueFamily>();
            uint count = 0;
            Instance.Commands.getQueueFamilyProperties(physicalDevice, ref count, IntPtr.Zero);
            var props = new NativeArray<Unmanaged.VkQueueFamilyProperties>((int)count);
            Instance.Commands.getQueueFamilyProperties(physicalDevice, ref count, props.Address);

            using (props) {
                for (int i = 0; i < count; i++) {
                    var queueFamily = props[i];
                    var fam = new VkQueueFamily(queueFamily, this, (uint)i);
                    queueFamilies.Add(fam);
                }
            }

            QueueFamilies = queueFamilies.AsReadOnly();
        }

        void GetDeviceFeatures() {
            using (var feat = new Native<Unmanaged.VkPhysicalDeviceFeatures>()) {
                Instance.Commands.getFeatures(physicalDevice, feat.Address);
                features = feat.Value;
            }
        }

        void GetDeviceExtensions() {
            List<VkExtension> availableExtensions = new List<VkExtension>();
            uint count = 0;
            Instance.Commands.getExtensions(physicalDevice, null, ref count, IntPtr.Zero);
            var props = new NativeArray<Unmanaged.VkExtensionProperties>((int)count);
            Instance.Commands.getExtensions(physicalDevice, null, ref count, props.Address);

            using (props) {
                for (int i = 0; i < count; i++) {
                    var ex = props[i];
                    availableExtensions.Add(new VkExtension(ex));
                }
            }

            AvailableExtensions = availableExtensions.AsReadOnly();
        }

        void GetMemoryProperties() {
            using (var prop = new Native<Unmanaged.VkPhysicalDeviceMemoryProperties>()) {
                Instance.Commands.getMemoryProperties(physicalDevice, prop.Address);
                memoryProperties = prop.Value;
            }
        }

        public Unmanaged.VkFormatProperties GetFormatProperties(VkFormat format) {
            using (var prop = new Native<Unmanaged.VkFormatProperties>()) {
                Instance.Commands.getPhysicalDeviceFormatProperties(physicalDevice, format, prop.Address);
                return prop.Value;
            }
        }

        public Unmanaged.VkImageFormatProperties GetImageFormatProperties(VkFormat format, VkImageType type, VkImageTiling tiling, VkImageUsageFlags usage, VkImageCreateFlags flags) {
            using (var prop = new Native<Unmanaged.VkImageFormatProperties>()) {
                Instance.Commands.getPhysicalDeviceImageFormatProperties(physicalDevice, format, type, tiling, usage, flags, prop.Address);
                return prop.Value;
            }
        }

        public IList<Unmanaged.VkSparseImageFormatProperties> GetSparseImageFormatProperties(VkFormat format, VkImageType type, VkSampleCountFlags samples, VkImageUsageFlags usage, VkImageTiling tiling) {
            var result = new List<Unmanaged.VkSparseImageFormatProperties>();

            uint count = 0;
            Instance.Commands.getPhysicalDeviceSparseImageFormatProperties(physicalDevice, format, type, samples, usage, tiling, ref count, IntPtr.Zero);
            var resultNative = new NativeArray<Unmanaged.VkSparseImageFormatProperties>((int)count);
            Instance.Commands.getPhysicalDeviceSparseImageFormatProperties(physicalDevice, format, type, samples, usage, tiling, ref count, resultNative.Address);

            using (resultNative) {
                for (int i = 0; i < count; i++) {
                    var prop = resultNative[i];
                    result.Add(prop);
                }
            }

            return result;
        }
    }

    public class VkQueueFamily {
        public VkQueueFlags Flags { get; private set; }
        public uint QueueCount { get; private set; }
        public uint TimestampValidBits { get; private set; }
        public Unmanaged.VkExtent3D MinImageTransferGranularity { get; private set; }

        VkPhysicalDevice pDevice;
        uint index;

        internal VkQueueFamily(Unmanaged.VkQueueFamilyProperties prop, VkPhysicalDevice pDevice, uint index) {
            this.pDevice = pDevice;
            this.index = index;

            Flags = prop.queueFlags;
            QueueCount = prop.queueCount;
            TimestampValidBits = prop.timestampValidBits;
            MinImageTransferGranularity = prop.minImageTransferGranularity;
        }

        public bool SurfaceSupported(VkSurface surface) {
            bool supported;
            pDevice.Instance.Commands.getPresentationSupport(pDevice.Native, index, surface.Native, out supported);
            return supported;
        }
    }

    public class VkPhysicalDeviceProperties {
        public string Name { get; private set; }
        public VkVersion APIVersion { get; private set; }
        public VkPhysicalDeviceType Type { get; private set; }
        public uint DriverVersion { get; private set; }
        public uint VendorID { get; private set; }
        public uint DeviceID { get; private set; }
        public Unmanaged.VkPhysicalDeviceLimits Limits { get; private set; }
        public Unmanaged.VkPhysicalDeviceSparseProperties SparseProperties { get; private set; }
        public Guid PipelineCache { get; private set; }

        internal VkPhysicalDeviceProperties(Unmanaged.VkPhysicalDeviceProperties prop) {
            unsafe {
                Name = Interop.GetString(&prop.deviceName);
                byte[] uuid = new byte[16];
                for (int i = 0; i < 16; i++) {
                    uuid[i] = (&prop.pipelineCacheUUID)[i];
                }
                PipelineCache = new Guid(uuid);
            }
            APIVersion = prop.apiVersion;
            Type = prop.deviceType;
            DriverVersion = prop.driverVersion;
            VendorID = prop.vendorID;
            DeviceID = prop.deviceID;
            Limits = prop.limits;
            SparseProperties = prop.sparseProperties;
        }
    }
}
