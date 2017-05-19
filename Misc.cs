using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundOut;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public async void writeLog(string Message)
        {
            DateTime now = DateTime.Now;
            int counter = 0;
            bool success = false;
            while (!success && counter < 3)
            {
                try
                {
                    using (StreamWriter file = File.AppendText("brodietheatre_log.txt"))
                    {
                        file.WriteLine(now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Message);
                        success = true;
                    }
                }
                catch
                {
                    counter += 1;
                    await doDelay(50);
                }
            }
        }

        async Task doDelay(int ms)
        {
            await Task.Delay(ms);
        }

        public void playAlert()
        {
            Stream str = Properties.Resources.alert;
            
            if (Properties.Settings.Default.speechDevice == "Default Audio Device")
            {
                SoundPlayer snd = new SoundPlayer(str);
                snd.Play();
            }
            else
            {
                
                int deviceID = -1;
                foreach (var device in WaveOutDevice.EnumerateDevices())
                {
                    if (device.Name == Properties.Settings.Default.speechDevice)
                    {
                        deviceID = device.DeviceId;
                    }

                }
                using (var waveOut = new WaveOut { Device = new WaveOutDevice(deviceID) })
                using (var waveSource = new MediaFoundationDecoder(str))
                {
                    waveOut.Initialize(waveSource);
                    waveOut.Play();
                    waveOut.WaitForStopped();
                }
                
            }
        }
    }
}