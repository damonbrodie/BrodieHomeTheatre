using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SoapBox.FluentDwelling;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
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

        //42.22.B8 Pot
        //42.20.F8 Tray
        //41.66.88 Motion Sensor

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
            await ConnectHarmonyAsync();
            Program.Client.OnActivityChanged += harmonyClient_OnActivityChanged;

            currentPLMport = Properties.Settings.Default.plmPort;
            connectPLM();

            ConnectProjector();

            if (projectorConnected)
            {
                checkProjectorPower();
            }

            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
            recognitionEngine.SetInputToDefaultAudioDevice();
            //recognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
            //recognitionEngine.EndSilenceTimeout = TimeSpan.FromSeconds(1.5);
            //recognitionEngine.BabbleTimeout = TimeSpan.FromSeconds(5);

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






 





        private void trackBarTray_ValueChanged(object sender, EventArgs e)
        {
            labelTray.Text = (trackBarTray.Value * 10).ToString() + "%";
        }

        private void trackBarPots_ValueChanged(object sender, EventArgs e)
        {
            labelPots.Text = (trackBarPots.Value * 10).ToString() + "%";
        }

        private void timerPotTrack_Tick(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            setLightLevel(Properties.Settings.Default.potsAddress, trackBarPots.Value);
        }

        private void timerTrayTrack_Tick(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            setLightLevel(Properties.Settings.Default.trayAddress, trackBarTray.Value);
        }

        private void trackBarTray_Scroll(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            timerTrayTrack.Enabled = true;
        }

        private void trackBarPots_Scroll(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            timerPotTrack.Enabled = true;
        }





        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
            //timerClearStatus.Enabled = false;
        }

        private void timerShutdown_Tick(object sender, EventArgs e)
        {
            timerShutdown.Enabled = false;
            toolStripStatus.Text = "Turning off lights";
            setLightLevel(Properties.Settings.Default.potsAddress, 0);
            trackBarPots.Value = 0;
            setLightLevel(Properties.Settings.Default.trayAddress, 0);
            trackBarTray.Value = 0;
        }

        private void toolStripStatus_TextChanged(object sender, EventArgs e)
        {
            timerClearStatus.Enabled = false;
            timerClearStatus.Enabled = true;
        }

        private void timerStartLights_Tick(object sender, EventArgs e)
        {
            timerStartLights.Enabled = false;
            toolStripStatus.Text = "Setting lights to Stopped Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
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

        }

        /*
         * This timer runs when ever:
         *   - The lights get turned on
         *   - A Harmony Activity is enabled
         */

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
                    resetGlobalTimer();
                    timerShutdown.Enabled = false;
                    toolStripStatus.Text = "Room is now occupied";

                    // Power on the Amplifier
                    harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyEnterCommand);
                }
                ));


                if (labelCurrentActivity.Text == "PowerOff")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        toolStripStatus.Text = "Turning on lights";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;

                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                    }
                    ));
                }
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
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
                            harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyExitCommand);
                        }
                        ));
                    }

                    formMain.BeginInvoke(new Action(() =>
                    {
                        toolStripStatus.Text = "Room is now vacated";
                        toolStripStatus.Text = "Turning off lights";
                        setLightLevel(Properties.Settings.Default.potsAddress, 0);
                        trackBarPots.Value = 0;
                        setLightLevel(Properties.Settings.Default.trayAddress, 0);
                        trackBarTray.Value = 0;
                    }
                    ));
                }
                else // There is playback or it is paused.  Start the timer to shut this off after 2 hours
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        resetGlobalTimer();
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





        private void labelMotionSensorStatus_TextChanged(object sender, EventArgs e)
        {
            if (labelMotionSensorStatus.Text == "Motion Detected")
            {
                disableGlobalShutdown();
            }
            else
            {
                resetGlobalTimer();
            }
        }

        private void disableGlobalShutdown()
        {
            globalShutdownActive = false;
        }

        private void sayGreeting()
        {
            int currHour = DateTime.Now.Hour;
            string timeGreeting;
            if (currHour >= 5 && currHour <= 11)
            {
                timeGreeting = "good morning";
            }
            else if (currHour >= 12 && currHour <=17)
            {
                timeGreeting = "good afternoon";
            }
            else
            {
                timeGreeting = "good evening";
            }
            List <string> greetings = new List<string>(new string[] { timeGreeting, "hello there", "how can I help you" });
            Random rnd = new Random();
            int r = rnd.Next(greetings.Count);
            speakText(greetings[r]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            labelRoomOccupancy.Text = "Occupied";
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
