using System;
using System.Collections.Generic;

namespace CSGL.Graphics {
    public class Bitmap<T> where T : struct {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public T[] Data { get; private set; }

        public Bitmap(int width, int height) {
            Width = width;
            Height = height;

            Data = new T[width * height];
        }

        public int GetIndex(int x, int y) {
            return x + y * Height;
        }

        public T this[int x, int y] {
            get {
                return Data[GetIndex(x, y)];
            }
            set {
                Data[GetIndex(x, y)] = value;
            }
        }
    }
}
