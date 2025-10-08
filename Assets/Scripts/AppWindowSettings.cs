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

    [Header("System Tray Icon")]
    public Texture2D trayIconTexture; 

    private Process trayHelperProcess;
    private int currentProcessId;

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
        // Get current process ID for the tray script
        currentProcessId = Process.GetCurrentProcess().Id;

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

        // Create system tray using PowerShell
        CreateSystemTrayWithPowerShell();

        UnityEngine.Debug.Log("Application started. Use system tray to exit.");
    }

    private void CreateSystemTrayWithPowerShell()
    {
        try
        {
            string iconPath = "";
            
            if (trayIconTexture != null)
            {
                iconPath = ExportIconToFile();
            }

            string iconLoadScript = string.IsNullOrEmpty(iconPath) 
                ? "$notifyIcon.Icon = [System.Drawing.SystemIcons]::Application"
                : $@"
try {{
    $bitmap = New-Object System.Drawing.Bitmap('{iconPath.Replace("\\", "\\\\")}')
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $notifyIcon.Icon = $icon
}} catch {{
    Write-Host 'Failed to load custom icon, using default'
    $notifyIcon.Icon = [System.Drawing.SystemIcons]::Application
}}";

            string powerShellScript = $@"
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

try {{
    $notifyIcon = New-Object System.Windows.Forms.NotifyIcon
    {iconLoadScript}
    $notifyIcon.Text = 'Fish Lamp'
    $notifyIcon.Visible = $true

    $contextMenu = New-Object System.Windows.Forms.ContextMenuStrip
    
    $exitItem = $contextMenu.Items.Add('Quit')
    $exitItem.add_Click({{
        Write-Host 'Exit requested from system tray'
        $notifyIcon.Visible = $false
        $notifyIcon.Dispose()
        
        # Force kill the process immediately
        try {{
            Stop-Process -Id {currentProcessId} -Force -ErrorAction SilentlyContinue
        }} catch {{
            Write-Host 'Process already terminated'
        }}
        exit
    }})

    $notifyIcon.ContextMenuStrip = $contextMenu

    # Add double-click event for notification
    $notifyIcon.add_DoubleClick({{
        $notifyIcon.ShowBalloonTip(3000, 'Fish Lamp', 'Right-click to exit application', [System.Windows.Forms.ToolTipIcon]::Info)
    }})

    Write-Host 'System tray icon created successfully'

    # Keep the script running and monitor the specific process ID
    while ($true) {{
        Start-Sleep -Seconds 2
        try {{
            $process = Get-Process -Id {currentProcessId} -ErrorAction SilentlyContinue
            if (-not $process) {{
                Write-Host 'Main application (PID {currentProcessId}) closed, exiting tray'
                $notifyIcon.Visible = $false
                $notifyIcon.Dispose()
                break
            }}
        }} catch {{
            Write-Host 'Error checking process, exiting tray'
            $notifyIcon.Visible = $false
            $notifyIcon.Dispose()
            break
        }}
    }}
}} catch {{
    Write-Host ""Error creating system tray: $($_.Exception.Message)""
    exit 1
}}";

            string tempPath = Path.Combine(Path.GetTempPath(), "fishlamp_tray.ps1");
            File.WriteAllText(tempPath, powerShellScript);

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-WindowStyle Hidden -ExecutionPolicy Bypass -File \"{tempPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            trayHelperProcess = Process.Start(startInfo);
            
            // Read output for debugging
            if (trayHelperProcess != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    string output = trayHelperProcess.StandardOutput.ReadToEnd();
                    string errors = trayHelperProcess.StandardError.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(output))
                        UnityEngine.Debug.Log($"PowerShell output: {output}");
                    if (!string.IsNullOrEmpty(errors))
                        UnityEngine.Debug.LogError($"PowerShell errors: {errors}");
                });
            }

            UnityEngine.Debug.Log($"System tray process started with PID monitoring {currentProcessId} and {(string.IsNullOrEmpty(iconPath) ? "default" : "custom")} icon.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to create system tray: {ex.Message}");
        }
    }

    private string ExportIconToFile()
    {
        try
        {
            if (trayIconTexture == null) return "";

            // Create a readable copy of the texture
            Texture2D readableTexture = new Texture2D(trayIconTexture.width, trayIconTexture.height, TextureFormat.RGBA32, false);
            
            // Copy pixels from the original texture
            RenderTexture renderTexture = RenderTexture.GetTemporary(trayIconTexture.width, trayIconTexture.height);
            Graphics.Blit(trayIconTexture, renderTexture);
            
            RenderTexture.active = renderTexture;
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();
            
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            // Convert to PNG
            byte[] pngData = readableTexture.EncodeToPNG();
            
            // Clean up
            if (Application.isPlaying)
                Destroy(readableTexture);
            else
                DestroyImmediate(readableTexture);

            // Save to temp file
            string iconPath = Path.Combine(Path.GetTempPath(), "fishlamp_icon.png");
            File.WriteAllBytes(iconPath, pngData);

            UnityEngine.Debug.Log($"Icon exported to: {iconPath}");
            return iconPath;
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to export icon: {ex.Message}");
            return "";
        }
    }

    private void OnDestroy()
    {
        // Clean up helper process
        if (trayHelperProcess != null && !trayHelperProcess.HasExited)
        {
            try
            {
                trayHelperProcess.Kill();
            }
            catch { }
        }

        // Clean up temp files
        try
        {
            string iconPath = Path.Combine(Path.GetTempPath(), "fishlamp_icon.png");
            if (File.Exists(iconPath))
                File.Delete(iconPath);
            
            string scriptPath = Path.Combine(Path.GetTempPath(), "fishlamp_tray.ps1");
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
        catch { }
    }

    private void OnApplicationQuit()
    {
        OnDestroy();
    }
}
