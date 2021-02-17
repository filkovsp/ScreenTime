/*
* Code Example:
* https://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title
* https://www.tcx.be/blog/2006/list-open-windows/
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.IO;

namespace Windows
{

    class LogRecord
    {
        public string user { get; set; }
        public string hid { get; set; }
        public string pid { get; set; }
        public string filename { get; set; }
        public string title { get; set; }

        public LogRecord() { }
    }

    class Windows
    {
        private static readonly HttpClient _Client = new HttpClient();
        private static JavaScriptSerializer _Serializer = new JavaScriptSerializer();
        private static LogRecord data = new LogRecord();
        private static bool showConsole = false;
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        
        public static bool HideConsole()
        {
            var hwnd = WindowProcessor.GetConsoleWindow();
            WindowProcessor.GetWindowThreadProcessId(hwnd, out var pid);

            if (pid != Process.GetCurrentProcess().Id)
                return false;

            WindowProcessor.ShowWindow(hwnd, SW_HIDE);

            return true;
        }

        [STAThread]
        static void Main(string[] args)
        {

            if (args.Contains("--verbose")) {
                showConsole = true;
            }
            else {
                HideConsole();
            }

            while (true)
            {

                IDictionary<IntPtr, WindowProcess> windows = WindowProcessor.GetOpenWindowProcesses();

                if (showConsole) {

                    Console.Clear();

                    Console.WriteLine(windows.Values.ToStringTable(
                        new[] { " * ", "HID", "PID", "isAlive", "FileName", "Title" },
                        window => (window.isActive() ? " * " : " "),
                        window => window.hWnd,
                        window => window.pid,
                        window => (window.isAlive() ? " + " : " "),
                        window => window.imageFileName,
                        window => window.GetWindowText()
                    ));
                }

                IEnumerable<WindowProcess> wnd = windows.Values.Where(window => window.isActive());

                if (wnd.Count() > 0) {

                    data.user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    data.hid = wnd.First().hWnd.ToString();
                    data.pid = wnd.First().pid.ToString();
                    data.filename = wnd.First().imageFileName;
                    data.title = wnd.First().GetWindowText();

                    try {
                        post(data).Wait();
                    } catch (Exception ex) {
                        if (showConsole)
                        {
                            Console.Out.WriteLine(ex.Message);
                        }
                        else {
                            Log(ex.Message).Wait();
                        }
                    }
                }

                Thread.Sleep(5 * 1000);
            }
        }

        static async Task post(LogRecord data)
        {
            string url = "http://192.168.0.10:3500/screentime";
            var request = _Serializer.Serialize(data);
            var headers = new Dictionary<string, string>();
            var response = await Request(HttpMethod.Post, url, request, headers);

            if (!response.IsSuccessStatusCode) {
                string responseText = await response.Content.ReadAsStringAsync();
                Log(responseText).Wait();
            }
        }



        /// <summary>
        /// Makes an async HTTP Request
        /// </summary>
        /// <param name="pMethod">Those methods you know: GET, POST, HEAD, etc...</param>
        /// <param name="pUrl">Very predictable...</param>
        /// <param name="pJsonContent">String data to POST on the server</param>
        /// <param name="pHeaders">If you use some kind of Authorization you should use this</param>
        /// <returns></returns>
        static async Task<HttpResponseMessage> Request(HttpMethod pMethod, string pUrl, string pJsonContent, Dictionary<string, string> pHeaders)
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = pMethod;
            httpRequestMessage.RequestUri = new Uri(pUrl);
            foreach (var head in pHeaders)
            {
                httpRequestMessage.Headers.Add(head.Key, head.Value);
            }
            switch (pMethod.Method)
            {
                case "POST":
                    HttpContent httpContent = new StringContent(pJsonContent, Encoding.UTF8, "application/json");
                    httpRequestMessage.Content = httpContent;
                    break;

            }

            return await _Client.SendAsync(httpRequestMessage);
        }


        static async Task Log(string text) {
            string path = @"./Windows.log";
            StreamWriter logFile;

            if (!File.Exists(path))
            {
                logFile = new StreamWriter(path);
            }
            else
            {
                logFile = File.AppendText(path);
            }

            using (logFile)
            {
                try
                {
                    logFile.WriteLine(DateTime.Now.ToString() + " - " + text);
                }
                catch (Exception ex) {
                    //
                }
            }
        }

    }

}