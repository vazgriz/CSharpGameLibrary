using System;
using System.Collections.Generic;

using CSGL.GLFW.Unmanaged;
using UGLFW = CSGL.GLFW.Unmanaged.GLFW;

namespace CSGL.GLFW {
    public class Monitor : INative<MonitorPtr> {
        static List<Monitor> monitors;
        static Dictionary<MonitorPtr, Monitor> monitorMap;

        public static IList<Monitor> Monitors { get; private set; }
        public static Monitor Primary { get; private set; }

        internal static void Init() {
            UGLFW.SetMonitorCallback(MonitorConnection);
            monitors = new List<Monitor>();
            monitorMap = new Dictionary<MonitorPtr, Monitor>();
            Monitors = monitors.AsReadOnly();
        }
        
        static void GetMonitors() {
            monitors.Clear();
            var monitorsNative = UGLFW.GetMonitors();

            foreach (var n in monitorsNative) {
                if (monitorMap.ContainsKey(n)) {
                    monitors.Add(monitorMap[n]);
                } else {
                    var m = new Monitor(n);
                    monitors.Add(m);
                    monitorMap.Add(n, m);
                }
            }

            Primary = Monitors[0];
        }

        static void MonitorConnection(MonitorPtr monitor, ConnectionStatus status) {
            if (status == ConnectionStatus.Disconnected) {
                monitorMap[monitor].connected = false;
                monitorMap.Remove(monitor);
            }

            GetMonitors();
        }

        MonitorPtr monitor;
        bool connected = true;
        IList<VideoMode> videoModes;

        public MonitorPtr Native {
            get {
                return monitor;
            }
        }

        public bool Status {
            get {
                return connected;
            }
        }

        public IList<VideoMode> VideoModes {
            get {
                if (videoModes == null) {
                    videoModes = new List<VideoMode>(UGLFW.GetVideoModes(monitor)).AsReadOnly();
                }

                return videoModes;
            }
        }

        Monitor(MonitorPtr monitor) {
            this.monitor = monitor;
        }
    }
}
