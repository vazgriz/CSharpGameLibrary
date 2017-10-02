using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public struct VkMemoryType {
        public VkMemoryPropertyFlags propertyFlags;
        public int heapIndex;

        internal VkMemoryType(Unmanaged.VkMemoryType other) {
            propertyFlags = other.propertyFlags;
            heapIndex = (int)other.heapIndex;
        }
    }
    
    public struct VkMemoryHeap {
        public long size;
        public VkMemoryHeapFlags flags;

        internal VkMemoryHeap(Unmanaged.VkMemoryHeap other) {
            size = (long)other.size;
            flags = other.flags;
        }
    }

    public class VkPhysicalDeviceMemoryProperties {
        public IList<VkMemoryType> MemoryTypes { get; private set; }
        public IList<VkMemoryHeap> MemoryHeaps { get; private set; }

        internal VkPhysicalDeviceMemoryProperties(Unmanaged.VkPhysicalDeviceMemoryProperties other) {
            var types = new List<VkMemoryType>((int)other.memoryTypeCount);
            for (int i = 0; i < (int)other.memoryTypeCount; i++) {
                types.Add(new VkMemoryType(other.GetMemoryTypes(i)));
            }
            MemoryTypes = types.AsReadOnly();

            var heaps = new List<VkMemoryHeap>((int)other.memoryHeapCount);
            for (int i = 0; i < (int)other.memoryHeapCount; i++) {
                heaps.Add(new VkMemoryHeap(other.GetMemoryHeaps(i)));
            }

            MemoryHeaps = heaps.AsReadOnly();
        }
    }
}
