using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RDPKeepAlive
{
    internal sealed class Monitor : IEquatable<Monitor>
    {
        internal static List<Monitor> AllMonitors
        {
            get
            {
                var closure = new MonitorEnumCallback();
                var proc = new MonitorEnumProc(closure.Callback);
                NativeMethods.EnumDisplayMonitors(HandleRef, IntPtr.Zero, proc, IntPtr.Zero);
                var result = new List<Monitor>();
                foreach (Monitor m in closure.Monitors)
                {
                    result.Add(m);
                }
                return result;
            }
        }

        internal Rect Bounds { get; private set; }
        internal IntPtr Handle { get; private set; }
        internal bool IsPrimary { get; private set; }
        internal string Name { get; private set; }
        internal Rect WorkingArea { get; private set; }

        private static readonly HandleRef HandleRef = new HandleRef(null, IntPtr.Zero);

        private Monitor(IntPtr monitor, IntPtr hdc)
        {
            var info = new MonitorInfoEx();
            NativeMethods.GetMonitorInfo(new HandleRef(null, monitor), info);

            Bounds = new Rect(
                info.rcMonitor.Left, info.rcMonitor.Top,
                info.rcMonitor.Right - info.rcMonitor.Left,
                info.rcMonitor.Bottom - info.rcMonitor.Top);

            WorkingArea = new Rect(
                info.rcWork.Left, info.rcWork.Top,
                info.rcWork.Right - info.rcWork.Left,
                info.rcWork.Bottom - info.rcWork.Top);

            IsPrimary = (info.dwFlags & (int)NativeMethods.MonitorDefaultTo.MONITOR_DEFAULTTOPRIMARY) != 0;
            Name = new string(info.szDevice).TrimEnd((char)0);
            Handle = monitor;
        }

        internal delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam);

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Monitor))
                return false;
            return Handle == ((Monitor)obj).Handle;
        }

        public bool Equals(Monitor other)
        {
            if (other == null) return false;
            return Handle == other.Handle;
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Name: {0} | IsPrimary: {1} | Bounds: {2} | WorkingArea: {3}", Name, IsPrimary, Bounds, WorkingArea);
        }

        internal static Rect GetMonitorBounds(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcMonitor;
        }

        internal static Monitor GetMonitorFromWindow(IntPtr hWnd)
        {
            var hMonitor = GetMonitorHandleFromWindow(hWnd);
            foreach (var monitor in AllMonitors)
            {
                if (monitor.Handle == hMonitor.Handle)
                    return monitor;
            }
            return null;
        }

        internal static Rect GetMonitorWorkArea(IntPtr hWindow)
        {
            var monitorInfoEx = new MonitorInfoEx();
            var hMonitor = GetMonitorHandleFromWindow(hWindow);
            NativeMethods.GetMonitorInfo(hMonitor, monitorInfoEx);
            return monitorInfoEx.rcWork;
        }

        private static HandleRef GetMonitorHandleFromWindow(IntPtr hWnd)
        {
            var ptrMonitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MonitorDefaultTo.MONITOR_DEFAULTTONEAREST);
            return new HandleRef(null, ptrMonitor);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        internal class MonitorInfoEx
        {
            internal int cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
            internal Rect rcMonitor = new Rect();
            internal Rect rcWork = new Rect();
            internal int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal char[] szDevice = new char[32];
        }

        private class MonitorEnumCallback
        {
            internal ArrayList Monitors { get; private set; }

            internal MonitorEnumCallback()
            {
                Monitors = new ArrayList();
            }

            internal bool Callback(IntPtr monitor, IntPtr hdc, IntPtr lprcMonitor, IntPtr lParam)
            {
                Monitors.Add(new Monitor(monitor, hdc));
                return true;
            }
        }
    }
}
