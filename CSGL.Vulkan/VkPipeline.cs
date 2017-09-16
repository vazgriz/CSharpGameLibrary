using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public abstract class VkPipeline : IDisposable, INative<Unmanaged.VkPipeline> {
        protected Unmanaged.VkPipeline pipeline;
        bool disposed = false;

        public VkDevice Device { get; protected set; }

        public Unmanaged.VkPipeline Native {
            get {
                return pipeline;
            }
        }

        public VkPipelineCreateFlags Flags { get; protected set; }
        public VkPipelineLayout Layout { get; protected set; }

        public static IList<VkGraphicsPipeline> CreatePipelines(VkDevice device, IList<GraphicsPipelineCreateInfo> infos, VkPipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new List<VkGraphicsPipeline>(infos.Count);
            var natives = VkGraphicsPipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Count; i++) {
                pipelines.Add(new VkGraphicsPipeline(device, natives[i], infos[i]));
            }

            return pipelines;
        }

        public static IList<VkComputePipeline> CreatePipelines(VkDevice device, IList<ComputePipelineCreateInfo> infos, VkPipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new List<VkComputePipeline>(infos.Count);
            var natives = VkComputePipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Count; i++) {
                pipelines.Add(new VkComputePipeline(device, natives[i], infos[i]));
            }

            return pipelines;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyPipeline(Device.Native, pipeline, Device.Instance.AllocationCallbacks);
            disposed = true;
        }

        ~VkPipeline() {
            Dispose(false);
        }
    }

    public class PipelineException : VulkanException {
        public PipelineException(VkResult result, string message) : base(result, message) { }
    }
}
