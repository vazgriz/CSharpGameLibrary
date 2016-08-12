using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        public VkPhysicalDevice Native {
            get {
                return device;
            }
        }

        vkGetPhysicalDevicePropertiesDelegate getProperties;
        vkGetPhysicalDeviceQueueFamilyPropertiesDelegate getQueueFamilyProperties;
        vkGetPhysicalDeviceFeaturesDelegate getFeatures;
        vkEnumerateDeviceExtensionPropertiesDelegate getExtensions;
        vkGetPhysicalDeviceSurfaceSupportKHRDelegate getPresentationSupport;

        internal PhysicalDevice(Instance instance, VkPhysicalDevice device) {
            Instance = instance;
            this.device = device;

            getProperties = instance.Commands.getProperties;
            getQueueFamilyProperties = instance.Commands.getQueueFamilyProperties;
            getFeatures = instance.Commands.getFeatures;
            getExtensions = instance.Commands.getExtensions;
            getPresentationSupport = instance.Commands.getPresentationSupport;

            GetDeviceProperties();
            GetQueueProperties();
            //GetDeviceFeatures();
            GetDeviceExtensions();

            //Name = Properties.Name;
        }

        void GetDeviceProperties() {
            unsafe
            {
                VkPhysicalDeviceProperties prop = new VkPhysicalDeviceProperties();
                getProperties(device, ref prop);
                Properties = new PhysicalDeviceProperties(prop);
            }
        }

        void GetQueueProperties() {
            QueueFamilies = new List<QueueFamily>();
            unsafe {
                uint count = 0;
                getQueueFamilyProperties(device, &count, null);
                VkQueueFamilyProperties* props = stackalloc VkQueueFamilyProperties[(int)count];
                getQueueFamilyProperties(device, &count, props);

                for (uint i = 0; i < count; i++) {
                    var fam = new QueueFamily(props[i], this, i);
                    QueueFamilies.Add(fam);
                }
            }
        }

        void GetDeviceFeatures() {
            unsafe
            {
                fixed (VkPhysicalDeviceFeatures* temp = &features) {
                    getFeatures(device, ref features);
                }
            }
        }

        void GetDeviceExtensions() {
            AvailableExtensions = new List<Extension>();
            unsafe
            {
                uint count = 0;
                getExtensions(device, null, &count, null);
                var props = stackalloc VkExtensionProperties[(int)count];
                getExtensions(device, null, &count, props);

                for (int i = 0; i < count; i++) {
                    string name = Interop.GetString(props[i].extensionName);
                    uint version = props[i].specVersion;
                    var ex = new Extension(name, version);
                    AvailableExtensions.Add(ex);
                }
            }
        }

        public class QueueFamily {
            public VkQueueFlags Flags { get; private set; }
            public uint QueueCount { get; private set; }
            public uint TimestampValidBits { get; private set; }
            public VkExtent3D MinImageTransferGranularity { get; private set; }

            PhysicalDevice pDevice;
            uint index;

            internal QueueFamily(VkQueueFamilyProperties prop, PhysicalDevice pDevice, uint index) {
                this.pDevice = pDevice;
                this.index = index;

                Flags = prop.queueFlags;
                QueueCount = prop.queueCount;
                TimestampValidBits = prop.timestampValidBits;
                MinImageTransferGranularity = prop.minImageTransferGranularity;
            }

            public bool SurfaceSupported(Surface surface) {
                uint support = 0;
                unsafe
                {
                    pDevice.getPresentationSupport(pDevice.Native, index, surface.Native, &support);
                }
                return support != 0;
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

        internal PhysicalDeviceProperties(VkPhysicalDeviceProperties prop) {
            Name = Interop.GetString(prop.deviceName);
            PipelineCache = new Guid(prop.pipelineCacheUUID);
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
