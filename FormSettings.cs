using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace BrodieTheatre
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.harmonyHubIP = textBoxHarmonyHubIP.Text;
            Properties.Settings.Default.voiceActivity = textBoxVoiceActivity.Text;
            Properties.Settings.Default.occupancyDevice = textBoxOccupancyDevice.Text;
            Properties.Settings.Default.occupancyEnterCommand = textBoxOccupancyEnterCommand.Text;
            Properties.Settings.Default.occupancyExitCommand = textBoxOccupancyExitCommand.Text;

            Properties.Settings.Default.plmPort = comboBoxInsteonPort.Text;
            Properties.Settings.Default.projectorPort = comboBoxProjectorPort.Text;

            Properties.Settings.Default.potsAddress = textBoxPotsAddress.Text;
            Properties.Settings.Default.trayAddress = textBoxTrayAddress.Text;

            Properties.Settings.Default.trayPlaybackLevel = trackBarTrayPlayback.Value;
            Properties.Settings.Default.potsPlaybackLevel = trackBarPotsPlayback.Value;
            Properties.Settings.Default.trayPausedLevel = trackBarTrayPaused.Value;
            Properties.Settings.Default.potsPausedLevel = trackBarPotsPaused.Value;
            Properties.Settings.Default.trayStoppedLevel = trackBarTrayStopped.Value;
            Properties.Settings.Default.potsStoppedLevel = trackBarPotsStopped.Value;

            Properties.Settings.Default.trayEnteringLevel = trackBarTrayEntering.Value;
            Properties.Settings.Default.potsEnteringLevel = trackBarPotsEntering.Value;

            Properties.Settings.Default.globalShutdown = trackBarGlobalShutdown.Value;

            Properties.Settings.Default.motionSensorAddress = textBoxMotionSensorAddress.Text;

            Properties.Settings.Default.Save();
            this.Close();
        }


        private void FormSettings_Load(object sender, EventArgs e)
        {
            textBoxHarmonyHubIP.Text = Properties.Settings.Default.harmonyHubIP;
            textBoxVoiceActivity.Text = Properties.Settings.Default.voiceActivity;
            textBoxOccupancyDevice.Text = Properties.Settings.Default.occupancyDevice;
            textBoxOccupancyEnterCommand.Text = Properties.Settings.Default.occupancyEnterCommand;
            textBoxOccupancyExitCommand.Text = Properties.Settings.Default.occupancyExitCommand;

            try
            {
                trackBarTrayPlayback.Value = Properties.Settings.Default.trayPlaybackLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayPlayback.Value = trackBarTrayPlayback.Minimum;
                }
            }

            try
            {
                trackBarPotsPlayback.Value = Properties.Settings.Default.potsPlaybackLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsPlayback.Value = trackBarPotsPlayback.Minimum;
                }
            }

            try
            {
                trackBarTrayPaused.Value = Properties.Settings.Default.trayPausedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayPaused.Value = trackBarTrayPaused.Minimum;
                }
            }

            try
            {
                trackBarPotsPaused.Value = Properties.Settings.Default.potsPausedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsPaused.Value = trackBarPotsPaused.Minimum;
                }
            }

            try
            {
                trackBarTrayStopped.Value = Properties.Settings.Default.trayStoppedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayStopped.Value = trackBarTrayStopped.Minimum;
                }
            }

            try
            {
                trackBarPotsStopped.Value = Properties.Settings.Default.potsStoppedLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsStopped.Value = trackBarPotsStopped.Minimum;
                }
            }

            try
            {
                trackBarPotsEntering.Value = Properties.Settings.Default.potsEnteringLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarPotsEntering.Value = trackBarPotsEntering.Minimum;
                }
            }

            try
            {
                trackBarTrayEntering.Value = Properties.Settings.Default.trayEnteringLevel;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarTrayEntering.Value = trackBarTrayEntering.Minimum;
                }
            }

            try
            {
                trackBarGlobalShutdown.Value = Properties.Settings.Default.globalShutdown;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarGlobalShutdown.Value = 0;
                }
            }

            textBoxPotsAddress.Text = Properties.Settings.Default.potsAddress;
            textBoxTrayAddress.Text = Properties.Settings.Default.trayAddress;
            textBoxMotionSensorAddress.Text = Properties.Settings.Default.motionSensorAddress;

            //show list of valid com ports
            foreach (string s in SerialPort.GetPortNames())
            {
                comboBoxInsteonPort.Items.Add(s);
                comboBoxProjectorPort.Items.Add(s);
                if (s == Properties.Settings.Default.plmPort)
                {
                    comboBoxInsteonPort.SelectedItem = s;
                }
                if (s == Properties.Settings.Default.projectorPort)
                {
                    comboBoxProjectorPort.SelectedItem = s;
                }
            }
        }

        private void trackBarTrayPlayback_ValueChanged(object sender, EventArgs e)
        {
            
            labelTrayPlayback.Text = (trackBarTrayPlayback.Value * 10).ToString() + "%";
        }

        private void trackBarPotsPlayback_ValueChanged(object sender, EventArgs e)
        {
            labelPotsPlayback.Text = (trackBarPotsPlayback.Value * 10).ToString() + "%";
        }

        private void trackBarTrayPaused_ValueChanged(object sender, EventArgs e)
        {
            labelTrayPaused.Text = (trackBarTrayPaused.Value * 10).ToString() + "%";
        }

        private void trackBarPotsPaused_ValueChanged(object sender, EventArgs e)
        {
            labelPotsPaused.Text = (trackBarPotsPaused.Value * 10).ToString() + "%";
        }

        private void trackBarTrayStopped_ValueChanged(object sender, EventArgs e)
        {
            labelTrayStopped.Text = (trackBarTrayStopped.Value * 10).ToString() + "%";
        }

        private void trackBarPotsStopped_ValueChanged(object sender, EventArgs e)
        {
            labelPotsStopped.Text = (trackBarPotsStopped.Value * 10).ToString() + "%";
        }

        private void trackBarTrayEntering_ValueChanged(object sender, EventArgs e)
        {
            labelTrayEntering.Text = (trackBarTrayEntering.Value * 10).ToString() + "%";
        }

        private void trackBarPotsEntering_ValueChanged(object sender, EventArgs e)
        {
            labelPotsEntering.Text = (trackBarPotsEntering.Value * 10).ToString() + "%";
        }

        private void trackBarGlobalShutdown_ValueChanged(object sender, EventArgs e)
        {
            if (trackBarGlobalShutdown.Value == 1)
            {
                labelGlobalShutdownHours.Text = "hour";
            }
            else
            {
                labelGlobalShutdownHours.Text = "hours";
            }
            labelGlobalShutdown.Text = trackBarGlobalShutdown.Value.ToString();
        }
    }
}
