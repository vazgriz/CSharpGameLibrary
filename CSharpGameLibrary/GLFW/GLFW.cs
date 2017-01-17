using System;
using System.Collections.Generic;

using CSGL.Input;
using glfw = CSGL.GLFW.Unmanaged.GLFW;

namespace CSGL.GLFW {
    public static class GLFW {
        public static string Version { get; private set; }
        public static int VersionMajor { get; private set; }
        public static int VersionMinor { get; private set; }
        public static int VersionPatch { get; private set; }

        public static IList<Monitor> Monitors {
            get {
                return Monitor.Monitors;
            }
        }

        public static bool Init() {
            bool result = glfw.Init();

            Version = glfw.GetVersion();

            int major, minor, revision;
            glfw.GetVersion(out major, out minor, out revision);
            VersionMajor = major;
            VersionMinor = minor;
            VersionPatch = revision;

            Monitor.Init();

            return result;
        }

        public static void Terminate() {
            glfw.Terminate();
        }
    }
}
