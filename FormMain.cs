using System;
using System.Threading;
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

        public bool projectorConnected;
        public string projectorLastCommand;

        public float projectorNewAspect = 0;

        public List<string> greetingsEvening = new List<string>();
        public List<string> greetingsMorning = new List<string>();
        public List<string> greetingsAfternoon = new List<string>();
        public List<string> greetingsPresense = new List<string>();

        public string wavePath = @"c:\Users\damon\Documents\Shared\Wavs";
        public string waveFile = @"c:\Users\damon\Documents\Shared\wavefile.txt";

        public SpeechRecognitionEngine recognitionEngine;

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

            // XXX Play startup sound


            

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
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
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
            // Evaluate the conditions to determine if we should be counting down or not
            // for shutting down the theatre
            DateTime now = DateTime.Now;

            // Calculate when the shutdown timer was initiated based on when it will end.

            DateTime globalShutdownStart = GlobalShutdown.AddHours(Properties.Settings.Default.globalShutdown * -1);
            var totalSeconds = (GlobalShutdown - globalShutdownStart).TotalSeconds;
            var progress = (now - globalShutdownStart).TotalSeconds;

            // Reasons the timer should be ticking down if either of these is TRUE
            // - A light is on
            // - A Harmony Activity is active

            // The timer should not be active if the following are all TRUE:
            // - The lights are Off
            // - The Harmony Activity is Off
            // - The Room is vacant


            if (((labelCurrentActivity.Text != "PowerOff" && labelCurrentActivity.Text != "") || trackBarPots.Value > 0 || trackBarTray.Value > 0) && labelRoomOccupancy.Text == "Vacant")
            {
                if (GlobalShutdown > now)
                {
                    int percentage = (100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
         
                    toolStripProgressBarGlobal.Value = percentage;
                    return;            
                }
                else
                {    
                    writeLog("Global Timer:  Sending Harmony 'PowerOff', turning off lights");
                    startActivityByName("PowerOff");
                    toolStripProgressBarGlobal.Value = 0;
                    lightsOff();               
                }
                toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Minimum;
            }
        }

        private async void labelRoomOccupancy_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    
                    formMain.writeLog("Occupancy:  Room Occupied");
                    formMain.resetGlobalTimer();

                    if (labelCurrentActivity.Text == "PowerOff" && labelKodiStatus.Text == "Stopped")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.lightsToEnteringLevel();
                            formMain.timerStartupSound.Enabled = true;
                            formMain.writeLog("Occupancy:  Powering On AV Amplifier");
                            formMain.harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyEnterCommand);
                        }
                        ));
                    }

                    if (labelKodiStatus.Text != "Playing")
                    {
                        formMain.recognitionEngine = new SpeechRecognitionEngine();
                        formMain.recognitionEngine.SetInputToDefaultAudioDevice();
                        formMain.loadVoiceCommands();
                        formMain.recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
                        formMain.recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    }
                    

                    formMain.toolStripStatus.Text = "Room is now occupied";
                }
                ));
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Occupancy:  Room Vacant");
                    try
                    {
                        formMain.recognitionEngine.Dispose();
                    }
                    catch
                    {
                        writeLog("Error:  Failed to Dispose of Recognition Engine");
                    }
                }
                ));
                if (labelKodiStatus.Text == "Stopped")
                {
                    if (labelCurrentActivity.Text != "PowerOff")
                    {
                        // Turn off active Harmony Activity
                        try
                        {
                            await Program.Client.StartActivityAsync("-1");
                            writeLog("Harmony:  Sending Poweroff command");
                        }
                        catch
                        {
                            writeLog("Error:  Could not send Harmony PowerOff");
                        }
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
                else // There is playback or it is paused.  Start the timer to shut this off after configured time
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.resetGlobalTimer();
                    }
                    ));
                }
            }
        }

        private void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            writeLog("Speech:  Spoke Words");
        }

        private void resetGlobalTimer()
        {
            GlobalShutdown = DateTime.Now.AddHours(Properties.Settings.Default.globalShutdown);

            writeLog("Global Timer:  Resetting Shutdown timer");
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

        private void labelRoomOccupancy_Click(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                labelRoomOccupancy.Text = "Vacant";
            }
            else
            {
                labelRoomOccupancy.Text = "Occupied";
            }
        }

        private void timerStartupSound_Tick(object sender, EventArgs e)
        {
            timerStartupSound.Enabled = false;
        }
    }
}
