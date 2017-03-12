using System;
using System.Windows.Forms;
using SoapBox.FluentDwelling;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;
using System.Collections.Generic;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        //42.22.B8 Pot
        //42.20.F8 Tray
        //41.66.88 Motion Sensor

        static FormMain formMain;
        public class Activities
        {
            public string Text;
            public string Id;

            public override string ToString()
            {
                return Text;
            }
        }

        Plm powerlineModem;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc proc = HookCallback;
        public static IntPtr hookID = IntPtr.Zero;

        public string currentHarmonyIP;
        public string currentPLMport;
        public bool plmConnected;

        public DateTime GlobalShutdown;
        public bool globalShutdownActive;

        public bool projectorConnected;
        public string projectorLastCommand;

        public float projectorNewAspect = 0;

        public SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        public SpeechRecognitionEngine recognitionEngine = new SpeechRecognitionEngine();

        Dictionary<string, int> lights = new Dictionary<string, int>();      

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();
            resetGlobalTimer();
            projectorConnected = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form formSettings = new FormSettings();
            formSettings.ShowDialog();

            if (currentPLMport != Properties.Settings.Default.plmPort)
            {
                currentPLMport = Properties.Settings.Default.plmPort;
                connectPLM();
            }

            if (currentHarmonyIP != Properties.Settings.Default.harmonyHubIP)
            {
                currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
                await ConnectHarmonyAsync();
            }
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            writeLog("------ Starting Up ------");
            await ConnectHarmonyAsync();
            Program.Client.OnActivityChanged += harmonyClient_OnActivityChanged;

            currentPLMport = Properties.Settings.Default.plmPort;
            connectPLM();

            ConnectProjector();

            if (projectorConnected)
            {
                checkProjectorPower();
            }

            if (Properties.Settings.Default.potsAddress != "")
            {
                lights[Properties.Settings.Default.potsAddress] = -1;
            }

            if (Properties.Settings.Default.trayAddress != "")
            {
                lights[Properties.Settings.Default.trayAddress] = -1;
            }
            timerSetLights.Enabled = true;

            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
            recognitionEngine.SetInputToDefaultAudioDevice();

            loadVoiceCommands();
            recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;

            recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
            if (Program.Client.Token != "")
            {
                formMain.labelHarmonyStatus.Text = "Connected";
                formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }
            else
            {
                formMain.labelHarmonyStatus.Text = "Disconnected";
                formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.Maroon;
            }

            speechSynthesizer.Volume = 100;
            speechSynthesizer.Rate = 2;
            speechSynthesizer.SetOutputToDefaultAudioDevice();     
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
            //timerClearStatus.Enabled = false;
        }

        private void timerShutdown_Tick(object sender, EventArgs e)
        {
            timerShutdown.Enabled = false;
            lightsOff();
        }

        private void toolStripStatus_TextChanged(object sender, EventArgs e)
        {
            timerClearStatus.Enabled = false;
            timerClearStatus.Enabled = true;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Program.Client.Dispose();
            }
            catch { };
            if (powerlineModem != null)
            {
                powerlineModem.Dispose();
            }
            UnhookWindowsHookEx(hookID);
            writeLog("------ Shutting Down ------");
        }

        private void timerGlobal_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime globalShutdownStart = GlobalShutdown.AddHours(Properties.Settings.Default.globalShutdown * -1);
            var totalSeconds = (GlobalShutdown - globalShutdownStart).TotalSeconds;
            var progress = (now - globalShutdownStart).TotalSeconds;

            if ((labelCurrentActivity.Text != "PowerOff" && labelCurrentActivity.Text != "") || trackBarPots.Value > 0 || trackBarTray.Value > 0)
            {
                if (globalShutdownActive && GlobalShutdown > now)

                {
                    int percentage = (100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                    if (percentage <= 1)
                    {
                        globalShutdownActive = false;
                        startActivityByName("PowerOff");
                        toolStripProgressBarGlobal.Value = 0;
                    }
                    else
                    {
                        toolStripProgressBarGlobal.Value = percentage;
                    }
                }
                else
                {
                    toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Maximum;
                }
            }
            else
            {
                toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Minimum;
            }
        }

        private async void labelRoomOccupancy_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    writeLog("Occupancy Changed: Room Occupied");
                    formMain.resetGlobalTimer();
                    formMain.timerShutdown.Enabled = false;
                    formMain.toolStripStatus.Text = "Room is now occupied";

                    // Power on the Amplifier
                    formMain.harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyEnterCommand);
                }
                ));


                if (labelCurrentActivity.Text == "PowerOff")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.lightsToEnteringLevel();
                    }
                    ));
                }
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    writeLog("Occupancy Changed: Room Occupied");
                }
                ));
                if (labelKodiStatus.Text == "Stopped")
                {
                    if (labelCurrentActivity.Text != "PowerOff")
                    {
                        // Turn off active Harmony Activity
                        await Program.Client.StartActivityAsync("-1");
                    }
                    else
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            // Power off the Amplifier
                            formMain.harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyExitCommand);
                        }
                        ));
                    }

                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Room is now vacant";
                        formMain.lightsOff();
                    }
                    ));
                }
                else // There is playback or it is paused.  Start the timer to shut this off after 2 hours
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.resetGlobalTimer();
                    }
                    ));
                }
            }
        }

        private void resetGlobalTimer()
        {
            GlobalShutdown = DateTime.Now.AddHours(Properties.Settings.Default.globalShutdown);
            globalShutdownActive = true;
            timerGlobal.Enabled = false;
            timerGlobal.Interval = 1000;
            timerGlobal.Enabled = true;
        }

        private void disableGlobalShutdown()
        {
            globalShutdownActive = false;
        }

        private void labelProjectorPower_TextChanged(object sender, EventArgs e)
        {
            if (labelProjectorPower.Text == "On")
            {
                buttonProjectorChangeAspect.Enabled = true;
            }
            else
            {
                buttonProjectorChangeAspect.Enabled = false;
            }
        }
    }
}
