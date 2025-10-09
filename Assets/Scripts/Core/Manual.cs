using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Manual : MonoBehaviour
{
    [Header("Manual Settings")]
    [SerializeField] private TextAsset manualTextAsset;
    [SerializeField] private string manualFileName = "Manual.txt";


    public void OpenManual()
    {
        string manualPath = GetManualPath();

        if (!string.IsNullOrEmpty(manualPath) && File.Exists(manualPath))
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = manualPath,
                    UseShellExecute = true // open default text editor
                };

                Process.Start(startInfo);
                UnityEngine.Debug.Log($"Opened manual: {manualPath}");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to open manual: {ex.Message}");
            }
        }
        else
        {
            UnityEngine.Debug.LogError($"Manual file not found: {manualPath}");
            CreateSampleManual();
        }
    }

    private string GetManualPath()
    {
        return CreateTempManualFromTextAsset();
    }

    private string CreateTempManualFromTextAsset()
    {
        try
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "FishLamp_Manual.txt");
            File.WriteAllText(tempPath, manualTextAsset.text);
            UnityEngine.Debug.Log($"Created temporary manual from TextAsset: {tempPath}");
            return tempPath;
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to create temporary manual: {ex.Message}");
            return "";
        }
    }


    private void CreateSampleManual()
    {
        try
        {
            string sampleManualPath;

            sampleManualPath = Path.Combine(Application.persistentDataPath, manualFileName);

            string sampleContent = @"Fish Lamp Manual
==================

Fish Lamp User Manual
====================

Controls:
- Mouse: Interact with the lamp button
- Right-click + drag: Rotate the lamp 
- Left-click + drag: Move the lamp
- Right-click + mouse scroll wheel: Scale the lamp

System Tray:
- Select ""Quit"" to exit the application
- Select ""Reset"" to reset rotation, scale and position of the widget


Troubleshooting:
- If the app becomes unresponsive, use Task Manager
";

            File.WriteAllText(sampleManualPath, sampleContent);
            UnityEngine.Debug.Log($"Created sample manual at: {sampleManualPath}");

            OpenManual();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to create sample manual: {ex.Message}");
        }
    }
}
