using System;
using System.Threading;
using System.Windows.Forms;
using SoapBox.FluentDwelling;
using SoapBox.FluentDwelling.Devices;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public int processDimmerMessage(string message, string address)
        {
            int level = -1;
            switch (message)
            {
                case "Turn On":
                    level = 10;
                    break;
                case "Turn Off":
                    level = 0;
                    break;
                case "Begin Manual Brightening":
                    level = -1;
                    break;
                case "End Manual Brightening/Dimming":
                    level = getLightLevel(address);
                    break;
                default:
                    level = getLightLevel(address);
                    break;
            }
            return level;
        }

        public bool processMotionSensorMessage(string message, string address)
        {
            bool state = false;
            switch (message)
            {
                case "Turn On":
                    state = true;
                    break;
            }
            return state;
        }

        private void timerPLMreceive_Tick(object sender, EventArgs e)
        {
            powerlineModem.Receive();
        }

        private void connectPLM()
        {
            if (Properties.Settings.Default.plmPort != "")
            {
                plmConnected = false;
                labelPLMstatus.Text = "Connecting";
                labelPLMstatus.ForeColor = System.Drawing.Color.ForestGreen;

                powerlineModem = new Plm(Properties.Settings.Default.plmPort);
                powerlineModem.Network.StandardMessageReceived
                    += new StandardMessageReceivedHandler((s, eventReceive) =>
                    {
                        string desc = eventReceive.Description;
                        string address = eventReceive.PeerId.ToString();

                        int level;

                        if (address == Properties.Settings.Default.trayAddress)
                        {
                            level = processDimmerMessage(desc, address);
                            if (level >= 0)
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.trackBarTray.Value = level;
                                    formMain.resetGlobalTimer();
                                }
                                ));
                            }
                        }

                        else if (address == Properties.Settings.Default.potsAddress)
                        {
                            level = processDimmerMessage(desc, address);
                            if (level >= 0)
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.trackBarPots.Value = level;
                                    formMain.resetGlobalTimer();
                                }
                                ));
                            }
                        }
                        else if (address == Properties.Settings.Default.motionSensorAddress)
                        {
                            if (processMotionSensorMessage(desc, address))
                            { //Motion Detected
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    if (labelMotionSensorStatus.Text != "Motion Detected")
                                    {
                                        writeLog("Insteon: Motion Detected");
                                        formMain.labelMotionSensorStatus.Text = "Motion Detected";
                                        formMain.labelRoomOccupancy.Text = "Occupied";
                                    }
                                }
                                ));
                            }
                            else
                            { //No Motion Detected
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    if (labelMotionSensorStatus.Text != "No Motion")
                                    {
                                        writeLog("Insteon: No Motion Detected");
                                        formMain.labelRoomOccupancy.Text = "Vacant";
                                        formMain.labelMotionSensorStatus.Text = "No Motion";
                                    }
                                }
                                ));
                            }

                        }
                    });
                timerPLMreceive.Enabled = true;
                queueLightLevel(Properties.Settings.Default.potsAddress, 0);
                queueLightLevel(Properties.Settings.Default.trayAddress, 0);
                timerCheckPLM.Enabled = true;
            }
        }

        public int getLightLevel(string address)
        {
            int level = 0;
            DeviceBase device;
            if (powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var lightingControl = device as DimmableLightingControl;
                byte onLevel;

                lightingControl.TryGetOnLevel(out onLevel);
                int integerLevel = Convert.ToInt32(onLevel);
                float decLevel = (float)integerLevel / 254 * 10;

                level = (int)decLevel;
                writeLog("Insteon: Get Level Light " + address + " at level " + level.ToString());
            }
            return level;
        }

        public void setLightLevel(string address, int level)
        {
            DeviceBase device;
            if (powerlineModem != null && powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                bool finished = false;
                int counter = 0;

                while (!finished && counter < 3)
                { 
                    var lightingControl = device as DimmableLightingControl;
                    float theVal = (level * 254 / 10) + 1;
                    int toInt = (int)theVal;
                    finished = lightingControl.RampOn((byte)toInt);
                    if (!finished)
                    {
                        writeLog("Error: Could not set Light " + address + " Level " + level.ToString());
                    }
                    else
                    {
                        lights[address] = -1;
                        writeLog("Insteon: Set Light " + address + " Level " + level.ToString());

                        if (toInt > 0)
                        {
                            resetGlobalTimer();
                        }
                    }
                    counter++;
                }
            }
            else
            {
                toolStripStatus.Text = "Could not connect to light - " + address;
                writeLog("Insteon: Error Setting Light " + address + " Level " + level.ToString());
            }
        }

        private void PowerlineModem_OnError(object sender, EventArgs e)
        {
            if (powerlineModem.Exception.GetType() == typeof(TimeoutException))
            {
                plmConnected = false;
                labelPLMstatus.Text = "Disconnected";
                labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;
                timerPLMreceive.Enabled = false;
                timerCheckPLM.Enabled = true;
            }
        }

        private void timerCheckPLM_Tick(object sender, EventArgs e)
        {
            timerCheckPLM.Enabled = false;
            plmConnected = true;
            labelPLMstatus.Text = "Connected";
            labelPLMstatus.ForeColor = System.Drawing.Color.ForestGreen;
            trackBarTray.Value = getLightLevel(Properties.Settings.Default.trayAddress);
            Thread.Sleep(200);
            trackBarPots.Value = getLightLevel(Properties.Settings.Default.potsAddress);
        }
    }
}