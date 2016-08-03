using System;

namespace CSGL.NanoVG {
    public enum CreateFlags {
        Antialias = 1 << 0,
        StencilStrokes = 1 << 1,
        Debug = 1 << 2
    }

    public enum LineCap {
        Butt = 0,
        Round = 1,
        Square = 2
    }

    public enum LineJoin {
        Round = 1,
        Bevel = 3,
        Miter = 4
    }

    public enum Align {
        // Horizontal align
        Left = 1 << 0,    // Default, align text horizontally to left.
        Center = 1 << 1,  // Align text horizontally to center.
        Right = 1 << 2,   // Align text horizontally to right.
                                    // Vertical align
        Top = 1 << 3, // Align text vertically to top.
        Middle = 1 << 4,  // Align text vertically to middle.
        Bottom = 1 << 5,  // Align text vertically to bottom. 
        Baseline = 1 << 6, // Default, align text vertically to baseline. 
    }

    public enum ImageFlags {
        None = 0,
        GenerateMipmaps = 1 << 0,     // Generate mipmaps during creation of the image.
        RepeatX = 1 << 1,     // Repeat image in X direction.
        RepeatY = 1 << 2,     // Repeat image in Y direction.
        FlipY = 1 << 3,       // Flips (inverses) image in Y direction when rendered.
        PreMultiplied = 1 << 4,       // Image data has premultiplied alpha.
    }
}
