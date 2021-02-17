using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace Windows
{
    public class WindowProcess
    {
        public IntPtr hWnd { get; private set; }
        public uint pid { get; private set; }
        public string imageFullPath { get; private set; }
        public string imageFileName { get; private set; }
        public string imageFolderName { get; private set; }
        public string windowText { get; set; }
        public DateTime timeRegistered { get; private set; }

        public enum ProcessInfo : uint
        {
            hWnd = 0,
            pid = 1,
            isActive = 3,
            fileName = 4,
            title = 5,
            timeRegistered = 6,
            runTime = 7
        }

        public WindowProcess(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            try
            {
                this.pid = WindowProcessor.GetProcessId(hWnd);
                this.imageFullPath = WindowProcessor.GetProcessFilePath(this.pid);
                // imageFolderName = Path.GetDirectoryName(imageFullPath);
                this.imageFileName = Path.GetFileName(imageFullPath);
                this.windowText = WindowProcessor.GetWindowText(hWnd);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
            finally
            {
                this.timeRegistered = DateTime.Now;
            }
        }

        public bool isAlive()
        {
            return (WindowProcessor.GetProcessId(this.hWnd) == this.pid);
        }

        public bool isActive()
        {
            return WindowProcessor.IsActive(this.hWnd);
        }

        public string GetWindowText()
        {
            return WindowProcessor.GetWindowText(this.hWnd);
        }

        /// <summary> Retreives process' handle by windows PID </summary>
        public Process GetProcessInfoEx()
        {
            Process proc = null;
            try
            {
                proc = Process.GetProcessById((int)this.pid);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message.ToString());
            }
            finally
            {
                // Console.Out.WriteLine(this.pid);
            }
            return proc;
        }

        public List<string> ToString(params ProcessInfo[] fields)
        {
            List<string> window = new List<string>();

            foreach (ProcessInfo field in fields)
            {
                switch (field)
                {
                    case ProcessInfo.isActive:
                        window.Add(this.isAlive().ToString());
                        break;
                    case ProcessInfo.pid:
                        window.Add(this.hWnd.ToString());
                        break;
                    case ProcessInfo.fileName:
                        window.Add(this.imageFileName);
                        break;
                    case ProcessInfo.title:
                        window.Add(WindowProcessor.GetWindowText(this.hWnd));
                        break;
                    case ProcessInfo.timeRegistered:
                        window.Add(this.timeRegistered.ToString());
                        break;
                    default:
                        break;
                }
            }
            return window;
        }
    }
}