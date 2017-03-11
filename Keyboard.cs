using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeysConverter kc = new KeysConverter();
                string keyCode = kc.ConvertToString(vkCode);
                switch (keyCode)
                {
                    case "F12":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.lightsToEnteringLevel();
                        }
                        ));
                        break;
                    case "F11":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.lightsOff();
                        }
                        ));
                        break;
                    case "F9":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.lightsToStoppedLevel();
                        }
                        ));
                        break;
                    case "F7":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.lightsToPlaybackLevel();
                        }
                        ));
                        break;
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}