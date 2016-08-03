using System;

namespace CSGL.GLFW {
    public class GLFWException : Exception {
        public GLFWException(ErrorCode code, string message) : base(string.Format("GLFW Error {0}: {1}", code, message)) { }
    }
}
