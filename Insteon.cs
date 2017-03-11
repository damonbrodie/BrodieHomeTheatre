using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HarmonyHub;
using HarmonyHub.Entities.Response;
using System.IO;
using SoapBox.FluentDwelling;
using SoapBox.FluentDwelling.Devices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
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
                                trackBarTray.Value = level;
                                resetGlobalTimer();
                            }
                        }

                        else if (address == Properties.Settings.Default.potsAddress)
                        {
                            level = processDimmerMessage(desc, address);
                            if (level >= 0)
                            {
                                trackBarPots.Value = level;
                                resetGlobalTimer();
                            }
                        }
                        else if (address == Properties.Settings.Default.motionSensorAddress)
                        {
                            if (processMotionSensorMessage(desc, address))
                            { //Motion Detected
                                labelMotionSensorStatus.Text = "Motion Detected";
                                labelRoomOccupancy.Text = "Occupied";
                            }
                            else
                            { //No Motion Detected
                                labelRoomOccupancy.Text = "Vacant";
                                labelMotionSensorStatus.Text = "No Motion";
                            }

                        }
                    });
                timerPLMreceive.Enabled = true;
                setLightLevel(Properties.Settings.Default.potsAddress, 0);
                setLightLevel(Properties.Settings.Default.trayAddress, 0);
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
            }
            return level;
        }

        public void setLightLevel(string address, int level)
        {
            DeviceBase device;
            if (powerlineModem != null && powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var lightingControl = device as DimmableLightingControl;
                float theVal = level * 254 / 10;
                int toInt = (int)theVal;
                Boolean retVal = lightingControl.RampOn((byte)toInt);
                Thread.Sleep(500);
                if (toInt > 0)
                {
                    resetGlobalTimer();
                }
            }
            else
            {
                toolStripStatus.Text = "Could not connect to light - " + address;
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