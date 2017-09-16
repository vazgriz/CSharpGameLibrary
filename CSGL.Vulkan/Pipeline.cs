using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public abstract class Pipeline : IDisposable, INative<Unmanaged.VkPipeline> {
        protected Unmanaged.VkPipeline pipeline;
        bool disposed = false;

        public Device Device { get; protected set; }

        public Unmanaged.VkPipeline Native {
            get {
                return pipeline;
            }
        }

        public VkPipelineCreateFlags Flags { get; protected set; }
        public PipelineLayout Layout { get; protected set; }

        public static IList<GraphicsPipeline> CreatePipelines(Device device, IList<GraphicsPipelineCreateInfo> infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new List<GraphicsPipeline>(infos.Count);
            var natives = GraphicsPipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Count; i++) {
                pipelines.Add(new GraphicsPipeline(device, natives[i], infos[i]));
            }

            return pipelines;
        }

        public static IList<ComputePipeline> CreatePipelines(Device device, IList<ComputePipelineCreateInfo> infos, PipelineCache cache) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (infos == null) throw new ArgumentNullException(nameof(infos));

            var nativeCache = Unmanaged.VkPipelineCache.Null;
            if (cache != null) {
                nativeCache = cache.Native;
            }

            var pipelines = new List<ComputePipeline>(infos.Count);
            var natives = ComputePipeline.CreatePipelinesInternal(device, infos, nativeCache);

            for (int i = 0; i < infos.Count; i++) {
                pipelines.Add(new ComputePipeline(device, natives[i], infos[i]));
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
