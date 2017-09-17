using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class VkQueryPoolCreateInfo {
        public VkQueryType queryType;
        public int queryCount;
        public VkQueryPipelineStatisticFlags pipelineStatistics;
    }

    public class VkQueryPool : IDisposable, INative<Unmanaged.VkQueryPool> {
        bool disposed;

        Unmanaged.VkQueryPool queryPool;

        public Unmanaged.VkQueryPool Native {
            get {
                return queryPool;
            }
        }

        public VkDevice Device { get; private set; }
        public VkQueryType QueryType { get; private set; }
        public int QueryCount { get; private set; }
        public VkQueryPipelineStatisticFlags PipelineStatistics { get; private set; }

        public VkQueryPool(VkDevice device, VkQueryPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateQueryPool(info);

            QueryType = info.queryType;
            QueryCount = info.queryCount;
            PipelineStatistics = info.pipelineStatistics;
        }

        void CreateQueryPool(VkQueryPoolCreateInfo mInfo) {
            var info = new Unmanaged.VkQueryPoolCreateInfo();
            info.sType = VkStructureType.QueryPoolCreateInfo;
            info.queryType = mInfo.queryType;
            info.queryCount = (uint)mInfo.queryCount;
            info.pipelineStatistics = mInfo.pipelineStatistics;

            var result = Device.Commands.createQueryPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out queryPool);
            if (result != VkResult.Success) throw new QueryPoolException(result, string.Format("Error creating query pool: {0}", result));
        }

        public VkResult GetResults(int firstQuery, int queryCount, byte[] data, long stride, VkQueryResultFlags flags) {
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                fixed (byte* ptr = data) {
                    var result = Device.Commands.getQueryPoolResults(
                        Device.Native, queryPool,
                        (uint)firstQuery, (uint)queryCount,
                        (IntPtr)data.Length, (IntPtr)ptr,
                        (ulong)stride, flags
                    );

                    if (!(result == VkResult.Success || result == VkResult.NotReady)) throw new QueryPoolException(result, string.Format("Error getting results: {0}", result));
                    return result;
                }
            }
        }

        public VkResult GetResults<T>(int firstQuery, int queryCount, IList<T> data, long stride, VkQueryResultFlags flags) where T : struct {
            if (data == null) throw new ArgumentNullException(nameof(data));

            unsafe {
                int size = (int)Interop.SizeOf(data);
                byte* results = stackalloc byte[size];
                var result = Device.Commands.getQueryPoolResults(
                    Device.Native, queryPool,
                    (uint)firstQuery, (uint)queryCount,
                    (IntPtr)size, (IntPtr)results,
                    (ulong)stride, flags
                );

                Interop.Copy((IntPtr)results, data);

                if (!(result == VkResult.Success || result == VkResult.NotReady)) throw new QueryPoolException(result, string.Format("Error getting results: {0}", result));
                return result;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyQueryPool(Device.Native, queryPool, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~VkQueryPool() {
            Dispose(false);
        }
    }

    public class QueryPoolException : VulkanException {
        public QueryPoolException(VkResult result, string message) : base(result, message) { }
    }
}
