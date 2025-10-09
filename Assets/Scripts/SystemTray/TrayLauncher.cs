using System.Diagnostics;
using System.IO;
using UnityEngine;

public class TrayLauncher : MonoBehaviour
{
    [Header("System Tray Icon")]
    public Texture2D trayIconTexture;

    void Start()
    {
        string exePath = FindProgramExecutable();

        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
        {
            // Get the Unity process ID to pass to the tray helper
            int unityProcessId = Process.GetCurrentProcess().Id;
            
            string iconPath = "";
            if (trayIconTexture != null)
            {
                iconPath = ExportIconToFile();
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"{unityProcessId} \"{iconPath}\"", // Pass Unity PID and icon path
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
            UnityEngine.Debug.Log($"Launched Program.exe from: {exePath} with Unity PID: {unityProcessId}");
        }
        else
        {
            UnityEngine.Debug.Log("Program.exe not found in StreamingAssets");
        }
    }

    private string FindProgramExecutable()
    {
        try
        {
            // Search for Program.exe in all subdirectories of StreamingAssets
            string[] files = Directory.GetFiles(Application.streamingAssetsPath, "Program.exe", SearchOption.AllDirectories);
            
            if (files.Length > 0)
            {
                UnityEngine.Debug.Log($"Found Program.exe at: {files[0]}");
                return files[0];
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Error searching for Program.exe: {ex.Message}");
        }
        
        return null;
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
            string iconPath = Path.Combine(System.IO.Path.GetTempPath(), "fishlamp_icon.png");
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
}
