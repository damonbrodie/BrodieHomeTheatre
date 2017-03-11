using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;


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
            toolStripStatus.Text = "Setting lights to Stopped Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
        }
    }
}