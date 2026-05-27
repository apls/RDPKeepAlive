using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal static partial class NativeMethods
    {
        #region Delegates

        /// <summary>
        ///     Delegate for the EnumWindows callback.
        /// </summary>
        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        #endregion Delegates

        #region Methods

        /// <summary>
        ///     Enumerates all top-level windows on the screen.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        /// <summary>
        ///     Finds a window whose class name and window name match.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindowExW", SetLastError = true)]
        internal static extern IntPtr FindWindowExW(IntPtr hwndParent, IntPtr hwndChildAfter, [MarshalAs(UnmanagedType.LPWStr)] string lpszClass, [MarshalAs(UnmanagedType.LPWStr)] string lpszWindow);

        /// <summary>
        ///     Retrieves the name of the class to which the specified window belongs.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetClassNameW", SetLastError = true)]
        internal static extern int GetClassName(IntPtr hWnd, [Out] char[] lpClassName, int nMaxCount);

        /// <summary>
        ///     Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        ///     Retrieves a handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        /// <summary>
        ///     Retrieves the dimensions of the specified screen.
        /// </summary>
        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(SystemMetric smIndex);

        /// <summary>
        ///     Retrieves a handle to a window with the specified relationship.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        /// <summary>
        ///     Copies the text of the specified window's title bar into a buffer.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "GetWindowTextW", SetLastError = true)]
        internal static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpWindowText, int nMaxCount);

        /// <summary>
        ///     Retrieves the identifier of the thread that created the specified window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        ///     Synthesizes input events such as keystrokes, mouse movements, and button clicks.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        /// <summary>
        ///     Sets the specified window as the foreground window.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        ///     Changes the size, position, and Z order of a window.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", ExactSpelling = true)]
        internal static extern bool EnumDisplayMonitors(HandleRef hdc, IntPtr rcClip, Monitor.MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern bool GetMonitorInfo(HandleRef hMonitor, [In, Out] Monitor.MonitorInfoEx info);

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorDefaultTo dwFlags);

        #endregion Methods

        #region Data Structures

        /// <summary>
        ///     Specifies the type of input event.
        /// </summary>
        internal enum InputType : uint
        {
            INPUT_MOUSE = 0,
        }

        /// <summary>
        ///     Specifies various mouse event flags.
        /// </summary>
        [Flags]
        internal enum MouseEventFlags : uint
        {
            MOVE = 0x0001,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            XDOWN = 0x0080,
            XUP = 0x0100,
            WHEEL = 0x0800,
            HWHEEL = 0x01000,
            MOVE_NOCOALESCE = 0x2000,
            VIRTUALDESK = 0x4000,
            ABSOLUTE = 0x8000
        }

        /// <summary>
        ///     The window sizing and positioning flags.
        /// </summary>
        [Flags]
        internal enum SetWindowPosFlags : uint
        {
            NoSize = 0x0001,
            NoMove = 0x0002,
            NoActivate = 0x0010
        }

        /// <summary>
        ///     Specifies the system metrics to be retrieved.
        /// </summary>
        internal enum SystemMetric
        {
            SM_CXSCREEN = 0,
            SM_CYSCREEN = 1
        }

        /// <summary>
        ///     Represents a generic input event.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public InputType type;
            public InputUnion U;
        }

        /// <summary>
        ///     Represents a union of input data types.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        /// <summary>
        ///     Represents the mouse input data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        /// <summary>
        ///     Represents a point in 2D space.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            public int X;
            public int Y;
        }

        internal enum MonitorDefaultTo
        {
            MONITOR_DEFAULTTONULL = 0,
            MONITOR_DEFAULTTOPRIMARY = 1,
            MONITOR_DEFAULTTONEAREST = 2
        }

        #endregion Data Structures
    }
}
