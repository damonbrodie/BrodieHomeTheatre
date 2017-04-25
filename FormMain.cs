using System;
using System.Windows.Forms;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        //42.22.B8 Pot
        //42.20.F8 Tray
        //41.66.88 Motion Sensor
        //41.58.FC Door Sensor

        // Projector on COM3
        // Insteon on COM1

        static FormMain formMain;
        public DateTime GlobalShutdown;
        public int statusTickCounter = 0;
        public Random random = new Random();
        public bool vacancyWarning = false;

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();

            if (Properties.Settings.Default.startMinimized)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }

            resetGlobalTimer();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form formSettings = new FormSettings();
            formSettings.ShowDialog();

            // Reset things after the settings have been saved.
            if (currentPLMport != Properties.Settings.Default.plmPort)
            {
                currentPLMport = Properties.Settings.Default.plmPort;
                insteonConnectPLM();
            }

            if (currentHarmonyIP != Properties.Settings.Default.harmonyHubIP)
            {
                currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
                await harmonyConnectAsync();
            }

            if (currentKodiIP != Properties.Settings.Default.kodiIP || currentKodiPort != Properties.Settings.Default.kodiJSONPort)
            {
                currentKodiIP = Properties.Settings.Default.kodiIP;
                currentKodiPort = Properties.Settings.Default.kodiJSONPort;
                kodiStatusDisconnect();
            }

            setVoice();
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.writeLog("------ Brodie Theatre Starting Up ------");
                formMain.timerSetLights.Enabled = true;
                RecognizerInfo info = null;
                foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers())
                {
                    if (ri.Culture.TwoLetterISOLanguageName.Equals("en"))
                    {
                        info = ri;
                        break;
                    }
                }
                if (info != null)
                {
                    formMain.recognitionEngine = new SpeechRecognitionEngine(info);
                    formMain.recognitionEngine.SetInputToDefaultAudioDevice();
                    formMain.recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
                }
                else
                {
                    formMain.writeLog("Voice:  Unable to load Speech Recognition Engine.  Shutting Down.");
                    Application.Exit();
                }
            }));

            currentPLMport = Properties.Settings.Default.plmPort;
            insteonConnectPLM();
            projectorConnect();

            formMain.BeginInvoke(new Action(() =>
            {
                formMain.speechSynthesizer = new SpeechSynthesizer();
                formMain.speechSynthesizer.TtsVolume = 100;
                formMain.setVoice();
            }));

            if (labelProjectorStatus.Text == "Connected")
            {
                projectorCheckPower();
            }

            if (Properties.Settings.Default.potsAddress != string.Empty)
            {
                lights[Properties.Settings.Default.potsAddress] = -1;
            }

            if (Properties.Settings.Default.trayAddress != string.Empty)
            {
                lights[Properties.Settings.Default.trayAddress] = -1;
            }

            await harmonyConnectAsync(true);
            if (Program.Client != null)
            {
                Program.Client.OnActivityChanged += harmonyClient_OnActivityChanged;
            }
            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
            if (Program.Client!= null && Program.Client.Token != string.Empty)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.labelHarmonyStatus.Text = "Connected";
                    formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.ForestGreen;
                }));
            }
            else
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.labelHarmonyStatus.Text = "Disconnected";
                    formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.Maroon;
                }));
            }
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            if (statusTickCounter > 0)
            {
                statusTickCounter -= 1;
            }
            else
            {
                toolStripStatus.Text = "";
            }
        }

        private void toolStripStatus_TextChanged(object sender, EventArgs e)
        {
            if (toolStripStatus.Text != string.Empty)
            {
                statusTickCounter = 2;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (Program.Client != null)
                {
                    Program.Client.Dispose();
                }
            }
            catch { };
            if (powerlineModem != null)
            {
                powerlineModem.Dispose();
            }
            UnhookWindowsHookEx(hookID);
            writeLog("------ Brodie Theatre Shutting Down ------");
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

            if ((harmonyIsActivityStarted() || trackBarPots.Value > 0 || trackBarTray.Value > 0) && labelRoomOccupancy.Text == "Vacant")
            {
                if (GlobalShutdown > now)
                {
                    int percentage = Math.Abs(100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                    toolStripProgressBarGlobal.Value = percentage;
                    return;
                }
                else
                {
                    writeLog("Global Timer:  Sending Harmony 'PowerOff', turning off lights");
                    if (harmonyIsActivityStarted())
                    {
                        harmonyStartActivityByName("PowerOff");
                    }
                    toolStripProgressBarGlobal.Value = 0;
                    lightsOff();
                    return;
                }
            }
            toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Minimum;
        }

        private void listBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            harmonyStartActivity(activity.Text, activity.Id);
        }

        private void labelRoomOccupancy_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                writeLog("Occupancy:  Room Occupied");
                resetGlobalTimer();

                if (!harmonyIsActivityStarted() && labelKodiPlaybackStatus.Text == "Stopped")
                {
                    lightsToEnteringLevel();
                }
                try
                {
                    recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    labelListeningStatus.Text = "Listening";
                }
                catch
                {
                    labelLastVoiceCommand.Text = "Can't turn speech recognizer on";
                    writeLog("Voice:  Can't start Speech Recognizer");
                }

                toolStripStatus.Text = "Room is now occupied";
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                try
                {
                    recognitionEngine.RecognizeAsyncStop();
                    labelListeningStatus.Text = "Stopped listening";
                }
                catch
                {
                    writeLog("Voice:  Failed to pause recognition engine");
                }

                if (labelKodiPlaybackStatus.Text == "Stopped")
                {
                    if (harmonyIsActivityStarted())
                    {
                        // Turn off active Harmony Activity
                        harmonyStartActivityByName("PowerOff");
                    }

                    writeLog("Occupancy:  Room vacant");
                    toolStripStatus.Text = "Room is now vacant";
                    lightsOff();
                }
                else // There is playback or it is paused.  Start the timer to shut this off after configured time
                {
                    resetGlobalTimer();
                }
            }
        }

        private void resetGlobalTimer()
        {
            GlobalShutdown = DateTime.Now.AddHours(Properties.Settings.Default.globalShutdown);
            writeLog("Global Timer:  Resetting shutdown timer");
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
                writeLog("Occupancy:  Overriding Room to Vacant");
                insteonMotionLatchActive = false;
            }
            else
            {
                labelRoomOccupancy.Text = "Occupied";
                insteonDoMotion();
                writeLog("Occupancy:  Overriding Room to Occupied");
            }
        }
    }
}