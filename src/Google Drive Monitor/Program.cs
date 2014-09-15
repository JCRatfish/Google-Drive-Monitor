using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using JCS;

namespace Google_Drive_Monitor
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            /* 
             * Current Google Drive Version: 1.17.7290.4094
             * 
             * Check if Google Drive is currently running.
             * Google Drive (googledrivesync.exe) runs in two separate processes for some unknown reason.
             * If both of those processes are not running, kill and restart Google Drive.
            */

            // Hide the console window.
            ShowWindow(GetConsoleWindow(), 0);

            // Check if Google Drive is installed.
            if (ErrorMessages.Any(GoogleDriveInstallLocation.Contains))
            {
                MessageBox.Show(GoogleDriveInstallLocation);
                Environment.Exit(0);
            }

            // Get file name without extension.
            FileInfo googledrivesync = new FileInfo(Path.GetFileNameWithoutExtension(GoogleDriveInstallLocation));

            // Continuously check if Google Drive is running.
            while (true)
            {
                // Collect the Google Drive processes.
                Process[] googledrivesyncProcesses = Process.GetProcessesByName(googledrivesync.ToString());

                // If there are less than two (2) "googledrivesync.exe" processes running, restart Google Drive.
                if (googledrivesyncProcesses.Length < 2)
                {
                    // Kill each "googledrivesync.exe".
                    foreach (Process p in googledrivesyncProcesses)
                    {
                        p.Kill();
                        p.WaitForExit();
                    }

                    // Start Google Drive.
                    try
                    {
                        Process.Start(GoogleDriveInstallLocation);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Google Drive Monitor");
                    }
                }

                // Wait 60 seconds before checking again.
                Thread.Sleep(60000);
            }
        }

        // Fetch Google Drive's install location from the registry.
        static string GoogleDriveInstallLocation
        {
            get
            {
                switch (OSVersionInfo.Name)
                {
                    case "Windows XP":
                        using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Google\Drive"))
                        {
                            if (registryKey != null)
                            {
                                return (string)registryKey.GetValue("InstallLocation");
                            }
                            else
                            {
                                return ErrorMessages[1];
                            }
                        }
                    case "Windows 7":
                        using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Google\Drive"))
                        {
                            if (registryKey != null)
                            {
                                return (string)registryKey.GetValue("InstallLocation");
                            }
                            else
                            {
                                return ErrorMessages[1];
                            }
                        }
                    default:
                        return ErrorMessages[0];
                }
            }
        }

        static string[] ErrorMessages
        {
            get
            {
                string[] errorMessages = new string[] { "Operating System Not Supported!", "Google Drive Not Installed!" };
                return errorMessages;
            }
        }
    }
}
