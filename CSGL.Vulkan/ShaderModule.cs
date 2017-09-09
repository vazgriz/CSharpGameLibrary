using System;
using System.Collections.Generic;
using System.IO;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public class ShaderModuleCreateInfo {
        public byte[] data;
    }

    public class ShaderModule : IDisposable, INative<VkShaderModule> {
        VkShaderModule shaderModule;
        bool disposed = false;

        Device device;

        vkCreateShaderModuleDelegate createShaderModule;
        vkDestroyShaderModuleDelegate destroyShaderModule;

        public VkShaderModule Native {
            get {
                return shaderModule;
            }
        }

        public ShaderModule(Device device, ShaderModuleCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (info.data == null) throw new ArgumentNullException(nameof(info.data));

            this.device = device;

            createShaderModule = device.Commands.createShaderModule;
            destroyShaderModule = device.Commands.destroyShaderModule;

            CreateShader(info);
        }

        void CreateShader(ShaderModuleCreateInfo mInfo) {
            VkShaderModuleCreateInfo info = new VkShaderModuleCreateInfo();
            info.sType = VkStructureType.ShaderModuleCreateInfo;
            info.codeSize = (IntPtr)mInfo.data.Length;

            var dataPinned = new PinnedArray<byte>(mInfo.data);
            info.pCode = dataPinned.Address;

            using (dataPinned) {
                var result = createShaderModule(device.Native, ref info, device.Instance.AllocationCallbacks, out shaderModule);
                if (result != VkResult.Success) throw new ShaderModuleException(result, string.Format("Error creating shader module: {0}"));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            destroyShaderModule(device.Native, shaderModule, device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~ShaderModule() {
            Dispose(false);
        }
    }

    public class ShaderModuleException : VulkanException {
        public ShaderModuleException(VkResult result, string message) : base(result, message) { }
    }
}
