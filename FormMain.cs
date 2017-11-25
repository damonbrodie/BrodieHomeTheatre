using System;
using System.Windows.Forms;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        /* Insteon addresses
           42.22.B8 Pot
           42.20.F8 Tray
           41.66.88 Motion Sensor
           41.58.FC Door Sensor

          Mapped keypresses
           F12 - Lights to Entering level
           F11 - Lights Off
           F9  - Lights to Sopped level
           F7  - Lights to Playback level
           F6  - Projector Lens Kodi Menu (Not captured by App)
           F5  - Projector Lens to Narrow aspect ratio
           F4  - Projector Lens to Wide aspect ratio
           F3  - Kodi next audio languuage (Not captured by App)
           F2  - Toggle Voice playback control
        */

        static FormMain formMain;
        public DateTime globalShutdown;
        public bool globalShutdownActive = false;
        public bool globalShutdownWarning = false;
        public int statusTickCounter = 0;
        public Random random = new Random();
        public bool vacancyWarning = false;

        public bool debugHarmony = true;

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();
            writeLog("------ Brodie Theatre Starting Up ------");
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
                await harmonyConnectAsync(true);
            }

            if (currentKodiIP != Properties.Settings.Default.kodiIP || currentKodiPort != Properties.Settings.Default.kodiJSONPort)
            {
                currentKodiIP = Properties.Settings.Default.kodiIP;
                currentKodiPort = Properties.Settings.Default.kodiJSONPort;
                kodiStatusDisconnect();
            }

            if (Properties.Settings.Default.lightingDelayProjectorOn > 0)
            {
                timerStartLights.Interval = Properties.Settings.Default.lightingDelayProjectorOn * 1000;
            }

            setVoice();
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.timerSetLights.Enabled = true;
                formMain.resetGlobalTimer();
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
            if (Program.Client != null && Program.Client.Token != string.Empty)
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

            formMain.BeginInvoke(new Action(() =>
            {
                if (Properties.Settings.Default.lightingDelayProjectorOn > 0)
                {
                    formMain.timerStartLights.Interval = Properties.Settings.Default.lightingDelayProjectorOn * 1000;
                }
            }));
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
            /* Reasons the timer should be ticking down if either of these is TRUE
               - A light is on
               - A Harmony Activity is active

               The timer should not be active if the following are all TRUE:
               - The lights are Off
               - The Harmony Activity is Off
               - The Room is vacant
            */
            DateTime now = DateTime.Now;
            DateTime globalShutdownStart = globalShutdown.AddHours(Properties.Settings.Default.globalShutdown * -1);
            var totalSeconds = (globalShutdown - globalShutdownStart).TotalSeconds;
            var progress = (now - globalShutdownStart).TotalSeconds;

            if ((harmonyIsActivityStarted() || trackBarPots.Value > 0 || trackBarTray.Value > 0) && labelKodiPlaybackStatus.Text != "Playing")
            {
                if (globalShutdown.AddMinutes(-1) <= now && ! globalShutdownWarning)
                {      
                    writeLog("Global Timer:  One minute warning for global shutdown");
                    int r = random.Next(ttsWarningPhrases.Count);
                    speakText(ttsWarningPhrases[r]);           
                    globalShutdownWarning = true;
                    return;
                }
                else if (globalShutdown > now)
                {
                    int percentage = Math.Abs(100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                    toolStripProgressBarGlobal.Value = percentage;
                    if (! globalShutdownActive)
                    {
                        writeLog("Global Timer:  Timer active");
                    }
                    globalShutdownActive = true;
                    return;
                }
                else
                {
                    writeLog("Global Timer:  Shutting down theatre");
                    globalShutdownActive = false;
                    globalShutdownWarning = false;
                    if (harmonyIsActivityStarted())
                    {
                        harmonyStartActivityByName("PowerOff", false);
                    }
                    toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Minimum;
                    lightsOff();
                    return;
                }
            }
            if (globalShutdownActive)
            {
                writeLog("Global Timer:  Disabling timer");
                globalShutdownActive = false;
            }
            toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Minimum;
        }

        private void listBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            harmonyStartActivity(activity.Text, activity.Id, true);
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
                        recognitionEngine = new SpeechRecognitionEngine(info);
                        recognitionEngine.SetInputToDefaultAudioDevice();
                        recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
                    }
                    else
                    {
                        writeLog("Voice:  No Recognizers found - Do you need to install a Speech Recognition Language (TELE)?");
                        Application.Exit();
                    }
                    loadVoiceCommands();

                    recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    labelListeningStatus.Text = "Listening";
                    if (labelKodiPlaybackStatus.Text == "Stopped")
                    {
                        playAlert();
                    }
                }
                catch (Exception ex)
                {
                    labelLastVoiceCommand.Text = "Cannot enable speech recognizer";
                    writeLog("Voice:  Can't start Speech Recognizer");
                    writeLog("Voice:  ex: " + ex.ToString());
                }

                toolStripStatus.Text = "Room is now occupied";
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                try
                {
                    recognitionEngine.RecognizeAsyncStop();
                    recognitionEngine.UnloadAllGrammars();
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
            globalShutdown = DateTime.Now.AddHours(Properties.Settings.Default.globalShutdown);
            globalShutdownWarning = false;
            //writeLog("Global Timer:  Resetting shutdown timer");
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
                insteonDoMotion(false);
                writeLog("Occupancy:  Overriding Room to Occupied");
            }
        }
    }
}