using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];

    public Client(string ipAddress, int port)
    {
        client = new TcpClient();
        client.Connect(ipAddress, port);
        stream = client.GetStream();
        Console.WriteLine("Connected to server...");
    }

    public void Send(string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);
        stream.Write(data, 0, data.Length);
        Console.WriteLine(message);
    }

    public string Receive()
    {
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }

    public void Close()
    {
        stream.Close();
        client.Close();
    }
}
