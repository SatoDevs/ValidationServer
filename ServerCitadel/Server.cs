using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ServerCitadel
{
    internal class Server
    {
        public TcpListener listener;
        public bool isRunning = false;
        private BackgroundWorker serverWorker;
        private Form1 form;
        private Dictionary<string, string> licenseKeys;
        private Dictionary<string, int> licenseKeyUsage;
        private System.Threading.Timer licenseKeyTimer;
        private readonly int MAX_USAGE_COUNT = 10;
        private readonly int LICENSE_KEY_EXPIRY_MINUTES = 60;

        public Server(IPAddress ipAddress, int port, Form1 form)
        {
            listener = new TcpListener(ipAddress, port);
            this.form = form;
            serverWorker = new BackgroundWorker();
            serverWorker.WorkerSupportsCancellation = true;
            serverWorker.DoWork += new DoWorkEventHandler(serverWorker_DoWork);
            serverWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(serverWorker_RunWorkerCompleted);
            licenseKeys = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase)
            {
                {"a", "aa"},
                {"b", "bb"},
                {"c", "cc"}
            };
            licenseKeyUsage = new Dictionary<string, int>();
            licenseKeyTimer = new System.Threading.Timer(new TimerCallback(ClearLicenseKeyUsage), null, TimeSpan.Zero, TimeSpan.FromMinutes(LICENSE_KEY_EXPIRY_MINUTES));
        }

        public void Start()
        {
            serverWorker.RunWorkerAsync();
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();
        }

        private void serverWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listener.Start();
            isRunning = true;

            while (isRunning)
            {
                TcpClient client = null;
                try
                {
                    client = listener.AcceptTcpClient();
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        isRunning = false;
                        break;
                    }
                    else
                    {
                        form.Invoke((MethodInvoker)delegate
                        {
                            form.AppendToLog($"Error accepting client connection: {ex.Message}");
                        });
                        continue;
                    }
                }

                if (client != null)
                {
                    form.Invoke((MethodInvoker)delegate
                    {
                        form.AppendToLog("Client connected");
                    });

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string licenseKey = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                    form.Invoke((MethodInvoker)delegate
                    {
                        form.AppendToLog($"Client License Key: {licenseKey}");
                    });

                    form.Invoke((MethodInvoker)delegate
                    {
                        form.AppendToLog($"License keys: {string.Join(",", licenseKeys.Keys)}");
                        form.AppendToLog($"License key entered: {licenseKey}");
                    });

                    if (licenseKeys.ContainsKey(licenseKey))
                    {
                        if (licenseKeyUsage.ContainsKey(licenseKey) && licenseKeyUsage[licenseKey] >= MAX_USAGE_COUNT)
                        {
                            string textResponse = "Your license key has reached the maximum usage limit.";
                            SendToClient(client, textResponse);
                            form.Invoke((MethodInvoker)delegate
                            {
                                form.AppendToLog($"Sent data to client: {textResponse}");
                            });
                        }
                        else
                        {
                            string textResponse = "OK";
                            SendToClient(client, textResponse);
                            form.Invoke((MethodInvoker)delegate
                            {
                                form.AppendToLog($"Sent data to client: {textResponse}");
                            });
                            UpdateLicenseKeyUsage(licenseKey);
                        }
                    }
                    else stream.Close();
                    client.Close();
                }
            }
        }

        private void serverWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listener.Stop();
        }

        private void SendToClient(TcpClient client, string data)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
        }

        private void UpdateLicenseKeyUsage(string licenseKey)
        {
            if (licenseKeyUsage.ContainsKey(licenseKey))
            {
                licenseKeyUsage[licenseKey]++;
            }
            else
            {
                licenseKeyUsage.Add(licenseKey, 1);
            }
        }

        private void ClearLicenseKeyUsage(object state)
        {
            List<string> expiredKeys = new List<string>();
            foreach (KeyValuePair<string, int> pair in licenseKeyUsage)
            {
                if (pair.Value >= MAX_USAGE_COUNT)
                {
                    expiredKeys.Add(pair.Key);
                }
            }

            foreach (string key in expiredKeys)
            {
                licenseKeyUsage.Remove(key);
            }
        }
    }
}