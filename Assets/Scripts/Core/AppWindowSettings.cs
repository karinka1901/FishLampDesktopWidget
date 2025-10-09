using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

public class AppWindowSettings : MonoBehaviour
{
    // WinAPI
    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020; 
    private const uint WS_EX_TOOLWINDOW = 0x00000080; // Hide from taskbar
    private const uint LWA_COLORKEY = 0x00000001;
    private const uint LWA_ALPHA = 0x00000002;

    // SetWindowPos flags
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetActiveWindow(); 

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8) return GetWindowLongPtr64(hWnd, nIndex);
        return GetWindowLongPtr32(hWnd, nIndex);
    }

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    // color key as 0x00BBGGRR (COLORREF) — using magenta 255,0,255
    private const uint COLOR_KEY_MAGENTA = 0x00FF00FF;

    void Start()
    {
        IntPtr hWnd = GetForegroundWindow();
        if (hWnd == IntPtr.Zero) hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero)
        {
            UnityEngine.Debug.Log("Couldn't get window handle.");
            return;
        }

        // Set the window to always be on top
        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

        IntPtr style = GetWindowLongPtr(hWnd, GWL_EXSTYLE);
        ulong newStyle = (ulong)style.ToInt64() | WS_EX_LAYERED | WS_EX_TOOLWINDOW; // add layered and hide from taskbar
        SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr((long)newStyle));

        // Extend frame into client area for better transparency
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        // Use color key transparency: make the magenta background fully transparent
        bool ok = SetLayeredWindowAttributes(hWnd, COLOR_KEY_MAGENTA, 0, LWA_COLORKEY);
        if (!ok) UnityEngine.Debug.Log("SetLayeredWindowAttributes failed.");
        else UnityEngine.Debug.Log("Set layered window color-keyed transparency OK.");

    }
}
