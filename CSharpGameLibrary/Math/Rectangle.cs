using System;
using System.Numerics;

namespace CSGL.Math {
    public struct Rectangle {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Rectangle(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Rectanglei rect) {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }

        public Vector2 Position {
            get {
                return new Vector2(X, Y);
            }
            set {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Size {
            get {
                return new Vector2(Width, Height);
            }
            set {
                Width = value.X;
                Height = value.Y;
            }
        }

        public float Top {
            get {
                return Y;
            }
        }

        public float Bottom {
            get {
                return Y + Height;
            }
        }

        public float Left {
            get {
                return X;
            }
        }

        public float Right {
            get {
                return X + Width;
            }
        }

        public bool Intersects(Rectangle other) {
            return (X < other.X + other.Width) &&
                (X + Width > other.X) &&
                (Y < other.Y + other.Height) &&
                (Y + Height > other.Y);
        }

        public bool Intersects(Rectanglei other) {
            var otherf = new Rectangle(other);
            return Intersects(otherf);
        }
    }
}
