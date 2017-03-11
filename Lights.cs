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

        private void timerStartLights_Tick(object sender, EventArgs e)
        {
            timerStartLights.Enabled = false;
            lightsToStoppedLevel();
        }

        private void lightsToStoppedLevel()
        {
            toolStripStatus.Text = "Setting lights to Stopped Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
        }

        private void lightsOn()
        {
            toolStripStatus.Text = "Turning On Lights";
            setLightLevel(Properties.Settings.Default.potsAddress, (100));
            trackBarPots.Value = 100;
            setLightLevel(Properties.Settings.Default.trayAddress, (100));
            trackBarTray.Value = 100;
        }

        private void lightsToEnteringLevel()
        {
            toolStripStatus.Text = "Turning on lights";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;

            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
        }

        private void lightsOff()
        {
            toolStripStatus.Text = "Turning off lights";
            setLightLevel(Properties.Settings.Default.potsAddress, 0);
            trackBarPots.Value = 0;
            setLightLevel(Properties.Settings.Default.trayAddress, 0);
            trackBarTray.Value = 0;
        }

        private void lightsToPlaybackLevel()
        {
            toolStripStatus.Text = "Dimming lights to Playback Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPlaybackLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPlaybackLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;
        }

        private void lightsToPausedLevel()
        {
            toolStripStatus.Text = "Setting lights to Paused Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPausedLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPausedLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;
        }
    }
}