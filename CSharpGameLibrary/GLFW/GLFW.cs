using System;
using System.Collections.Generic;

using CSGL.Input;
using UGLFW = CSGL.GLFW.Unmanaged.GLFW;

namespace CSGL.GLFW {
    public static class GLFW {
        [ThreadStatic]
        static Exception exception;
        internal static bool Initialized { get; private set; }

        public static string Version { get; private set; }
        public static int VersionMajor { get; private set; }
        public static int VersionMinor { get; private set; }
        public static int VersionPatch { get; private set; }

        public static IList<Monitor> Monitors {
            get {
                return Monitor.Monitors;
            }
        }

        public static void CheckError() {      //only way to convert GLFW error to managed exception
            if (exception != null) {    //the error callback creates an exception without throwing it
                var ex = exception;     //and stores it in a thread local variable
                exception = null;       //if that variable is not null when this method is called, an exception is thrown
                throw ex;
            }
        }

        static void InternalError(ErrorCode code, string desc) {
            exception = new GLFWException(code, desc);
        }

        public static bool Init() {
            UGLFW.SetErrorCallback(InternalError);
            bool result = UGLFW.Init();
            CheckError();
            Initialized = true;

            Version = UGLFW.GetVersion();
            CheckError();

            int major, minor, revision;
            UGLFW.GetVersion(out major, out minor, out revision);
            CheckError();
            VersionMajor = major;
            VersionMinor = minor;
            VersionPatch = revision;

            Monitor.Init();
            CheckError();

            return result;
        }

        public static void Terminate() {
            UGLFW.Terminate();
            Initialized = false;
        }

        public static void SwapInterval(int interval) {
            UGLFW.SwapInterval(interval);
            CheckError();
        }

        public static void WindowHint(WindowHint hint, int value) {
            UGLFW.WindowHint(hint, value);
            CheckError();
        }

        public static void DefaultWindowHints() {
            UGLFW.DefaultWindowHints();
            CheckError();
        }

        public static void PollEvents() {
            UGLFW.PollEvents();
            CheckError();
        }
    }
}
