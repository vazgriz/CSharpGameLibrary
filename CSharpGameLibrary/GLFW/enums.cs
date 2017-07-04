using System;

namespace CSGL.GLFW {
    public enum ErrorCode {
        NotInitialized = 0x00010001,
        NoCurrentContext = 0x00010002,
        InvalidEnum = 0x00010003,
        InvalidValue = 0x00010004,
        OutOfMemory = 0x00010005,
        APIUnavailable = 0x00010006,
        VersionUnavailable = 0x00010007,
        PlatformError = 0x00010008,
        FormatUnavailable = 0x00010009,
        NoWindowContext = 0x0001000A
    }

    public enum WindowAttribute {
        Focused = 0x00020001,
        Iconified = 0x00020002,
        Resizable = 0x00020003,
        Visible = 0x00020004,
        Decorated = 0x00020005,
        AutoIconify = 0x00020006,
        Floating = 0x00020007,
        Maximized = 0x00020008
    }

    public enum WindowHint {
        Focused = 0x00020001,
        Iconified = 0x00020002,
        Resizable = 0x00020003,
        Visible = 0x00020004,
        Decorated = 0x00020005,
        AutoIconify = 0x00020006,
        Floating = 0x00020007,
        Maximized = 0x00020008,
        RedBits = 0x00021001,
        GreenBits = 0x00021002,
        BlueBits = 0x00021003,
        AlphaBits = 0x00021004,
        DepthBits = 0x00021005,
        StencilBits = 0x00021006,
        AccumRedBits= 0x00021007,
        AccumGreenBits = 0x00021008,
        AccumBlueBits = 0x00021009,
        AccumAlphaBits = 0x0002100A,
        AuxBuffers = 0x0002100B,
        Stereo = 0x0002100C,
        Samples = 0x0002100D,
        sRGBCapable = 0x0002100E,
        RefreshRate = 0x0002100F,
        DoubleBuffer = 0x00021010,
        ClientAPI = 0x00022001,
        ContextVersionMajor = 0x00022002,
        ContextVersionMinor = 0x00022003,
        ContextVersionRevision = 0x00022004,
        ContextRobustness = 0x00022005,
        OpenGLForwardCompat = 0x00022006,
        OpenGLDebugContext = 0x00022007,
        OpenGLProfile = 0x00022008,
        ContextReleaseBehavior = 0x00022009,
        ContextNoError = 0x0002200A,
        ContextCreationAPI = 0x0002200B,
    }

    public enum ClientAPI {
        NoAPI = 0,
        OpenGLAPI = 0x00030001,
        OpenGL_ES_API = 0x00030002,
    }

    public enum ContextRobustness {
        NoRobustness = 0,
        NoResetNotification = 0x00031001,
        LoseContextOnReset = 0x00031002,
    }

    public enum OpenGLProfile {
        AnyProfile = 0,
        Core = 0x0032001,
        Compat = 0x00032002
    }

    public enum ReleaseBehavior {
        Any = 0,
        Flush = 0x00035001,
        None = 0x00035002
    }

    public enum ContextCreationAPI {
        NativeContextAPI = 0x00036001,
        EGLContextAPI = 0x00036002,
    }

    public enum ConnectionStatus {
        Connected = 0x00040001,
        Disconnected = 0x00040002
    }

    public enum CursorShape {
        Arrow = 0x00036001,
        IBeam = 0x0036002,
        Crosshair = 0x00036003,
        Hand = 0x00036004,
        HorizontalResize = 0x00036005,
        VerticalRezie = 0x00036006
    }
}

namespace CSGL.Input {
    public enum KeyAction {
        Release,
        Press,
        Repeat,
    }

    public enum KeyCode {
        Unknown = -1,
        Space = 32,
        Apostrophe = 39,
        Comma = 44,
        Minus = 45,
        Period = 46,
        Slash = 47,
        Alpha0 = 48,
        Alpha1 = 49,
        Alpha2 = 50,
        Alpha3 = 51,
        Alpha4 = 52,
        Alpha5 = 53,
        Alpha6 = 54,
        Alpha7 = 55,
        Alpha8 = 56,
        Alpha9 = 57,
        Semicolon = 59,
        Equal = 61,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LeftBracket = 91,
        Backslash = 92,
        RightBracket = 93,
        GraveAccent = 96,
        World1 = 161, 
        World2 = 162,
        Escape = 256,
        Enter = 257,
        Tab = 258,
        Backspace = 259,
        Insert = 260,
        Delete = 261,
        Right = 262,
        Left = 263,
        Down  = 264,
        Up = 265,
        PageUp = 266,
        PageDown = 267,
        Home = 268,
        End = 269,
        CapsLock = 280,
        ScrollLock = 281,
        NumLock = 282,
        PrintScreen = 283,
        Pause = 284,
        F1 = 290,
        F2 = 291,
        F3 = 292,
        F4 = 293,
        F5 = 294,
        F6 = 295,
        F7 = 296,
        F8 = 297,
        F9 = 298,
        F10 = 299,
        F11 = 300,
        F12 = 301,
        F13 = 302,
        F14 = 303,
        F15 = 304,
        F16 = 305,
        F17 = 306,
        F18 = 307,
        F19 = 308,
        F20 = 309,
        F21 = 310,
        F22 = 311,
        F23 = 312,
        F24 = 313,
        F25 = 314,
        KeyPad0 = 320,
        KeyPad1 = 321,
        KeyPad2 = 322,
        KeyPad3 = 323,
        KeyPad4 = 324,
        KeyPad5 = 325,
        KeyPad6 = 326,
        KeyPad7 = 327,
        KeyPad8 = 328,
        KeyPad9 = 329,
        KeyPadDecimal = 330,
        KeyPadDivide = 331,
        KeyPadMultiply = 332,
        KeyPadSubtract = 333,
        KeyPadAdd  = 334,
        KeyPadEnter = 335,
        KeyPadEqual = 336,
        LeftShift = 340,
        LeftControl = 341,
        LeftAlt = 342,
        LeftSuper = 343,
        RightShift = 344,
        RightControl = 345,
        RightAlt = 346,
        RightSuper = 347,
        Menu = 348,
        _LastKey = Menu,
    }

    public enum KeyMod {
        Shift = 0x1,
        Control = 0x2,
        Alt = 0x4,
        Super = 0x8
    }

    public enum MouseButton {
        Button1      = 0,
        Button2      = 1,
        Button3      = 2,
        Button4      = 3,
        Button5      = 4,
        Button6      = 5,
        Button7      = 6,
        Button8      = 7,
        Left   = Button1,
        Right  = Button2,
        Middle = Button3,
        _ButtonLast   = Button8,
    }

    public enum CursorMode {
        Normal = 0x00034001,
        Hidden = 0x00034002,
        Disabled = 0x00034003
    }

    public enum InputMode {
        Cursor = 0x33001,
        StickyKeys = 0x33002
    }
}