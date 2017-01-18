using System;
using System.Collections.Generic;

using CSGL.Input;
using UGLFW = CSGL.GLFW.Unmanaged.GLFW;

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
            bool result = UGLFW.Init();

            Version = UGLFW.GetVersion();

            int major, minor, revision;
            UGLFW.GetVersion(out major, out minor, out revision);
            VersionMajor = major;
            VersionMinor = minor;
            VersionPatch = revision;

            Monitor.Init();

            return result;
        }

        public static void Terminate() {
            UGLFW.Terminate();
        }

        public static void SwapInterval(int interval) {
            UGLFW.SwapInterval(interval);
        }

        public static void WindowHint(WindowHint hint, int value) {
            UGLFW.WindowHint(hint, value);
        }

        public static void DefaultWindowHints() {
            UGLFW.DefaultWindowHints();
        }

        public static void PollEvents() {
            UGLFW.PollEvents();
        }
    }
}
