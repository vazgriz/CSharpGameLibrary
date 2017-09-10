using System;
using System.Collections.Generic;

using CSGL.Vulkan.Unmanaged;

namespace CSGL.Vulkan {
    public abstract class Pipeline : IDisposable, INative<VkPipeline> {
        protected VkPipeline pipeline;
        bool disposed = false;

        public Device Device { get; protected set; }

        public VkPipeline Native {
            get {
                return pipeline;
            }
        }

        public VkPipelineCreateFlags Flags { get; protected set; }
        public PipelineLayout Layout { get; protected set; }

        public static GraphicsPipeline[] CreatePipelines(Device device, GraphicsPipelineCreateInfo[] infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new GraphicsPipeline[infos.Length];
            var natives = GraphicsPipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Length; i++) {
                pipelines[i] = new GraphicsPipeline(device, natives[i], infos[i]);
            }

            return pipelines;
        }

        public static ComputePipeline[] CreatePipelines(Device device, ComputePipelineCreateInfo[] infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            VkPipelineCache nativeCache = VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new ComputePipeline[infos.Length];
            var natives = ComputePipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Length; i++) {
                pipelines[i] = new ComputePipeline(device, natives[i], infos[i]);
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

        ~Pipeline() {
            Dispose(false);
        }
    }

    public class PipelineException : VulkanException {
        public PipelineException(VkResult result, string message) : base(result, message) { }
    }
}
