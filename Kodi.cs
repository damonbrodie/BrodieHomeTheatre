using System;
using System.Windows.Forms;
using System.IO;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void timerKodiPoller_Tick(object sender, EventArgs e)
        {
            if (File.Exists("kodi_ar.txt"))
            {
                string kodiAspectRatio = File.ReadAllText("kodi_ar.txt").Trim().ToLower();
                File.Delete("kodi_ar.txt");
                projectorQueueChangeAspect(float.Parse(kodiAspectRatio));
            }
            if (File.Exists("kodi_status.txt"))
            {
                string kodiPlayback = File.ReadAllText("kodi_status.txt").Trim().ToLower();
                File.Delete("kodi_status.txt");
                switch (kodiPlayback)
                {
                    case "playing":
                        labelKodiStatus.Text = "Playing";
                        toolStripStatus.Text = "Setting lights to Playback Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPlaybackLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPlaybackLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;

                        resetGlobalTimer();

                        break;
                    case "stopped":
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;

                        resetGlobalTimer();

                        break;
                    case "paused":
                        labelKodiStatus.Text = "Paused";
                        toolStripStatus.Text = "Setting lights to Paused Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPausedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPausedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;

                        resetGlobalTimer();

                        break;
                    default:
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;

                        resetGlobalTimer();

                        break;
                }
            }
        }
    }
}