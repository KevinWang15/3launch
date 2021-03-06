﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Microsoft.Win32.TaskScheduler;

namespace Configurator
{
    public partial class Configurator : Form
    {
        public bool isRunning()
        {
            Process[] pname = Process.GetProcessesByName("3launch");
            return pname.Length > 0;
        }

        public bool isStartOnBoot()
        {
            using (TaskService ts = new TaskService())
            {
                Microsoft.Win32.TaskScheduler.Task task = ts.FindTask("3launch", false);
                return task != null;
            }
        }

        public void setLayout()
        {
            if (running_button.Enabled)
            {
                if (isRunning())
                {
                    this.running_text.Text = "Running: Yes";
                    this.running_button.Text = "Stop";
                }
                else
                {
                    this.running_text.Text = "Running: No";
                    this.running_button.Text = "Run";
                }
            }
            if (isStartOnBoot())
            {
                this.start_on_boot_text.Text = "Start on boot: Yes";
                this.start_on_boot_button.Text = "Disable";
            }
            else
            {
                this.start_on_boot_text.Text = "Start on boot: No";
                this.start_on_boot_button.Text = "Enable";
            }
        }

        public Configurator()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Configurator_Load(object sender, EventArgs e)
        {
            setLayout();
        }

        private void running_button_Click(object sender, EventArgs e)
        {
            running_button.Enabled = false;
            if (isRunning())
            {
                Process[] processes = Process.GetProcessesByName("3launch");
                for (int i = 0; i < processes.Length; i++)
                {
                    try
                    {
                        processes[i].Kill();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to terminate 3launch.exe", "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                if (!File.Exists("3launch.exe"))
                {
                    MessageBox.Show("3launch.exe not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    running_button.Enabled = true;
                    return;
                }

                MessageBox.Show("Please double click 3launch.exe and launch it from Windows explorer.", "Please launch from Windows explorer", MessageBoxButtons.OK, MessageBoxIcon.Information);

                /*
                try
                {
                    RunAsDesktopUser.run("3launch.exe");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Process start failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    running_button.Enabled = true;
                    return;
                }
                */
            }
            new Thread(() =>
            {
                Thread.Sleep(1000);
                this.Invoke((MethodInvoker) delegate() { running_button.Enabled = true; });
                setLayout();
            }).Start();
        }

        private void layout_refresher_Tick(object sender, EventArgs e)
        {
            setLayout();
        }

        private void start_on_boot_button_Click(object sender, EventArgs e)
        {
            if (isStartOnBoot())
            {
                using (TaskService ts = new TaskService())
                {
                    Microsoft.Win32.TaskScheduler.Task task = ts.FindTask("3launch");
                    if (task != null)
                    {
                        ts.RootFolder.DeleteTask("3launch");
                    }
                }
            }
            else
            {
                var exePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                    "3launch.exe");
                if (!File.Exists(exePath))
                {
                    MessageBox.Show("3launch.exe not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "3launch";
                    td.Triggers.Add(new LogonTrigger());
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                    // td.Principal.RunLevel = TaskRunLevel.Highest;
                    td.Actions.Add(new ExecAction(exePath, "",
                        Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)));
                    ts.RootFolder.RegisterTaskDefinition("3launch", td);
                }
            }
            setLayout();
        }
    }
}