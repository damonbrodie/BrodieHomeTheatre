using System;
using System.Windows.Forms;
using SoapBox.FluentDwelling;
using SoapBox.FluentDwelling.Devices;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        Plm powerlineModem;
        public string currentPLMport;
        public bool plmConnected;

        public int insteonProcessDimmerMessage(string message, string address)
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
                    level = insteonGetLightLevel(address);
                    break;
                default:
                    level = insteonGetLightLevel(address);
                    break;
            }
            return level;
        }

        public bool insteonProcessMotionSensorMessage(string message, string address)
        {
            bool state = false;
            writeLog("Insteon:  Received from address '" + address + "' message '" + message + "'");
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

        private void insteonConnectPLM()
        {
            if (Properties.Settings.Default.plmPort != string.Empty)
            {
                plmConnected = false;
                labelPLMstatus.Text = "Connecting";
                writeLog("Insteon:  Connecting to PLM");
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
                            level = insteonProcessDimmerMessage(desc, address);
                            formMain.BeginInvoke(new Action(() =>
                            {
                                if (level >= 0)
                                {
                                    formMain.writeLog("Insteon:  Received Tray dimmer update from PLM - level " + level.ToString());
                                    formMain.trackBarTray.Value = level;
                                    if (level > 0)
                                    {
                                        //Only need to reset the timer if it is on
                                        formMain.resetGlobalTimer();
                                    }
                                }
                            }
                            ));
                        }

                        else if (address == Properties.Settings.Default.potsAddress)
                        {
                            level = insteonProcessDimmerMessage(desc, address);
                            formMain.BeginInvoke(new Action(() =>
                            {        
                                if (level >= 0)
                                {
                                    formMain.writeLog("Insteon:  Received Pots dimmer update from PLM - level " + level.ToString());
                                    formMain.trackBarPots.Value = level;
                                    if (level > 0)
                                    {
                                        //Only need to reset the timer if it is on
                                        formMain.resetGlobalTimer();
                                    }
                                }
                            }
                            ));
                        }
                        else if (address == Properties.Settings.Default.motionSensorAddress)
                        {
                            if (insteonProcessMotionSensorMessage(desc, address))
                            { //Motion Detected
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    if (labelMotionSensorStatus.Text != "Motion Detected")
                                    {
                                        formMain.writeLog("Insteon:  Motion Detected");
                                        formMain.labelMotionSensorStatus.Text = "Motion Detected";
                                        formMain.labelRoomOccupancy.Text = "Occupied";
                                        formMain.resetGlobalTimer();
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
                                        formMain.writeLog("Insteon:  No Motion Detected");
                                        formMain.labelRoomOccupancy.Text = "Vacant";
                                        formMain.labelMotionSensorStatus.Text = "No Motion";
                                    }
                                }
                                ));
                            }
                        }
                        else if (address == Properties.Settings.Default.doorSensorAddress)
                        {
                            if (insteonProcessMotionSensorMessage(desc, address))
                            { //Door Open Detected
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.writeLog("Insteon:  Door Opened");
                                    formMain.toolStripStatus.Text = "Door Opened";
                                }
                                ));
                            }
                            else
                            { //Door Closed
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.writeLog("Insteon:  Door Closed");
                                    formMain.toolStripStatus.Text = "Door Closed";
                                }
                                ));
                            }
                            // No matter if the door is opened or closed, we turn on the lights if the room is idle.
                            formMain.BeginInvoke(new Action(() =>
                            {
                                if (!harmonyIsActivityStarted() && formMain.labelKodiPlaybackStatus.Text == "Stopped" && formMain.labelRoomOccupancy.Text != "Occupied")
                                {
                                    formMain.lightsToEnteringLevel();
                                }
                            }
                            ));
                        }
                    });
                timerPLMreceive.Enabled = true;
                queueLightLevel(Properties.Settings.Default.potsAddress, 0);
                queueLightLevel(Properties.Settings.Default.trayAddress, 0);
                timerCheckPLM.Enabled = true;
            }
        }

        public int insteonGetLightLevel(string address)
        {
            int level = 0;
            DeviceBase device;
            if (address == string.Empty) return 0;
            if (powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var lightingControl = device as DimmableLightingControl;
                byte onLevel;

                lightingControl.TryGetOnLevel(out onLevel);
                int integerLevel = Convert.ToInt32(onLevel);
                float decLevel = (float)integerLevel / 254 * 10;

                level = (int)decLevel;
                writeLog("Insteon:  Get light " + address + " at level " + level.ToString());
            }
            return level;
        }

        public void insteonSetLightLevel(string address, int level)
        {
            DeviceBase device;
            if (address == string.Empty) return;
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
                        writeLog("Insteon:  Could not set Light " + address + " to level " + level.ToString());
                    }
                    else
                    {
                        lights[address] = -1;
                        writeLog("Insteon:  Set Light " + address + " to level " + level.ToString());

                        if (toInt > 0)
                        {
                            resetGlobalTimer();
                        }
                        return;
                    }
                    counter++;
                }
            }
            toolStripStatus.Text = "Could not connect to light - " + address;
            writeLog("Insteon:  Error Setting Light " + address + " to level " + level.ToString());
        }

        private void PowerlineModem_OnError(object sender, EventArgs e)
        {
            if (powerlineModem.Exception.GetType() == typeof(TimeoutException))
            {
                plmConnected = false;
                labelPLMstatus.Text = "Disconnected";
                writeLog("Insteon:  Error connecting to PLM");
                labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;
                timerPLMreceive.Enabled = false;
                timerCheckPLM.Enabled = true;
            }
        }

        private async void timerCheckPLM_Tick(object sender, EventArgs e)
        {
            timerCheckPLM.Enabled = false;
            plmConnected = true;
            labelPLMstatus.Text = "Connected";
            writeLog("Insteon:  Connected to PLM");
            labelPLMstatus.ForeColor = System.Drawing.Color.ForestGreen;
            trackBarTray.Value = insteonGetLightLevel(Properties.Settings.Default.trayAddress);
            await doDelay(200);
            trackBarPots.Value = insteonGetLightLevel(Properties.Settings.Default.potsAddress);
        }
    }
}