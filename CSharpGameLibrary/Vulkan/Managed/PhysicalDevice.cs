using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class PhysicalDevice {
        VkPhysicalDevice device;

        public string Name { get; private set; }
        public Instance Instance { get; private set; }
        public PhysicalDeviceProperties Properties { get; private set; }
        public List<QueueFamily> QueueFamilies { get; private set; }
        public List<Extension> AvailableExtensions { get; private set; }

        VkPhysicalDeviceFeatures features;
        public VkPhysicalDeviceFeatures Features {
            get {
                return features;
            }
        }

        public int GraphicsIndex { get; private set; } = -1;
        public int ComputeIndex { get; private set; } = -1;
        public int TransferIndex { get; private set; } = -1;
        public int SparseBindingIndex { get; private set; } = -1;

        public VkPhysicalDevice Native {
            get {
                return device;
            }
        }

        vkGetPhysicalDevicePropertiesDelegate GetProperties;
        vkGetPhysicalDeviceQueueFamilyPropertiesDelegate GetQueueFamilyProperties;
        vkGetPhysicalDeviceFeaturesDelegate GetFeatures;
        vkEnumerateDeviceExtensionPropertiesDelegate GetExtensions;

        internal PhysicalDevice(Instance instance, VkPhysicalDevice device) {
            Instance = instance;
            this.device = device;

            Vulkan.Load(ref GetProperties, instance);
            Vulkan.Load(ref GetQueueFamilyProperties, instance);
            Vulkan.Load(ref GetFeatures, instance);
            Vulkan.Load(ref GetExtensions, instance);

            GetDeviceProperties();
            GetQueueProperties();
            GetDeviceFeatures();
            GetDeviceExtensions();

            Name = Properties.Name;
        }

        void GetDeviceProperties() {
            Properties = new PhysicalDeviceProperties(device, GetProperties);
        }

        void GetQueueProperties() {
            QueueFamilies = new List<QueueFamily>();
            unsafe {
                uint count = 0;
                VkQueueFamilyProperties* temp = null;
                GetQueueFamilyProperties(device, ref count, ref *temp);
                VkQueueFamilyProperties* props = stackalloc VkQueueFamilyProperties[(int)count];
                GetQueueFamilyProperties(device, ref count, ref props[0]);

                for (int i = 0; i < count; i++) {
                    var fam = new QueueFamily(props[i]);
                    QueueFamilies.Add(fam);
                    if ((fam.Flags & VkQueueFlags.QueueGraphicsBit) != 0 && GraphicsIndex == -1) GraphicsIndex = i;
                    if ((fam.Flags & VkQueueFlags.QueueComputeBit) != 0 && ComputeIndex == -1) ComputeIndex = i;
                    if ((fam.Flags & VkQueueFlags.QueueTransferBit) != 0 && TransferIndex == -1) TransferIndex = i;
                    if ((fam.Flags & VkQueueFlags.QueueSparseBindingBit) != 0 && SparseBindingIndex == -1) SparseBindingIndex = i;
                }
            }
        }

        void GetDeviceFeatures() {
            unsafe
            {
                GetFeatures(device, ref features);
            }
        }

        void GetDeviceExtensions() {
            AvailableExtensions = new List<Extension>();
            unsafe
            {
                uint count = 0;
                VkExtensionProperties* temp = null;
                GetExtensions(device, null, ref count, ref *temp);
                VkExtensionProperties* props = stackalloc VkExtensionProperties[(int)count];
                GetExtensions(device, null, ref count, ref props[0]);

                for (int i = 0; i < count; i++) {
                    string name = Interop.GetString(props[i].extensionName);
                    uint version = props[i].specVersion;
                    var ex = new Extension(name, version);
                    AvailableExtensions.Add(ex);
                }
            }
        }

        public class PhysicalDeviceProperties {
            public string Name { get; private set; }
            public VkVersion APIVersion { get; private set; }
            public VkPhysicalDeviceType Type { get; private set; }
            public uint DriverVersion { get; private set; }
            public uint VendorID { get; private set; }
            public uint DeviceID { get; private set; }
            public VkPhysicalDeviceLimits Limits { get; private set; }
            public VkPhysicalDeviceSparseProperties SparseProperties { get; private set; }
            public Guid PipelineCache { get; private set; }

            internal PhysicalDeviceProperties(VkPhysicalDevice device, vkGetPhysicalDevicePropertiesDelegate getter) {
                var prop = new VkPhysicalDeviceProperties();
                getter(device, ref prop);
                unsafe
                {
                    Name = Interop.GetString(prop.deviceName);
                    byte[] uuid = new byte[16];
                    for (int i = 0; i < 16; i++) {
                        uuid[i] = prop.pipelineCacheUUID[i];
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

        public class QueueFamily {
            public VkQueueFlags Flags { get; private set; }
            public uint QueueCount { get; private set; }
            public uint TimestampValidBits { get; private set; }
            public VkExtent3D MinImageTransferGranularity { get; private set; }

            internal QueueFamily(VkQueueFamilyProperties prop) {
                Flags = prop.queueFlags;
                QueueCount = prop.queueCount;
                TimestampValidBits = prop.timestampValidBits;
                MinImageTransferGranularity = prop.minImageTransferGranularity;
            }
        }
    }
}
