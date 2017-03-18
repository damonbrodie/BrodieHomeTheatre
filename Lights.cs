using System;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
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
            queueLightLevel(Properties.Settings.Default.potsAddress, trackBarPots.Value * 10);
        }

        private void timerTrayTrack_Tick(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            queueLightLevel(Properties.Settings.Default.trayAddress, trackBarTray.Value * 10);
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

        private void timerStartLights_Tick(object sender, EventArgs e)
        {
            timerStartLights.Enabled = false;
            lightsToStoppedLevel();
        }

        private void lightsToStoppedLevel()
        {
            writeLog("Setting lights to Stopped Level");
            toolStripStatus.Text = "Setting lights to Stopped Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsStoppedLevel);
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayStoppedLevel);
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
        }

        private void lightsOn()
        {
            writeLog("Setting lights to On");
            toolStripStatus.Text = "Turning On Lights";
            queueLightLevel(Properties.Settings.Default.potsAddress, (100));
            trackBarPots.Value = 100;
            queueLightLevel(Properties.Settings.Default.trayAddress, (100));
            trackBarTray.Value = 100;
        }

        private void lightsToEnteringLevel()
        {
            writeLog("Setting lights to Occupancy Level");
            toolStripStatus.Text = "Turning on lights to Occupancy Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsEnteringLevel);
            trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayEnteringLevel);
            trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
        }

        private void lightsOff()
        {
            writeLog("Setting lights Off");
            toolStripStatus.Text = "Turning off lights";
            queueLightLevel(Properties.Settings.Default.potsAddress, 0);
            trackBarPots.Value = 0;
            queueLightLevel(Properties.Settings.Default.trayAddress, 0);
            trackBarTray.Value = 0;
        }

        private void lightsToPlaybackLevel()
        {
            writeLog("Setting lights to Playback Level");
            toolStripStatus.Text = "Dimming lights to Playback Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsPlaybackLevel);
            trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayPlaybackLevel);
            trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;
        }

        private void lightsToPausedLevel()
        {
            writeLog("Setting lights to Paused Level");
            toolStripStatus.Text = "Setting lights to Paused Level";
            queueLightLevel(Properties.Settings.Default.potsAddress, Properties.Settings.Default.potsPausedLevel);
            trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
            queueLightLevel(Properties.Settings.Default.trayAddress, Properties.Settings.Default.trayPausedLevel);
            trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;
        }

        private void timerSetLights_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.potsAddress != "" && lights[Properties.Settings.Default.potsAddress] != -1)
            {
                setLightLevel(Properties.Settings.Default.potsAddress, lights[Properties.Settings.Default.potsAddress]);
            }
            else if (Properties.Settings.Default.trayAddress != "" && lights[Properties.Settings.Default.trayAddress] != -1)
            {
                setLightLevel(Properties.Settings.Default.trayAddress, lights[Properties.Settings.Default.trayAddress]);
            }
        }

        public void queueLightLevel(string address, int level)
        {
            writeLog("Queuing light " + address + " to level " + level.ToString());
            lights[address] = level;
        }
    }
}