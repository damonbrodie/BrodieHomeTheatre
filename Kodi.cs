using System;
using System.Windows.Forms;
using System.IO;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void timerKodiPoller_Tick(object sender, EventArgs e)
        {
            if (File.Exists("kodi_status.txt"))
            {
                string kodiPlayback = File.ReadAllText("kodi_status.txt").Trim().ToLower();
                File.Delete("kodi_status.txt");
                switch (kodiPlayback)
                {
                    case "playing":
                        writeLog("Kodi:  Kodi status changed to 'Playing'");
                        labelKodiStatus.Text = "Playing";
                        lightsToPlaybackLevel(); 
                        resetGlobalTimer();
                        break;
                    case "stopped":
                        writeLog("Kodi:  Kodi status changed to 'Stopped'");
                        labelKodiStatus.Text = "Stopped";
                        lightsToStoppedLevel();
                        resetGlobalTimer();
                        break;
                    case "paused":
                        writeLog("Kodi:  Kodi status changed to 'Paused'");
                        labelKodiStatus.Text = "Paused";
                        lightsToPausedLevel();
                        resetGlobalTimer();
                        break;
                    default:
                        writeLog("Kodi:  Unknown Kodi status - assuming 'Stopped'");
                        labelKodiStatus.Text = "Stopped";
                        lightsToStoppedLevel();
                        resetGlobalTimer();
                        break;
                }
            }
            if (File.Exists("kodi_ar.txt"))
            {
                string kodiAspectRatio = File.ReadAllText("kodi_ar.txt").Trim().ToLower();
                File.Delete("kodi_ar.txt");
                projectorQueueChangeAspect(float.Parse(kodiAspectRatio));
            }
        }
    }
}