using System;
using System.Collections.Generic;

namespace CSGL.Vulkan.Managed {
    public class InstanceCreateInfo {
        public ApplicationInfo ApplicationInfo { get; set; }
        public List<string> Extensions { get; set; }
        public List<string> Layers { get; set; }

        public InstanceCreateInfo(ApplicationInfo appInfo, List<string> extensions, List<string> layers) {
            ApplicationInfo = appInfo;
            Extensions = extensions;
            Layers = layers;
        }
    }
}
