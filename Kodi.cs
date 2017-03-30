using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string kodiWaveFile = @"c:\Users\damon\Documents\Shared\wavefile.txt";
        public string kodiPlaybackFile = @"c:\Users\damon\Documents\Shared\kodiplayback.txt";
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

        private void kodiPlayWave(string file)
        {
            bool success = false;
            int counter = 0;
            while (!success && counter < 3)
            {
                try
                {
                    StreamWriter fileHandle = new StreamWriter(kodiWaveFile);
                    fileHandle.WriteLine(Path.Combine(wavePath, file));
                    fileHandle.Close();
                    success = true;
                }
                catch
                {
                    Thread.Sleep(50);
                    counter += 1;
                }
            }
        }

        private void kodiPlaybackControl(string command, string media=null)
        {
            bool success = false;
            int counter = 0;
            while (!success && counter < 3)
            {
                try
                {
                    StreamWriter fileHandle = new StreamWriter(kodiPlaybackFile);
                    fileHandle.WriteLine(command + "|" + media);
                    fileHandle.Close();
                    success = true;
                }
                catch
                {
                    Thread.Sleep(50);
                    counter += 1;
                }
            }
        }
    }
}