using System.Net;

namespace ServerCitadel
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Form1 form = new Form1();
            Application.Run(form);
        }
    }
}