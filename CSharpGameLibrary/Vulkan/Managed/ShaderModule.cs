using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan.Managed {
    public class ShaderModuleCreateInfo {
        public byte[] Data { get; set; }

        public ShaderModuleCreateInfo(byte[] data) {
            Data = data;
        }

        public ShaderModuleCreateInfo(Stream stream) {
            int length = (int)(stream.Length - stream.Position);
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
        }
    }

    public class ShaderModule : IDisposable {
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
            info.sType = VkStructureType.StructureTypeShaderModuleCreateInfo;
            info.codeSize = (ulong)mInfo.Data.LongLength;

            GCHandle handle = GCHandle.Alloc(mInfo.Data, GCHandleType.Pinned);
                
            info.pCode = handle.AddrOfPinnedObject();

            IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkShaderModuleCreateInfo>());
            Marshal.StructureToPtr(info, infoPtr, false);

            IntPtr shaderModulePtr = Marshal.AllocHGlobal(Marshal.SizeOf<VkShaderModule>());

            try {
                var result = createShaderModule(device.Native, infoPtr, device.Instance.AllocationCallbacks, shaderModulePtr);
                if (result != VkResult.Success) throw new ShaderModuleException(string.Format("Error creating shader module: {0}"));

                shaderModule = Marshal.PtrToStructure<VkShaderModule>(shaderModulePtr);
            }
            finally {
                Marshal.DestroyStructure<VkShaderModuleCreateInfo>(infoPtr);

                Marshal.FreeHGlobal(infoPtr);
                Marshal.FreeHGlobal(shaderModulePtr);

                handle.Free();
            }
        }

        public void Dispose() {
            if (disposed) return;

            destroyShaderModule(device.Native, shaderModule, device.Instance.AllocationCallbacks);

            disposed = true;
        }
    }

    public class ShaderModuleException : Exception {
        public ShaderModuleException(string message) : base(message) { }
    }
}
