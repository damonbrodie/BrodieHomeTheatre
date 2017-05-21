using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using Microsoft.Speech.Synthesis;
using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundOut;

namespace BrodieTheatre
{
    public partial class FormSettings : Form
    {

        public SpeechSynthesizer speechSynthesizer;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.harmonyHubIP                = textBoxHarmonyHubIP.Text;
            Properties.Settings.Default.voiceActivity               = textBoxVoiceActivity.Text;
            Properties.Settings.Default.plmPort                     = comboBoxInsteonPort.Text;
            Properties.Settings.Default.projectorPort               = comboBoxProjectorPort.Text;
            Properties.Settings.Default.potsAddress                 = textBoxPotsAddress.Text;
            Properties.Settings.Default.trayAddress                 = textBoxTrayAddress.Text;
            Properties.Settings.Default.trayPlaybackLevel           = trackBarTrayPlayback.Value;
            Properties.Settings.Default.potsPlaybackLevel           = trackBarPotsPlayback.Value;
            Properties.Settings.Default.trayPausedLevel             = trackBarTrayPaused.Value;
            Properties.Settings.Default.potsPausedLevel             = trackBarPotsPaused.Value;
            Properties.Settings.Default.trayStoppedLevel            = trackBarTrayStopped.Value;
            Properties.Settings.Default.potsStoppedLevel            = trackBarPotsStopped.Value;
            Properties.Settings.Default.trayEnteringLevel           = trackBarTrayEntering.Value;
            Properties.Settings.Default.potsEnteringLevel           = trackBarPotsEntering.Value;
            Properties.Settings.Default.globalShutdown              = trackBarGlobalShutdown.Value;
            Properties.Settings.Default.motionSensorAddress         = textBoxMotionSensorAddress.Text;
            Properties.Settings.Default.doorSensorAddress           = textBoxDoorSensorAddress.Text;
            Properties.Settings.Default.voiceConfidence             = trackBarVoiceConfidence.Value;
            Properties.Settings.Default.voiceConfidenceNoActivity   = trackBarVoiceConfidenceNoActivity.Value;
            Properties.Settings.Default.startMinimized              = checkBoxStartMinimized.Checked;
            Properties.Settings.Default.computerName                = textBoxComputerName.Text;
            Properties.Settings.Default.kodiJSONPort                = (int)numericUpDownKodiPort.Value;
            Properties.Settings.Default.kodiIP                      = textBoxKodiIP.Text;
            Properties.Settings.Default.speechVoice                 = comboBoxTextToSpeechVoice.Text;
            Properties.Settings.Default.insteonMotionLatch          = trackBarInsteonMotionMinimumTime.Value;
            Properties.Settings.Default.speechDevice                = comboBoxTextToSpeechDevice.Text;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            checkBoxStartMinimized.Checked = Properties.Settings.Default.startMinimized;
            textBoxComputerName.Text = Properties.Settings.Default.computerName;
            textBoxHarmonyHubIP.Text = Properties.Settings.Default.harmonyHubIP;
            textBoxVoiceActivity.Text = Properties.Settings.Default.voiceActivity;
            numericUpDownKodiPort.Value = (decimal)Properties.Settings.Default.kodiJSONPort;
            textBoxKodiIP.Text = Properties.Settings.Default.kodiIP;
            textBoxPotsAddress.Text = Properties.Settings.Default.potsAddress;
            textBoxTrayAddress.Text = Properties.Settings.Default.trayAddress;
            textBoxMotionSensorAddress.Text = Properties.Settings.Default.motionSensorAddress;
            textBoxDoorSensorAddress.Text = Properties.Settings.Default.doorSensorAddress;

            comboBoxTextToSpeechDevice.Items.Add("Default Audio Device");
            comboBoxTextToSpeechDevice.SelectedItem = "Default Audio Device";
            foreach (var device in WaveOutDevice.EnumerateDevices())
            {
                comboBoxTextToSpeechDevice.Items.Add(device.Name);
                if (Properties.Settings.Default.speechDevice == device.Name)
                {
                    comboBoxTextToSpeechDevice.SelectedItem = device.Name;
                }
            }

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
                    trackBarGlobalShutdown.Value = trackBarGlobalShutdown.Minimum;
                }
            }

            try
            {
                trackBarVoiceConfidence.Value = Properties.Settings.Default.voiceConfidence;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarVoiceConfidence.Value = trackBarVoiceConfidence.Minimum;
                }
            }

            try
            {
                trackBarVoiceConfidenceNoActivity.Value = Properties.Settings.Default.voiceConfidenceNoActivity;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarVoiceConfidenceNoActivity.Value = trackBarVoiceConfidenceNoActivity.Minimum;
                }
            }

            try
            {
                trackBarInsteonMotionMinimumTime.Value = Properties.Settings.Default.insteonMotionLatch;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                {
                    trackBarInsteonMotionMinimumTime.Value = trackBarInsteonMotionMinimumTime.Minimum; 
                }
            }

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

            try
            {
                speechSynthesizer = new SpeechSynthesizer();

                foreach (InstalledVoice voice in speechSynthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    comboBoxTextToSpeechVoice.Items.Add(info.Id);
                    if (info.Id == Properties.Settings.Default.speechVoice)
                    {
                        comboBoxTextToSpeechVoice.SelectedItem = info.Id;
                    }
                }
            }
            catch
            { }
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

        private void trackBarVoiceConfidence_ValueChanged(object sender, EventArgs e)
        {
            labelVoiceConfidence.Text = (trackBarVoiceConfidence.Value * 10).ToString() + "%";
        }

        private void trackBarVoiceConfidence_ValueChanged_1(object sender, EventArgs e)
        {
            labelVoiceConfidence.Text = (trackBarVoiceConfidence.Value * 10).ToString() + "%";
        }

        private void buttonPreviewVoice_Click(object sender, EventArgs e)
        {
            string ttsText = "Maybe we should watch the movie Rogue One";
            try
            {
                foreach (InstalledVoice voice in speechSynthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;

                    if (info.Id == comboBoxTextToSpeechVoice.Text)
                    {
                        speechSynthesizer.SelectVoice(info.Name);
                    }
                }
                if (comboBoxTextToSpeechDevice.Text == "Default Audio Device")
                {
                    speechSynthesizer.SetOutputToDefaultAudioDevice();
                    speechSynthesizer.SpeakAsync(ttsText);
                }
                else
                {
                    using (var stream = new MemoryStream())
                    {
                        int deviceID = -1;
                        foreach (var device in WaveOutDevice.EnumerateDevices())
                        {
                            if (device.Name == comboBoxTextToSpeechDevice.Text)
                            {
                                deviceID = device.DeviceId;
                            }

                        }
                        speechSynthesizer.SetOutputToWaveStream(stream);
                        speechSynthesizer.Speak(ttsText);

                        using (var waveOut = new WaveOut { Device = new WaveOutDevice(deviceID) })
                        using (var waveSource = new MediaFoundationDecoder(stream))
                        {
                            waveOut.Initialize(waveSource);
                            waveOut.Play();
                            waveOut.WaitForStopped();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:  " + ex.ToString());
            }
        }

        private void trackBarInsteonMotionMinimumTime_ValueChanged(object sender, EventArgs e)
        {
            labelInsteonMotionLatch.Text = trackBarInsteonMotionMinimumTime.Value.ToString();
        }

        private void trackBarVoiceConfidenceNoActivity_ValueChanged(object sender, EventArgs e)
        {
            labelVoiceConfidenceNoActivity.Text = (trackBarVoiceConfidenceNoActivity.Value * 10).ToString() + "%";
        }
    }
}