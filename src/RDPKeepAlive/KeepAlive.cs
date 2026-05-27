using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal static class KeepAlive
    {
        private const int ClassNameCapacity = 128;
        private const int WindowTitleCapacity = 128;

        private static readonly string[] RdpClients = new string[]
        {
            "TscShellContainerClass",
            "WindowsForms10.Window.8.app.0.1d2098a_r8_ad1"
        };

        private static bool _clientIsNotTopmost;
        private static bool _found;
        private static IntPtr _originalForegroundWindow;
        private static string _rdpClientClassName = string.Empty;
        private static string _rdpClientWindowTitle = string.Empty;
        private static IntPtr _windowInFront;

        /// <summary>
        ///     Executes the keep-alive process for the RDP client window.
        /// </summary>
        internal static void Execute()
        {
            var clientWindow = NativeMethods.FindWindowExW(IntPtr.Zero, IntPtr.Zero, _rdpClientClassName, _rdpClientWindowTitle);
            if (clientWindow == IntPtr.Zero)
            {
                return;
            }

            NativeMethods.INPUT input;
            if (!TryGetMouseMovementParams(out input))
            {
                throw new KeepAliveException("Failed to get mouse movement parameters.", new Win32Exception(Marshal.GetLastWin32Error()));
            }

            TakeSnapshot(clientWindow);
            ProcessMouseMovement(clientWindow, input);
            RestoreSnapshot(clientWindow);
        }

        /// <summary>
        ///     Finds the RDP client window by enumerating all top-level windows.
        /// </summary>
        internal static bool TryGetRDPClient(out Client client)
        {
            _found = false;
            NativeMethods.EnumWindows(EnumRDPWindowsProc, IntPtr.Zero);

            client = new Client();
            client.ClassName = _rdpClientClassName;
            client.WindowTitle = _rdpClientWindowTitle;

            return _found;
        }

        /// <summary>
        ///     Callback method invoked by EnumWindows for each top-level window.
        /// </summary>
        private static bool EnumRDPWindowsProc(IntPtr hWnd, IntPtr lParam)
        {
            string className;
            string windowTitle;

            if (TryGetWindowClass(hWnd, out className) && TryGetWindowTitle(hWnd, out windowTitle))
            {
				Console.WriteLine(className);
                bool isRdpClient = false;
                foreach (string rdpClass in RdpClients)
                {
                    if (string.Equals(className, rdpClass, StringComparison.OrdinalIgnoreCase))
                    {
                        isRdpClient = true;
                        break;
                    }
                }

                if (isRdpClient)
                {
                    _found = true;
                    _rdpClientClassName = className;
                    _rdpClientWindowTitle = windowTitle;
                    return false;
                }
            }
            return true;
        }

        private static IntPtr GetWindowInFront(IntPtr clientWindow)
        {
            uint pidClient;
            NativeMethods.GetWindowThreadProcessId(clientWindow, out pidClient);

            var clientMonitor = Monitor.GetMonitorFromWindow(clientWindow);

            uint pidNext = 0;
            IntPtr next = clientWindow;
            Monitor nextMonitor = clientMonitor;

            while (pidNext == 0 || pidClient == pidNext || !clientMonitor.Equals(nextMonitor))
            {
                next = NativeMethods.GetWindow(next, 3); // GW_HWNDPREV

                NativeMethods.GetWindowThreadProcessId(next, out pidNext);
                nextMonitor = Monitor.GetMonitorFromWindow(next);
            }

            return next;
        }

        private static void ProcessMouseMovement(IntPtr clientWindow, NativeMethods.INPUT input)
        {
            if (_clientIsNotTopmost)
            {
                NativeMethods.SetForegroundWindow(clientWindow);
            }

            if (NativeMethods.SendInput(1, ref input, Marshal.SizeOf(typeof(NativeMethods.INPUT))) == 0)
            {
                throw new KeepAliveException("Failed to send mouse movement input.", new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        private static void RestoreSnapshot(IntPtr clientWindow)
        {
            if (!_clientIsNotTopmost)
            {
                return;
            }

            NativeMethods.SetForegroundWindow(_originalForegroundWindow);

            NativeMethods.SetWindowPos(
               clientWindow,
               _windowInFront,
               0, 0, 0, 0,
               NativeMethods.SetWindowPosFlags.NoMove |
               NativeMethods.SetWindowPosFlags.NoSize |
               NativeMethods.SetWindowPosFlags.NoActivate);
        }

        private static void TakeSnapshot(IntPtr clientWindow)
        {
            _originalForegroundWindow = NativeMethods.GetForegroundWindow();
            _clientIsNotTopmost = _originalForegroundWindow != clientWindow;

            if (!_clientIsNotTopmost)
            {
                return;
            }

            _windowInFront = GetWindowInFront(clientWindow);
        }

        private static bool TryGetMouseMovementParams(out NativeMethods.INPUT inputParams)
        {
            inputParams = new NativeMethods.INPUT();
            inputParams.type = NativeMethods.InputType.INPUT_MOUSE;
            inputParams.U = new NativeMethods.InputUnion();
            inputParams.U.mi = new NativeMethods.MOUSEINPUT();

            NativeMethods.POINT currentPosition;
            if (!NativeMethods.GetCursorPos(out currentPosition))
            {
                return false;
            }

            inputParams.U.mi.dwFlags = NativeMethods.MouseEventFlags.MOVE | NativeMethods.MouseEventFlags.ABSOLUTE;

            int screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSCREEN);
            int screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CYSCREEN);

            inputParams.U.mi.dx = (currentPosition.X * 65535) / screenWidth;
            inputParams.U.mi.dy = (currentPosition.Y * 65535) / screenHeight;

            return true;
        }

        private static bool TryGetWindowClass(IntPtr hWnd, out string className)
        {
            char[] name = new char[ClassNameCapacity];
            if (NativeMethods.GetClassName(hWnd, name, ClassNameCapacity) == 0)
            {
                className = string.Empty;
                return false;
            }

            string trimmed = new string(name).TrimEnd('\0');
            className = trimmed.Length > 0 ? trimmed : "[NoClass]";
            return true;
        }

        private static bool TryGetWindowTitle(IntPtr hWnd, out string windowTitle)
        {
            char[] title = new char[WindowTitleCapacity];
            if (NativeMethods.GetWindowText(hWnd, title, WindowTitleCapacity) == 0)
            {
                windowTitle = string.Empty;
                return false;
            }

            string trimmed = new string(title).TrimEnd('\0');
            windowTitle = trimmed.Length > 0 ? trimmed : "[NoTitle]";
            return true;
        }
    }
}
