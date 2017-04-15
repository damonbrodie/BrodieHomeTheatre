﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string projectorLastCommand;
        public float projectorNewAspect = 0;

        private void projectorConnect()
        {
            try
            {
                serialPortProjector.PortName = Properties.Settings.Default.projectorPort;
                if (!serialPortProjector.IsOpen)
                {
                    serialPortProjector.Open();
                }
                serialPortProjector.DataReceived += SerialPortProjector_DataReceived;
                labelProjectorStatus.Text = "Connected";
                labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }
            catch
            {
                toolStripStatus.Text = "Could not open Projector Serial Port";
                labelProjectorStatus.Text = "Disconnected";
                labelProjectorStatus.ForeColor = System.Drawing.Color.Maroon;
            }
        }

        private void projectorCheckPower()
        {
            if (labelProjectorStatus.Text == "Connected")
            {
                projectorLastCommand = "Power";
                projectorSendCommand("QPW");
            }
        }

        private void projectorSendCommand(string command)
        {
            int startByte = 2;
            int endByte = 3;

            char start = (char)startByte;
            char end = (char)endByte;

            string full_command = start + command + end;
            if (serialPortProjector.IsOpen)
            {
                serialPortProjector.Write(full_command);
            }
        }

        private void SerialPortProjector_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string response = serialPortProjector.ReadExisting();
            switch (projectorLastCommand)
            {
                case "Power":
                    if (response.Contains("001"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelProjectorPower.Text = "On";
                            formMain.buttonProjectorPower.Text = "Power Off";
                        }
                        ));
                    }
                    else if (response.Contains("000"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelProjectorPower.Text = "Off";
                            formMain.buttonProjectorPower.Text = "Power On";
                        }
                        ));
                    }
                    break;
                case "Lens":
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Lens Change - received: " + response;
                        formMain.writeLog("Projector:  Received Lens change response '" + response + "'");
                    }
                    ));
                    break;
            }
        }

        private void buttonProjectorPower_Click(object sender, EventArgs e)
        {
            if (buttonProjectorPower.Text == "Power On")
            {
                projectorPowerOn();
            }
            else
            {
                projectorPowerOff();
            }
        }

        private void buttonProjectorChangeAspect_Click(object sender, EventArgs e)
        {
            if (buttonProjectorChangeAspect.Text == "Narrow Aspect")
            {
                projectorQueueChangeAspect((float)1.0); //Narrow
            }
            else
            {
                projectorQueueChangeAspect((float)2.0); //Wide
            }

        }

        private void timerCheckProjector_Tick(object sender, EventArgs e)
        {
            projectorCheckPower();
        }

        private void projectorChangeAspect(float aspect)
        {
            timerProjectorLensControl.Enabled = true;
            buttonProjectorChangeAspect.Enabled = false;
            List<string> pj_codes = new List<string> {
                "VXX:LMLI0=+00000" ,
                "VXX:LMLI0=+00001",
                "VXX:LMLI0=+00002",
                "VXX:LMLI0=+00003" ,
                "VXX:LMLI0=+00004",
                "VXX:LMLI0=+00005" };
            projectorLastCommand = "Lens";
            if (aspect < 1.9 && labelLensAspect.Text != "Narrow")
            {
                projectorSendCommand(pj_codes[0]);
                labelLensAspect.Text = "Narrow";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Narrow");
            }
            else if (aspect >= 1.9 && labelLensAspect.Text != "Wide")
            {
                projectorSendCommand(pj_codes[1]);
                labelLensAspect.Text = "Wide";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Wide");
            }
        }

        private void projectorQueueChangeAspect(float aspect)
        {
            if (timerProjectorLensControl.Enabled == true)
            {
                // Wait for the last Aspect change to finish
                writeLog("Projector:  Queueing Aspect Ratio change - " + aspect.ToString());
                projectorNewAspect = aspect;
            }
            else
            {
                projectorChangeAspect(aspect);
            }
        }

        private void timerProjectorLensControl_Tick(object sender, EventArgs e)
        {
            timerProjectorLensControl.Enabled = false;
            if (projectorNewAspect > 0)
            {
                // A queued Aspect change is waiting
                projectorQueueChangeAspect(projectorNewAspect);
                projectorNewAspect = 0;
            }
            else
            {
                buttonProjectorChangeAspect.Enabled = true;
            }
        }

        private void projectorPowerOn()
        {
            projectorSendCommand("PON");
            labelProjectorPower.Text = "Powering On";
            writeLog("Projector:  Powering On");
        }

        private void projectorPowerOff()
        {
            projectorSendCommand("POF");
            labelProjectorPower.Text = "Powering Off";
            writeLog("Projector:  Powering Off");
        }


        private void labelLensAspect_TextChanged(object sender, EventArgs e)
        {
            if (labelLensAspect.Text == "Narrow")
            {
                buttonProjectorChangeAspect.Text = "Wide Aspect";
            }
            else
            {
                buttonProjectorChangeAspect.Text = "Narrow Aspect";
            }
        }
    }
}
