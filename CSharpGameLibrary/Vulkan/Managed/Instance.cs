using System;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class Instance {
        VkInstance instance;

        public Instance(InstanceCreateInfo info) {
            unsafe
            {
                VkInstanceCreateInfo iInfo;
                VkApplicationInfo appInfo;
                iInfo.sType = VkStructureType.StructureTypeInstanceCreateInfo;
                appInfo.sType = VkStructureType.StructureTypeApplicationInfo;
                iInfo.pApplicationInfo = &appInfo;
            }
        }
    }
}
