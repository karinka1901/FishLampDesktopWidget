using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TrayCommandReceiver : MonoBehaviour
{
    [Header("References")]
    public Manual manual;
    public ObjectDragger objectDragger;
    public ObjectRotator objectRotator;
    public ObjectScaler objectScaler;
    //public ChangeDisplayMonitor changeDisplayMonitor;

    private NamedPipeServerStream pipeServer;
    private bool isListening = false;
    private CancellationTokenSource cancellationTokenSource;

    void Start()
    {
        if (manual == null)
        {
            manual = FindAnyObjectByType<Manual>();
            if (manual == null)
            {
                UnityEngine.Debug.LogError("manual not found! Please assign it in the inspector.");
            }
        }

        StartPipeServer();
    }

    private async void StartPipeServer()
    {
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Run(() => ListenForCommands(cancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Pipe server error: {ex.Message}");
        }
    }

    private async Task ListenForCommands(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                pipeServer = new NamedPipeServerStream("FishLampPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                await pipeServer.WaitForConnectionAsync(cancellationToken);

                if (pipeServer.IsConnected)
                {
                    UnityEngine.Debug.Log("Tray client connected!");

                    // Reading the command
                    byte[] buffer = new byte[256];
                    int bytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        string command = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        UnityEngine.Debug.Log($"Received command: {command}");

                        // Processing the command on the main thread
                        UnityMainThreadDispatcher.Instance.Enqueue(() => ProcessCommand(command));
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Pipe communication error: {ex.Message}");
            }
            finally
            {
                try
                {
                    pipeServer?.Disconnect();
                    pipeServer?.Dispose();
                }
                catch { }
            }

            // Small delay before creating new pipe server
            await Task.Delay(100, cancellationToken);
        }
    }

    private void ProcessCommand(string command)
    {
        switch (command.ToUpper())
        {
            case "MANUAL":
                DisplayManual();
                break;
            case "RESET":
                Reset();
                break;
            case "MOVE":
               // changeDisplayMonitor.MoveToNextMonitor();
                break;
            default:
                UnityEngine.Debug.LogWarning($"Unknown command: {command}");
                break;
        }
    }

    private void DisplayManual()
    {
        if (manual != null)
        {
            manual.OpenManual();
            UnityEngine.Debug.Log("Opened manual via system tray");
        }
        else
        {
            UnityEngine.Debug.LogError("Manual reference is null! Cannot enable rotation.");
        }
    }

    private void Reset()
    {
        if (objectDragger != null)
        {
            objectDragger.Reset();
            UnityEngine.Debug.Log("Reset object position via system tray");
        }
        if (objectRotator != null)
        {

            objectRotator.ResetRotation();
            UnityEngine.Debug.Log("Reset object rotation via system tray");

        }
        if (objectScaler != null)
        {
            objectScaler.Reset();
            UnityEngine.Debug.Log("Reset object scale via system tray");
        }

        //if (changeDisplayMonitor != null)
        //{
        //    changeDisplayMonitor.Reset();
        //    UnityEngine.Debug.Log("Reset display via system tray");
        //}

    }

    void OnDestroy()
    {
        cancellationTokenSource?.Cancel();

        try
        {
            pipeServer?.Disconnect();
            pipeServer?.Dispose();
        }
        catch { }
    }

    void OnApplicationQuit()
    {
        OnDestroy();
    }
}