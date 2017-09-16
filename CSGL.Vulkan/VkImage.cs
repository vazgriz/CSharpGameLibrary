using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class ImageCreateInfo {
        public VkImageCreateFlags flags;
        public VkImageType imageType;
        public VkFormat format;
        public Unmanaged.VkExtent3D extent;
        public uint mipLevels;
        public uint arrayLayers;
        public VkSampleCountFlags samples;
        public VkImageTiling tiling;
        public VkImageUsageFlags usage;
        public VkSharingMode sharingMode;
        public IList<uint> queueFamilyIndices;
        public VkImageLayout initialLayout;
    }

    public class VkImage : IDisposable, INative<Unmanaged.VkImage> {
        Unmanaged.VkImage image;
        bool disposed = false;

        Unmanaged.VkMemoryRequirements requirements;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkImage Native {
            get {
                return image;
            }
        }

        public Unmanaged.VkMemoryRequirements Requirements {
            get {
                return requirements;
            }
        }

        public ulong Size {
            get {
                return requirements.size;
            }
        }

        public IList<Unmanaged.VkSparseImageMemoryRequirements> SparseRequirements { get; private set; }

        public VkImageCreateFlags Flags { get; private set; }
        public VkImageType ImageType { get; private set; }
        public VkFormat Format { get; private set; }
        public Unmanaged.VkExtent3D Extent { get; private set; }
        public uint MipLevels { get; private set; }
        public uint ArrayLayers { get; private set; }
        public VkSampleCountFlags Samples { get; private set; }
        public VkImageTiling Tiling { get; private set; }
        public VkImageUsageFlags Usage { get; private set; }
        public VkSharingMode SharingMode { get; private set; }
        public IList<uint> QueueFamilyIndices { get; private set; }

        public ulong Offset { get; private set; }
        public VkDeviceMemory Memory { get; private set; }

        internal VkImage(VkDevice device, Unmanaged.VkImage image, VkFormat format) { //for images that are implicitly created, eg a swapchains's images
            Device = device;
            this.image = image;
            Format = format;
        }

        public VkImage(VkDevice device, ImageCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateImage(info);

            Device.Commands.getImageMemoryRequirements(Device.Native, image, out requirements);

            Flags = info.flags;
            ImageType = info.imageType;
            Format = info.format;
            Extent = info.extent;
            MipLevels = info.mipLevels;
            ArrayLayers = info.arrayLayers;
            Samples = info.samples;
            Tiling = info.tiling;
            Usage = info.usage;
            SharingMode = info.sharingMode;
            QueueFamilyIndices = info.queueFamilyIndices.CloneReadOnly();
        }

        void CreateImage(ImageCreateInfo mInfo) {
            unsafe {
                int indicesCount = 0;
                if (mInfo.queueFamilyIndices != null) indicesCount = mInfo.queueFamilyIndices.Count;

                var info = new Unmanaged.VkImageCreateInfo();
                info.sType = VkStructureType.ImageCreateInfo;
                info.flags = mInfo.flags;
                info.imageType = mInfo.imageType;
                info.format = mInfo.format;
                info.extent = mInfo.extent;
                info.mipLevels = mInfo.mipLevels;
                info.arrayLayers = mInfo.arrayLayers;
                info.samples = mInfo.samples;
                info.tiling = mInfo.tiling;
                info.usage = mInfo.usage;
                info.sharingMode = mInfo.sharingMode;

                var queueFamilyIndicesNative = stackalloc uint[indicesCount];
                if (mInfo.queueFamilyIndices != null) Interop.Copy(mInfo.queueFamilyIndices, (IntPtr)queueFamilyIndicesNative);
                
                info.queueFamilyIndexCount = (uint)indicesCount;
                info.pQueueFamilyIndices = (IntPtr)queueFamilyIndicesNative;

                info.initialLayout = mInfo.initialLayout;
                
                var result = Device.Commands.createImage(Device.Native, ref info, Device.Instance.AllocationCallbacks, out image);
                if (result != VkResult.Success) throw new ImageException(result, string.Format("Error creating image: {0}", result));
            }
        }

        void GetSparseRequirements() {
            var sparseRequirements = new List<Unmanaged.VkSparseImageMemoryRequirements>();

            uint count = 0;
            Device.Commands.getImageSparseRequirements(Device.Native, image, ref count, IntPtr.Zero);
            var sparseRequirementsNative = new MarshalledArray<Unmanaged.VkSparseImageMemoryRequirements>((int)count);
            Device.Commands.getImageSparseRequirements(Device.Native, image, ref count, sparseRequirementsNative.Address);

            using (sparseRequirementsNative) {
                for (int i = 0; i < count; i++) {
                    var requirement = sparseRequirementsNative[i];
                    sparseRequirements.Add(requirement);
                }
            }

            SparseRequirements = sparseRequirements.AsReadOnly();
        }

        public void Bind(VkDeviceMemory memory, ulong offset) {
            Device.Commands.bindImageMemory(Device.Native, image, memory.Native, offset);
            Memory = memory;
            Offset = offset;
        }

        public Unmanaged.VkSubresourceLayout GetSubresourceLayout(Unmanaged.VkImageSubresource subresource) {
            var result = new Unmanaged.VkSubresourceLayout();

            Device.Commands.getSubresourceLayout(Device.Native, image, ref subresource, out result);

            return result;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyImage(Device.Native, image, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkImage() {
            Dispose(false);
        }
    }

    public class ImageException : VulkanException {
        public ImageException(VkResult result, string message) : base(result, message) { }
    }
}
