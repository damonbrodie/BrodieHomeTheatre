﻿using System;
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

        public DateTime insteonMotionLatchExpires;
        public bool insteonMotionLatchActive = false;

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

        public int insteonProcessMotionSensorMessage(string message, string address)
        {
            int state = -1;
            if (message == string.Empty)
            {
                return state;
            }
            writeLog("Insteon:  Process motion sensor from address '" + address + "' message '" + message + "'");
            switch (message)
            {
                case "Turn On":
                    state = 1;
                    break;
                case "Turn Off":
                    state = 0;
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
                powerlineModem.Network.StandardMessageReceived += Network_StandardMessageReceived;

                timerPLMreceive.Enabled = true;
                queueLightLevel(Properties.Settings.Default.potsAddress, 0);
                queueLightLevel(Properties.Settings.Default.trayAddress, 0);
                timerCheckPLM.Enabled = true;
            }
        }

        private void Network_StandardMessageReceived(object sender, StandardMessageReceivedArgs e)
        {                  
            string desc = e.Description;
            string address = e.PeerId.ToString();
            int level;

            if (address == Properties.Settings.Default.trayAddress)
            {
                level = insteonProcessDimmerMessage(desc, address);     
                if (level >= 0)
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Insteon:  Received Tray dimmer update from PLM - level '" + level.ToString() + "'");
                        formMain.trackBarTray.Value = level;
                    }
                    ));
                    if (level > 0)
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            //Only need to reset the timer if it is on
                            formMain.resetGlobalTimer();
                        }
                        ));
                    }
                }
            }
            else if (address == Properties.Settings.Default.potsAddress)
            {
                level = insteonProcessDimmerMessage(desc, address);            
                if (level >= 0)
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Insteon:  Received Pots dimmer update from PLM - level '" + level.ToString() + "'");
                        formMain.trackBarPots.Value = level;
                    }
                    ));
                    if (level > 0)
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            //Only need to reset the timer if it is on
                            formMain.resetGlobalTimer();
                        }
                        ));
                    }
                }
            }
            else if (address == Properties.Settings.Default.motionSensorAddress)
            {
                int state = insteonProcessMotionSensorMessage(desc, address);
                if (state == 1)
                { //Motion Detected
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.insteonDoMotion();
                    }
                    ));
                }
                else if (state == 0)
                { //No Motion Detected
                    formMain.BeginInvoke(new Action(() =>
                    {
                        if (formMain.labelMotionSensorStatus.Text != "No Motion")
                        {
                            formMain.writeLog("Insteon:  Motion Sensor reported 'No Motion Detected'");
                            formMain.progressBarInsteonMotionLatch.Value = formMain.progressBarInsteonMotionLatch.Maximum;
                            formMain.insteonMotionLatchExpires = DateTime.Now.AddMinutes(Properties.Settings.Default.InsteonMotionLatch);
                            formMain.insteonMotionLatchActive = true;
                            formMain.labelMotionSensorStatus.Text = "No Motion";
                        }
                    }
                    ));
                }
            }
            else if (address == Properties.Settings.Default.doorSensorAddress)
            {
                int state = insteonProcessMotionSensorMessage(desc, address);
                if (state == 1)
                { //Door Open Detected
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Insteon:  Door Opened");
                        formMain.toolStripStatus.Text = "Door Opened";
                    }
                    ));
                }
                else if (state == 0)
                { //Door Closed
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Insteon:  Door Closed");
                        formMain.toolStripStatus.Text = "Door Closed";
                    }
                    ));
                }
                // No matter if the door is opened or closed, we turn on the lights if the room is idle.

                if (!harmonyIsActivityStarted() && labelKodiPlaybackStatus.Text == "Stopped" && labelRoomOccupancy.Text != "Occupied")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.lightsToEnteringLevel();
                    }
                    ));
                }
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
                //writeLog("Insteon:  Get light '" + address + "' at level '" + level.ToString() + "'");
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
                        writeLog("Insteon:  Could not set light '" + address + "' to level '" + level.ToString() + "'");
                    }
                    else
                    {
                        lights[address] = -1;
                        writeLog("Insteon:  Set light '" + address + "' to level '" + level.ToString() + "'");
                        if (toInt > 0)
                        {
                            resetGlobalTimer();
                        }
                        return;
                    }
                    counter++;
                }
            }
            toolStripStatus.Text = "Could not connect to light '" + address + "'";
            writeLog("Insteon:  Error setting light '" + address + "' to level '" + level.ToString() + "'");
        }

        private void PowerlineModem_OnError(object sender, EventArgs e)
        {
            if (powerlineModem.Exception.GetType() == typeof(TimeoutException))
            {
                plmConnected = false;
                formMain.BeginInvoke(new Action(() =>
                {
                    labelPLMstatus.Text = "Disconnected";
                    writeLog("Insteon:  Error connecting to PLM");
                    labelPLMstatus.ForeColor = System.Drawing.Color.Maroon;
                    timerPLMreceive.Enabled = false;
                    timerCheckPLM.Enabled = true;
                }
                ));
            }
        }

        private void timerCheckPLM_Tick(object sender, EventArgs e)
        {
            plmConnected = true;
            timerCheckPLM.Enabled = false;
            formMain.labelPLMstatus.Text = "Connected";
            formMain.writeLog("Insteon:  Connected to PLM");
            formMain.labelPLMstatus.ForeColor = System.Drawing.Color.ForestGreen;
            formMain.insteonPollLights();
        }

        private void timerInsteonMotionLatch_Tick(object sender, EventArgs e)
        {
            if (insteonMotionLatchActive)
            {
                DateTime rightNow = DateTime.Now;
                if (insteonMotionLatchExpires < rightNow)
                {
                    insteonMotionLatchActive = false;
                    writeLog("Insteon:  Latch timer expired - setting room vacant");
                    labelRoomOccupancy.Text = "Vacant";
                    labelMotionSensorStatus.Text = "No Motion";
                }
                else
                {
                    float secondsDiff = (float)(insteonMotionLatchExpires - rightNow).TotalSeconds;
                    float totalSecs = Properties.Settings.Default.InsteonMotionLatch * 60;
                    float percentage = (secondsDiff / totalSecs) * 100;
                    progressBarInsteonMotionLatch.Value = Convert.ToInt32(percentage);
                    return;
                }
            }
            progressBarInsteonMotionLatch.Value = progressBarInsteonMotionLatch.Minimum;
        }

        private void insteonDoMotion()
        {
            if (labelMotionSensorStatus.Text != "Motion Detected")
            {
                writeLog("Insteon:  Motion Sensor reported 'Motion Detected'");
                labelMotionSensorStatus.Text = "Motion Detected";
                labelRoomOccupancy.Text = "Occupied";
                resetGlobalTimer();
                insteonMotionLatchActive = false;
            }
        }

        private async void insteonPollLights()
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.trackBarTray.Value = insteonGetLightLevel(Properties.Settings.Default.trayAddress);
            }
            ));   
            await doDelay(1200);
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.trackBarPots.Value = insteonGetLightLevel(Properties.Settings.Default.potsAddress);
            }
            ));
        }
        private void timerInsteonPoll_Tick(object sender, EventArgs e)
        {
            if (labelPLMstatus.Text == "Connected")
            {
                toolStripStatus.Text = "Polling for lights status";
                insteonPollLights();
            }
        }
    }
}