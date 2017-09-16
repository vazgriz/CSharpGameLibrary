using System;
using System.Collections.Generic;

namespace CSGL.Vulkan {
    internal static class ExtensionMethods {
        internal static IList<T> CloneReadOnly<T>(this IList<T> list) {
            if (list == null) {
                return new List<T>().AsReadOnly();
            } else {
                return new List<T>(list).AsReadOnly();
            }
        }
    }
}
