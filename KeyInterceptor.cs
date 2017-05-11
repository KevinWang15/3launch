using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace _3launch
{
    static class KeyInterceptor
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private const int KEYEVENTF_KEYUP = 0x2;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static bool otherKeyPressed = false;
        public static volatile MainWindow mainWindow = null;

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                WinForms.Keys key = (WinForms.Keys) vkCode;

                if (key == System.Windows.Forms.Keys.LWin)
                {
                    if ((int) wParam == WM_KEYDOWN)
                        otherKeyPressed = false;

                    if ((int) wParam == WM_KEYUP)
                    {
                        if (otherKeyPressed)
                        {
                            // A win key hotkey, do nothing
                        }
                        else
                        {
                            // send some rubbish keystrokes to prevent start menu from popping up 
                            keybd_event((int) System.Windows.Forms.Keys.LControlKey, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LShiftKey, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LMenu, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LWin, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event((int) System.Windows.Forms.Keys.None, 0x45, KEYEVENTF_EXTENDEDKEY, 0);
                            keybd_event((int) System.Windows.Forms.Keys.None, 0x45,
                                KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LWin, 0x45,
                                KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LMenu, 0x45,
                                KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LShiftKey, 0x45,
                                KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                            keybd_event((int) System.Windows.Forms.Keys.LControlKey, 0x45,
                                KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                            if (mainWindow.Visibility == Visibility.Visible)
                            {
                                mainWindow.clearAndHide();
                            }
                            else
                            {
                                mainWindow.Show();
                                mainWindow.Activate();
                            }
                            return (IntPtr) 1;
                        }
                    }
                }
                else
                {
                    otherKeyPressed = true;
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }


        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private static void DetachKeyboardHook()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        public static void start()
        {
            try
            {
                _hookID = InterceptKeys.SetHook(_proc);
            }
            catch
            {
                DetachKeyboardHook();
            }
        }
    }

    internal class InterceptKeys
    {
        #region Delegates

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private const int WH_KEYBOARD_LL = 13;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}