using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        //42.22.B8 Pot
        //42.20.F8 Tray
        //41.66.88 Motion Sensor

        static FormMain formMain;
        public DateTime GlobalShutdown;
        public int statusTickCounter = 0;
        public Random random = new Random();
        
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
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.writeLog("------ Starting Up ------");
            }
            ));
            await harmonyConnectAsync(true);
            if (Program.Client != null)
            {
                Program.Client.OnActivityChanged += harmonyClient_OnActivityChanged;
            }

            currentPLMport = Properties.Settings.Default.plmPort;
            insteonConnectPLM();

            projectorConnect();

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

            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
            if (Program.Client.Token != string.Empty)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.labelHarmonyStatus.Text = "Connected";
                    formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.ForestGreen;
                }
                ));
            }
            else
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.labelHarmonyStatus.Text = "Disconnected";
                    formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.Maroon;
                }
                ));
            }

            formMain.BeginInvoke(new Action(() =>
            {
                formMain.timerSetLights.Enabled = true;
                formMain.recognitionEngine = new SpeechRecognitionEngine();
                try
                {
                    formMain.recognitionEngine.SetInputToDefaultAudioDevice();
                }
                catch
                {
                    formMain.writeLog("Voice:  Unable to attach to default audio input device");
                }
                formMain.recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
                Task task = Task.Run((Action)loadVoiceCommands);
            }
            ));
           
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
            statusTickCounter = 2;       
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

            if (( harmonyIsActivityStarted() || trackBarPots.Value > 0 || trackBarTray.Value > 0) && labelRoomOccupancy.Text == "Vacant")
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
                    harmonyStartActivityByName("PowerOff");
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

                if (! harmonyIsActivityStarted() && labelKodiPlaybackStatus.Text == "Stopped")
                {
                    lightsToEnteringLevel();
                    playSound(startupWave);
                }
                
                try
                {
                    recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
                    labelLastVoiceCommand.Text = "Listening";
                }
                catch
                {
                    labelLastVoiceCommand.Text = "Grammar Not Loaded";
                }
                
                toolStripStatus.Text = "Room is now occupied";
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {     
                try
                {
                    recognitionEngine.RecognizeAsyncStop();
                    labelLastVoiceCommand.Text = "Not Listening";
                }
                catch
                {
                    writeLog("Voice:  Failed to pause Recognition Engine");
                }

                if (labelKodiPlaybackStatus.Text == "Stopped")
                {
                    if (harmonyIsActivityStarted())
                    {
                        // Turn off active Harmony Activity
                        harmonyStartActivityByName("PowerOff");
                    }

                    writeLog("Occupancy:  Room Vacant");
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

        private void timerKodiConnect_Tick(object sender, EventArgs e)
        {
            kodiConnect();       
        }

        private void timerKodiStartPlayback_Tick(object sender, EventArgs e)
        {
            timerKodiStartPlayback.Enabled = false;
            if (kodiPlayNext != null)
            {
                kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\": {\"file\": \"" + kodiPlayNext.file + "\" }}, \"id\": \"1\"}");
                writeLog("Kodi:  Starting movie: " + kodiPlayNext.name + " " + kodiPlayNext.file);
                kodiPlayNext = null;
            }
        }
    }
}