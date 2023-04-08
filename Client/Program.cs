using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        TcpClient client = new TcpClient();
        client.Connect("127.0.0.1", 8888);

        Console.Write("Enter your license key:");
        string licenseKey = Console.ReadLine();

        // Send the license key to the server for validation
        string validationMessage = licenseKey;
        byte[] validationData = Encoding.ASCII.GetBytes(validationMessage);
        NetworkStream validationStream = client.GetStream();
        validationStream.Write(validationData, 0, validationData.Length);

        // Receive the validation response from the server
        byte[] validationBuffer = new byte[1024];
        int validationBytesRead = validationStream.Read(validationBuffer, 0, validationBuffer.Length);
        string validationResponse = Encoding.ASCII.GetString(validationBuffer, 0, validationBytesRead);

        if (validationResponse == "OK")
        {
            Console.WriteLine("Authentication succeeded!");
        }
        else
        {
            Console.WriteLine("Authentication failed - invalid license key!");
        }

        client.Close();
    }
}
