using System;

namespace CSGL {
    public interface INative<T> where T : struct {
        T Native { get; }
    }
}
