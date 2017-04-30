using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class QueryPoolCreateInfo {
        public VkQueryType queryType;
        public uint queryCount;
        public VkQueryPipelineStatisticFlags pipelineStatistics;
    }

    public class QueryPool : IDisposable, INative<VkQueryPool> {
        bool disposed;

        VkQueryPool queryPool;

        public VkQueryPool Native {
            get {
                return queryPool;
            }
        }

        public Device Device { get; private set; }

        public QueryPool(Device device, QueryPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Device = device;

            CreateQueryPool(info);
        }

        void CreateQueryPool(QueryPoolCreateInfo mInfo) {
            VkQueryPoolCreateInfo info = new VkQueryPoolCreateInfo();
            info.sType = VkStructureType.QueryPoolCreateInfo;
            info.queryType = mInfo.queryType;
            info.queryCount = mInfo.queryCount;
            info.pipelineStatistics = mInfo.pipelineStatistics;

            Device.Commands.createQueryPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out queryPool);
        }

        public VkResult GetResults(uint firstQuery, uint queryCount, byte[] data, ulong stride, VkQueryResultFlags flags) {
            unsafe {
                fixed (byte* ptr = data) {
                    return Device.Commands.getQueryPoolResults(Device.Native, queryPool,
                        firstQuery, queryCount,
                        (ulong)data.Length, (IntPtr)ptr,
                        stride, flags
                    );
                }
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        void Dispose(bool disposing) {
            if (disposed) return;

            Device.Commands.destroyQueryPool(Device.Native, queryPool, Device.Instance.AllocationCallbacks);

            disposed = true;
        }

        ~QueryPool() {
            Dispose(false);
        }
    }
}
