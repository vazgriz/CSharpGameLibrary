﻿using System;
using System.Collections.Generic;
using System.IO;

namespace CSGL.Vulkan {
    public class VkShaderModuleCreateInfo {
        public IList<byte> data;
    }

    public class VkShaderModule : IDisposable, INative<Unmanaged.VkShaderModule> {
        Unmanaged.VkShaderModule shaderModule;
        bool disposed = false;

        public VkDevice Device { get; private set; }

        public Unmanaged.VkShaderModule Native {
            get {
                return shaderModule;
            }
        }

        public VkShaderModule(VkDevice device, VkShaderModuleCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateShader(info);
        }

        void CreateShader(VkShaderModuleCreateInfo mInfo) {
            if (mInfo.data == null) throw new ArgumentNullException(nameof(mInfo.data));

            var info = new Unmanaged.VkShaderModuleCreateInfo();
            info.sType = VkStructureType.ShaderModuleCreateInfo;
            info.codeSize = (IntPtr)mInfo.data.Count;

            var dataNative = new NativeArray<byte>(mInfo.data);
            info.pCode = dataNative.Address;

            using (dataNative) {
                var result = Device.Commands.createShaderModule(Device.Native, ref info, Device.Instance.AllocationCallbacks, out shaderModule);
                if (result != VkResult.Success) throw new ShaderModuleException(result, string.Format("Error creating shader module: {0}"));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyShaderModule(Device.Native, shaderModule, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkShaderModule() {
            Dispose(false);
        }
    }

    public class ShaderModuleException : VulkanException {
        public ShaderModuleException(VkResult result, string message) : base(result, message) { }
    }
}
