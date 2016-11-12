﻿using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class PhysicalDevice: INative<VkPhysicalDevice> {
        VkPhysicalDevice device;

        public string Name { get; private set; }
        public Instance Instance { get; private set; }
        public PhysicalDeviceProperties Properties { get; private set; }
        public List<QueueFamily> QueueFamilies { get; private set; }
        public List<Extension> AvailableExtensions { get; private set; }

        public VkPhysicalDevice Native {
            get {
                return device;
            }
        }

        VkPhysicalDeviceFeatures features;
        public VkPhysicalDeviceFeatures Features {
            get {
                return features;
            }
        }

        VkPhysicalDeviceMemoryProperties memoryProperties;
        public VkPhysicalDeviceMemoryProperties MemoryProperties {
            get {
                return memoryProperties;
            }
        }

        internal PhysicalDevice(Instance instance, VkPhysicalDevice device) {
            Instance = instance;
            this.device = device;

            GetDeviceProperties();
            GetQueueProperties();
            GetDeviceFeatures();
            GetDeviceExtensions();
            GetMemoryProperties();

            Name = Properties.Name;
        }

        void GetDeviceProperties() {
            using (var prop = new Marshalled<VkPhysicalDeviceProperties>()) {
                Instance.Commands.getProperties(device, prop.Address);
                Properties = new PhysicalDeviceProperties(prop.Value);
            }
        }

        void GetQueueProperties() {
            QueueFamilies = new List<QueueFamily>();
            uint count = 0;
            Instance.Commands.getQueueFamilyProperties(device, ref count, IntPtr.Zero);
            var props = new MarshalledArray<VkQueueFamilyProperties>((int)count);
            Instance.Commands.getQueueFamilyProperties(device, ref count, props.Address);

            for (int i = 0; i < count; i++) {
                var queueFamily = props[i];
                var fam = new QueueFamily(queueFamily, this, (uint)i);
                QueueFamilies.Add(fam);
            }

            props.Dispose();
        }

        void GetDeviceFeatures() {
            using (var feat = new Marshalled<VkPhysicalDeviceFeatures>()) {
                Instance.Commands.getFeatures(device, feat.Address);
                features = feat.Value;
            }
        }

        void GetDeviceExtensions() {
            AvailableExtensions = new List<Extension>();
            uint count = 0;
            Instance.Commands.getExtensions(device, null, ref count, IntPtr.Zero);
            var props = new MarshalledArray<VkExtensionProperties>((int)count);
            Instance.Commands.getExtensions(device, null, ref count, props.Address);

            for (int i = 0; i < count; i++) {
                var ex = props[i];
                AvailableExtensions.Add(new Extension(ex));
            }

            props.Dispose();
        }

        void GetMemoryProperties() {
            using (var prop = new Marshalled<VkPhysicalDeviceMemoryProperties>()) {
                Instance.Commands.getMemoryProperties(device, prop.Address);
                memoryProperties = prop.Value;
            }
        }

        VkFormatProperties GetFormatProperties(VkFormat format) {
            using (var prop = new Marshalled<VkFormatProperties>()) {
                Instance.Commands.getPhysicalDeviceFormatProperties(device, format, prop.Address);
                return prop.Value;
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
                bool supported;
                pDevice.Instance.Commands.getPresentationSupport(pDevice.Native, index, surface.Native, out supported);
                return supported;
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
            unsafe
            {
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
