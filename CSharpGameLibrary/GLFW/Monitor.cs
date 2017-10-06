using System;
using System.Collections.Generic;

using CSGL.GLFW.Unmanaged;

namespace CSGL.GLFW {
    public class Monitor : INative<MonitorPtr> {
        static List<Monitor> monitors;
        static Dictionary<MonitorPtr, Monitor> monitorMap;

        public static IList<Monitor> Monitors { get; private set; }
        public static Monitor Primary { get; private set; }

        internal static void Init() {
            GLFW.SetMonitorCallback(MonitorConnection);
            monitors = new List<Monitor>();
            monitorMap = new Dictionary<MonitorPtr, Monitor>();
            Monitors = monitors.AsReadOnly();
            GetMonitors();
        }

        internal Monitor GetMonitor(MonitorPtr ptr) {
            if (ptr == MonitorPtr.Null) return null;
            return monitorMap[ptr];
        }
        
        static void GetMonitors() {
            monitors.Clear();
            var monitorsNative = GLFW.GetMonitors();

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
                monitorMap[monitor].Connected = false;
                monitorMap.Remove(monitor);
            }

            GetMonitors();
        }

        MonitorPtr monitor;
        GammaRamp gammaRamp;

        public MonitorPtr Native {
            get {
                return monitor;
            }
        }

        public string Name { get; private set; }
        public bool Connected { get; private set; }
        public IList<VideoMode> VideoModes { get; private set; }
        public int PhysicalWidth { get; private set; }
        public int PhysicalHeight { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public GammaRamp GammaRamp {
            get {
                return gammaRamp;
            }
            set {
                GLFW.SetGammaRamp(monitor, value);
                gammaRamp = value;
            }
        }

        Monitor(MonitorPtr monitor) {
            this.monitor = monitor;

            Name = GLFW.GetMonitorName(monitor);

            VideoModes = new List<VideoMode>(GLFW.GetVideoModes(monitor)).AsReadOnly();

            int x, y, w, h;
            GLFW.GetMonitorPhysicalSize(monitor, out w, out h);
            GLFW.GetMonitorPos(monitor, out x, out y);

            PhysicalWidth = w;
            PhysicalHeight = h;
            X = x;
            Y = y;

            gammaRamp = GLFW.GetGammaRamp(monitor);

            Connected = true;
        }

        public void SetGamma(float gamma) {
            GLFW.SetGamma(monitor, gamma);
            gammaRamp = GLFW.GetGammaRamp(monitor);
        }
    }
}
