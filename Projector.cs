using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void ConnectProjector()
        {
            try
            {
                serialPortProjector.PortName = Properties.Settings.Default.projectorPort;
                if (!serialPortProjector.IsOpen) serialPortProjector.Open();
                serialPortProjector.DataReceived += SerialPortProjector_DataReceived;
                projectorConnected = true;
                labelProjectorStatus.Text = "Connected";
                labelProjectorStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }
            catch
            {
                toolStripStatus.Text = "Could not open Project Serial Port";
                projectorConnected = false;
                labelProjectorStatus.Text = "Disconnected";
                labelProjectorStatus.ForeColor = System.Drawing.Color.Maroon;
            }
        }

        private void checkProjectorPower()
        {
            if (projectorConnected)
            {
                projectorLastCommand = "Power";
                ProjectorSendCommand("QPW");
            }
        }

        private void ProjectorSendCommand(string command)
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
                        labelProjectorPower.Text = "On";
                        buttonProjectorPower.Text = "Power Off";
                    }
                    else if (response.Contains("000"))
                    {
                        labelProjectorPower.Text = "Off";
                        buttonProjectorPower.Text = "Power On";
                    }
                    break;
                case "Lens":
                    toolStripStatus.Text = "Lens Change - received: " + response;
                    writeLog("Projector:  Received Lens change response '" + response + "'");
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
            checkProjectorPower();
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
            if (aspect < 1.9)
            {
                ProjectorSendCommand(pj_codes[0]);
                labelLensAspect.Text = "Narrow";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Narrow");
            }
            else
            {
                ProjectorSendCommand(pj_codes[1]);
                labelLensAspect.Text = "Wide";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Wide");
            }
        }

        private void projectorQueueChangeAspect(float aspect)
        {
            if (timerProjectorLensControl.Enabled == true)
            {
                // Wait for the last Aspect change to finish
                writeLog("Projector:  Queueing Aspect Ratio change");
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
            ProjectorSendCommand("PON");
            labelProjectorPower.Text = "Powering On";
            writeLog("Projector:  Powering On");
        }

        private void projectorPowerOff()
        {
            ProjectorSendCommand("POF");
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
