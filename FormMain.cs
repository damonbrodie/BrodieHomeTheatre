﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Google.Cloud.TextToSpeech.V1;
using GoogleCast;
using GoogleCast.Models.Media;
using GoogleCast.Channels;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        /* My Insteon addresses
           42.22.B8 Pot
           42.20.F8 Tray
           41.66.88 Motion Sensor
           41.58.FC Door Sensor
           47.01.AC Exhaust Fan Switch

          Mapped keypresses
           F12 - Lights to Entering level
           F11 - Lights Off
           F9  - Lights to Stopped level
           F7  - Lights to Playback level
           F6  - Projector Lens Kodi Menu (Not captured by App)
           F5  - Projector Lens to Narrow aspect ratio
           F4  - Projector Lens to Wide aspect ratio
           F3  - Kodi next audio languuage (Not captured by App)
        */

        static FormMain formMain;
        public DateTime globalShutdown;
        public bool globalShutdownActive = false;
        public bool globalShutdownWarning = false;
        public int statusTickCounter = 0;
        public Random random = new Random();
        public bool vacancyWarning = false;
        private SimpleHTTPServer simpleHTTPServer;
        Grpc.Core.Channel googleCloudChannel;

        private IReceiver googleHomeReceiver;

        private string localIP;

        static public Dictionary<string, MemoryStream> textToSpeechFiles = new Dictionary<string, MemoryStream>();

        public bool debugInsteon = false;
        public bool debugHarmony = false;

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();
            Logging.writeLog("------ Brodie Theatre Starting Up ------");
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

            var receivers = await new DeviceLocator().FindReceiversAsync();

            foreach (var receiver in receivers)
            {
                if (receiver.FriendlyName == Properties.Settings.Default.SmartSpeaker)
                {
                    googleHomeReceiver = receiver;
                }
            }
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            if (File.Exists(Properties.Settings.Default.googleCloudCredentialsJSON))
            {
                try
                {
                    GoogleCredential credential = GoogleCredential.FromFile(Properties.Settings.Default.googleCloudCredentialsJSON);

                    googleCloudChannel = new Grpc.Core.Channel(TextToSpeechClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
                }
                catch (Exception ex)
                {
                    Logging.writeLog("Error:  Unable to load Google Credentials for Cloud - Text-To-Speech:  " + ex.ToString());
                }
            }

            localIP = GetLocalIPAddress();

            var receivers = await new DeviceLocator().FindReceiversAsync();

            foreach (var receiver in receivers)
            {
                if (receiver.FriendlyName == Properties.Settings.Default.SmartSpeaker)
                {
                    googleHomeReceiver = receiver;
                }
            }

                    if (Properties.Settings.Default.webServerPort > 0 && Properties.Settings.Default.webServerPort <= 65535)
            formMain.simpleHTTPServer = new SimpleHTTPServer(Properties.Settings.Default.webServerPort);

            formMain.BeginInvoke(new Action(() =>
            {
                formMain.timerSetLights.Enabled = true;
                formMain.resetGlobalTimer();
            }));

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

                formMain.fanPowerOff();
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
            try
            {
                simpleHTTPServer.Stop();
            }
            catch { };

            UnhookWindowsHookEx(hookID);
            Logging.writeLog("------ Brodie Theatre Shutting Down ------");
        }

        private void timerGlobal_Tick(object sender, EventArgs e)
        {
            /* Reasons the timer should be ticking down if either of these is TRUE
               - A light is on
               - A Harmony Activity is active, but playback is not playing

               The timer should not be active if the following are all TRUE:
               - The lights are Off
               - The Harmony Activity is Off
               - The Room is vacant
            */
            DateTime now = DateTime.Now;
            DateTime globalShutdownStart = globalShutdown.AddMinutes(Properties.Settings.Default.globalShutdown * -1);
            var totalSeconds = (globalShutdown - globalShutdownStart).TotalSeconds;
            var progress = (now - globalShutdownStart).TotalSeconds;

            if ((harmonyIsActivityStarted() || trackBarPots.Value > 0 || trackBarTray.Value > 0) && labelKodiPlaybackStatus.Text != "Playing")
            {
                if (globalShutdown.AddMinutes(-1) <= now && ! globalShutdownWarning)
                {
                    Logging.writeLog("Global Timer:  One minute warning for global shutdown");
                    //speakText(ttsWarningPhrases[r]);           
                    globalShutdownWarning = true;
                    return;
                }
                else if (globalShutdown > now)
                {
                    int percentage = Math.Abs(100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                    toolStripProgressBarGlobal.Value = percentage;
                    if (! globalShutdownActive)
                    {
                        Logging.writeLog("Global Timer:  Timer active");
                    }
                    globalShutdownActive = true;
                    return;
                }
                else
                {
                    Logging.writeLog("Global Timer:  Shutting down theatre");
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
                Logging.writeLog("Global Timer:  Disabling timer");
                globalShutdownActive = false;
                if (labelRoomOccupancy.Text != "Vacant")
                {
                    labelRoomOccupancy.Text = "Vacant";
                }
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
                Logging.writeLog("Occupancy:  Room Occupied");
                resetGlobalTimer();

                if (!harmonyIsActivityStarted() && labelKodiPlaybackStatus.Text == "Stopped")
                {
                    lightsToEnteringLevel();
                    kodiUpdateLibrary();
                }

                toolStripStatus.Text = "Room is now occupied";
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                if (labelKodiPlaybackStatus.Text == "Stopped")
                {
                    if (harmonyIsActivityStarted())
                    {
                        // Turn off active Harmony Activity
                        harmonyStartActivityByName("PowerOff");
                    }

                    Logging.writeLog("Occupancy:  Room vacant");
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
            globalShutdown = DateTime.Now.AddMinutes(Properties.Settings.Default.globalShutdown);
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
                Logging.writeLog("Occupancy:  Overriding Room to Vacant");
                insteonMotionLatchActive = false;
            }
            else
            {
                labelRoomOccupancy.Text = "Occupied";
                insteonDoMotion(false);
                Logging.writeLog("Occupancy:  Overriding Room to Occupied");
            }
        }
    }
}
