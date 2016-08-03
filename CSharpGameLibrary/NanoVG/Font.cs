using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using static CSGL.NanoVG.NanoVG_native;

namespace CSGL.NanoVG {
    public class Font {
        static Dictionary<string, Font> map;
        static Dictionary<int, Font> map2;

        public int ID { get; private set; }
        public string Name { get; private set; }

        static Font() {
            map = new Dictionary<string, Font>();
            map2 = new Dictionary<int, Font>();
        }

        public Font(Context ctx, string name, string fileName) {
            using (var fs = File.OpenRead(fileName)) {
                Init(ctx, name, fs);
            }
        }

        public Font(Context ctx, string name, Stream stream) {
            Init(ctx, name, stream);
        }

        void Init(Context ctx, string name, Stream stream) {
            int length = (int)stream.Length;
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, (int)stream.Length);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            ID = nvgCreateFontMem(ctx.ctx, name, ptr, length, 0);
            handle.Free();

            map.Add(name, this);
            map2.Add(ID, this);
        }

        public static Font GetFont(int id) {
            if (map2.ContainsKey(id)) return map2[id];
            else return null;
        }

        public static Font GetFont(string name) {
            if (map.ContainsKey(name)) return map[name];
            else return null;
        }
    }
}
