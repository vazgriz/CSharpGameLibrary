using System;
using System.Collections.Generic;
using System.IO;

namespace CSGL.Vulkan {
    public class ShaderModuleCreateInfo {
        public IList<byte> data;
    }

    public class ShaderModule : IDisposable, INative<Unmanaged.VkShaderModule> {
        Unmanaged.VkShaderModule shaderModule;
        bool disposed = false;

        Device device;

        Unmanaged.vkCreateShaderModuleDelegate createShaderModule;
        Unmanaged.vkDestroyShaderModuleDelegate destroyShaderModule;

        public Unmanaged.VkShaderModule Native {
            get {
                return shaderModule;
            }
        }

        public ShaderModule(Device device, ShaderModuleCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            this.device = device;

            createShaderModule = device.Commands.createShaderModule;
            destroyShaderModule = device.Commands.destroyShaderModule;

            CreateShader(info);
        }

        void CreateShader(ShaderModuleCreateInfo mInfo) {
            if (mInfo.data == null) throw new ArgumentNullException(nameof(mInfo.data));

            var info = new Unmanaged.VkShaderModuleCreateInfo();
            info.sType = VkStructureType.ShaderModuleCreateInfo;
            info.codeSize = (IntPtr)mInfo.data.Count;

            var dataPinned = new NativeArray<byte>(mInfo.data);
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
