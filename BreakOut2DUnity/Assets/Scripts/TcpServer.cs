using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class TcpServer : MonoBehaviour
{
    [Header("Connection Parameters")]
    private string address = "127.0.0.1";
    private int port = 60000;
    private int waitMs = 25; // same rate at which LV writes to port

    private TcpListener listener;
    private Thread thread;
    private CancellationTokenSource tokenSource;

    public float PaddleCommand { get; private set; } = 0f; // belongs to [0, 1] or -1
    public float PaddleActualPosition { get; set; } = 0f; // belongs to [0, 1] or -1
    public float PaddleDesiredPosition { get; set; } = 0f; // belongs to [0, 1] or -1
    public float BallDistanceFromPaddleDesiredPosition { get; set; } = 0f; // belongs to [0, 1] or -1
    public bool IsClientConnected { get; private set; } = false;

    void Start()
    {
        this.address = MenuManager.Config.TcpAddress;
        this.port = MenuManager.Config.TcpPort;
        this.waitMs = MenuManager.Config.TcpReadWriteIntervalMs;

        this.tokenSource = new CancellationTokenSource();
        this.thread = new Thread(() => this.Run(this.tokenSource.Token)) { IsBackground = true };
        this.thread.Start();
    }

    private void Run(CancellationToken token)
    {
        byte[] readBuffer = new byte[4], writeBuffer = new byte[12];
        this.listener = new TcpListener(IPAddress.Parse(this.address), this.port);
        this.listener.Start();

        try
        {
            // outer loop waits for a client to connect
            while (true)
            {
                if (this.listener.Pending())
                {
                    using (var client = this.listener.AcceptTcpClient()) // blocking
                    using (var stream = client.GetStream())
                    {
                        Debug.Assert(stream.CanWrite, "TcpServer stream cannot be written");
                        Debug.Assert(stream.CanRead, "TcpServer stream cannot be read");

                        // while a client is connected (i.e., is writing), read client 
                        // paddle command and write desired paddle position
                        this.IsClientConnected = true;
                        while (true)
                        {
                            // read paddle command
                            int i = stream.Read(readBuffer, 0, readBuffer.Length);
                            if (i == 0) break;
                            Array.Reverse(readBuffer, 0, readBuffer.Length);
                            var readVal = BitConverter.ToSingle(readBuffer, 0);
                            if (readVal == -1f) break;
                            this.PaddleCommand = Mathf.Clamp01(readVal);

                            // write actual and desired paddle position and normalized ball-paddle distance
                            var float1 = BitConverter.GetBytes(Mathf.Clamp01(this.PaddleActualPosition));
                            var float2 = BitConverter.GetBytes(Mathf.Clamp01(this.PaddleDesiredPosition));
                            var float3 = BitConverter.GetBytes(Mathf.Clamp01(this.BallDistanceFromPaddleDesiredPosition));
                            Array.Reverse(float1, 0, float1.Length);
                            Array.Reverse(float2, 0, float2.Length);
                            Array.Reverse(float3, 0, float3.Length);
                            Array.Copy(float1, 0, writeBuffer, 0, float1.Length);
                            Array.Copy(float2, 0, writeBuffer, float1.Length, float2.Length);
                            Array.Copy(float3, 0, writeBuffer, float1.Length + float2.Length, float3.Length);
                            stream.Write(writeBuffer, 0, writeBuffer.Length);

                            // go to sleep
                            Thread.Sleep(this.waitMs);
                            if (token.IsCancellationRequested)
                                break;
                        }
                        this.IsClientConnected = false;
                    }
                }
                else
                {
                    Thread.Sleep(this.waitMs * 5);
                }

                if (token.IsCancellationRequested)
                    break;
            }
        }
        catch (SocketException ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            this.listener.Stop();
        }
    }

    void OnDestroy()
    {
        this.tokenSource?.Cancel();
        this.tokenSource?.Dispose();
        this.thread?.Join(waitMs * 10);
    }

    public override string ToString() 
        => $"TcpServer @ {this.address}, {this.port}. Client? {this.IsClientConnected}. Paddle Command = {this.PaddleCommand}. Paddle Desired Pos = {this.PaddleDesiredPosition}";
}
