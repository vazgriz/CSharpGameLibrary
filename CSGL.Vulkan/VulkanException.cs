using System;

namespace CSGL.Vulkan {
    class VulkanException : Exception {
        public VkResult Result { get; private set; }

        public VulkanException(string message) : base(message) { }

        public VulkanException(VkResult result, string message) : base(message) {
            Result = result;
        }
    }
}
