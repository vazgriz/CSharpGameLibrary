﻿using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    public class QueryPoolCreateInfo {
        public VkQueryType queryType;
        public uint queryCount;
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
        public uint QueryCount { get; private set; }
        public VkQueryPipelineStatisticFlags PipelineStatistics { get; private set; }

        public VkQueryPool(VkDevice device, QueryPoolCreateInfo info) {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (info == null) throw new ArgumentNullException(nameof(info));

            Device = device;

            CreateQueryPool(info);

            QueryType = info.queryType;
            QueryCount = info.queryCount;
            PipelineStatistics = info.pipelineStatistics;
        }

        void CreateQueryPool(QueryPoolCreateInfo mInfo) {
            var info = new Unmanaged.VkQueryPoolCreateInfo();
            info.sType = VkStructureType.QueryPoolCreateInfo;
            info.queryType = mInfo.queryType;
            info.queryCount = mInfo.queryCount;
            info.pipelineStatistics = mInfo.pipelineStatistics;

            var result = Device.Commands.createQueryPool(Device.Native, ref info, Device.Instance.AllocationCallbacks, out queryPool);
            if (result != VkResult.Success) throw new QueryPoolException(result, string.Format("Error creating query pool: {0}", result));
        }

        public VkResult GetResults(uint firstQuery, uint queryCount, byte[] data, ulong stride, VkQueryResultFlags flags) {
            unsafe {
                fixed (byte* ptr = data) {
                    var result = Device.Commands.getQueryPoolResults(
                        Device.Native, queryPool,
                        firstQuery, queryCount,
                        (IntPtr)data.Length, (IntPtr)ptr,
                        stride, flags
                    );

                    if (!(result == VkResult.Success || result == VkResult.NotReady)) throw new QueryPoolException(result, string.Format("Error getting results: {0}", result));
                    return result;
                }
            }
        }

        public VkResult GetResults<T>(uint firstQuery, uint queryCount, IList<T> data, ulong stride, VkQueryResultFlags flags) where T : struct {
            unsafe {
                int size = (int)Interop.SizeOf(data);
                byte* results = stackalloc byte[size];
                var result = Device.Commands.getQueryPoolResults(
                    Device.Native, queryPool,
                    firstQuery, queryCount,
                    (IntPtr)size, (IntPtr)results,
                    stride, flags
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