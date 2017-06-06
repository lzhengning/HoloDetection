using HoloToolkit.Unity;
using System;
using UnityEngine;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

public class SocketManager : Singleton<SocketManager> {

    public string defaultIPAddress;
    public string port;

#if WINDOWS_UWP
    private StreamSocket socket = null;

    public async Task CreateSocketClient()
    {
        try
        {
            HostName serverHost = new HostName("166.111.71.10");
            socket = new StreamSocket();
            await socket.ConnectAsync(serverHost, "6666");
            Debug.Log("Socket connected");
        }
        catch
        {
            Debug.LogError("Error: failed to connect server");
        }
    }

    public async Task SendPhoto(byte[] photo)
    {
        using (var writer = new DataWriter(socket.OutputStream))
        {
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteString("P");
            writer.WriteInt32(photo.Length);
            writer.WriteBytes(photo);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
        }
    }

    public async Task SendInt(int x)
    {
        using (var writer = new DataWriter(socket.OutputStream))
        {
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteString("I");
            writer.WriteInt32(x);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
        }
    }

    public async Task SendString(string str)
    {
        using (var writer = new DataWriter(socket.OutputStream))
        {
            writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteString("M");
            writer.WriteString(str);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
        }
    }

    public async Task<BoundingBox[]> RecvDetections()
    {
        BoundingBox[] boxes = null;
        using (var reader = new DataReader(socket.InputStream))
        {
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;

            await reader.LoadAsync(sizeof(int));

            int count = reader.ReadInt32();
            boxes = new BoundingBox[count];
              
            for (int i = 0; i < count; ++i)
            {
                boxes[i] = new BoundingBox();
                await reader.LoadAsync(sizeof(float) * 5 + sizeof(int) * 3);
                boxes[i].x = reader.ReadSingle();
                boxes[i].y = reader.ReadSingle();
                boxes[i].w = reader.ReadSingle();
                boxes[i].h = reader.ReadSingle();
                boxes[i].imageHeight = reader.ReadInt32();
                boxes[i].imageWidth = reader.ReadInt32();
                boxes[i].prob = reader.ReadSingle();
                int length = reader.ReadInt32();
                await reader.LoadAsync((uint) length);
                boxes[i].name = reader.ReadString((uint) length);
            }
            reader.DetachStream();
        }
        return boxes;
    }
#else
    public bool CreateSocketClient()
    {
        return false;
    }
    public void SendPhoto(byte[] photo)
    {
    }
    public void SendInt(int x)
    {
    }
    public void SendString(string str)
    {
    }
    public void RecvDetections()
    {
    }
#endif

}
