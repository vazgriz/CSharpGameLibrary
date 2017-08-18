using System;
using System.Collections.Generic;
using System.IO;

using CSGL.Vulkan1.Unmanaged;

namespace CSGL.Vulkan1 {
    public class ShaderModuleCreateInfo {
        public byte[] Data { get; set; }

        public ShaderModuleCreateInfo(byte[] data) {
            Data = data;
        }

        public ShaderModuleCreateInfo(Stream stream) {
            int length = (int)(stream.Length - stream.Position);
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
            Data = data;
        }
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
            if (info.Data == null) throw new ArgumentNullException(nameof(info.Data));

            this.device = device;

            createShaderModule = device.Commands.createShaderModule;
            destroyShaderModule = device.Commands.destroyShaderModule;

            CreateShader(info);
        }

        void CreateShader(ShaderModuleCreateInfo mInfo) {
            VkShaderModuleCreateInfo info = new VkShaderModuleCreateInfo();
            info.sType = VkStructureType.ShaderModuleCreateInfo;
            info.codeSize = (IntPtr)mInfo.Data.LongLength;

            var dataPinned = new PinnedArray<byte>(mInfo.Data);
            info.pCode = dataPinned.Address;

            using (dataPinned) {
                var result = createShaderModule(device.Native, ref info, device.Instance.AllocationCallbacks, out shaderModule);
                if (result != VkResult.Success) throw new ShaderModuleException(string.Format("Error creating shader module: {0}"));
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

    public class ShaderModuleException : Exception {
        public ShaderModuleException(string message) : base(message) { }
    }
}
