using System.ComponentModel;
using System.Net;

namespace ServerCitadel
{
    public partial class Form1 : Form
    {
        private Server server;
        private Thread serverThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Create a new server object
            server = new Server(IPAddress.Any, 8888, this);

            // Create a new thread to run the server
            serverThread = new Thread(new ThreadStart(server.Start));

            // Start the thread
            serverThread.Start();

            // Disable the start button and enable the stop button
            startButton.Enabled = false;
            stopButton.Enabled = true;

            // Log the server start message
            AppendToLog("Server started, waiting for client connection...");
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            // Stop the server by setting the isRunning flag to false
            server.isRunning = false;

            // Stop the listener
            server.listener.Stop();

            // Join the server thread to wait for it to finish
            serverThread.Join();

            // Disable the stop button and enable the start button
            stopButton.Enabled = false;
            startButton.Enabled = true;
        }

        public void AppendToLog(string message)
        {
            // Append the message to the log box, with a timestamp
            string logMessage = string.Format("[{0}] {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), message);

            if (logBox.InvokeRequired)
            {
                logBox.Invoke((MethodInvoker)delegate
                {
                    logBox.AppendText(logMessage + "\r\n");
                });
            }
            else
            {
                logBox.AppendText(logMessage + "\r\n");
            }
        }

    }
}