using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string projectorLastCommand;
        public class ProjectorLensChange
        {
            public float newAspect = 0;
            public bool force = false;
        }

        public ProjectorLensChange projectorLensChange = new ProjectorLensChange();

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

        private async void SerialPortProjector_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
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
                            formMain.buttonProjectorPower.Text = "Powering On";
                        }
                        ));
                        // Wait for the projector to power on - it won't respond to Serial commands
                        // for 10 seconds after Power On
                        await doDelay(15000);
                        formMain.BeginInvoke(new Action(() =>
                        {
                            // Set the Projector to the currently AR in the UI to ensure we are in sync.
                            projectorQueueChangeAspect(1, true);
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

        private void projectorChangeAspect(float aspect, bool force=false)
        {
            if (aspect <= 0)
            {
                return;
            }
            timerProjectorLensControl.Enabled = true;
            buttonProjectorChangeAspect.Enabled = false;
            List<string> pj_codes = new List<string> {
                "VXX:LMLI0=+00000",
                "VXX:LMLI0=+00001",
                "VXX:LMLI0=+00002",
                "VXX:LMLI0=+00003",
                "VXX:LMLI0=+00004",
                "VXX:LMLI0=+00005"
            };
            projectorLastCommand = "Lens";
            if (aspect < 1.9 && (force || labelProjectorLensAspect.Text != "Narrow"))
            {
                projectorSendCommand(pj_codes[0]);
                labelProjectorLensAspect.Text = "Narrow";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Narrow");
            }
            else if (aspect >= 1.9 && (force || labelProjectorLensAspect.Text != "Wide"))
            {
                projectorSendCommand(pj_codes[1]);
                labelProjectorLensAspect.Text = "Wide";
                writeLog("Projector:  Changing to Lens Aspect Ratio to Wide");
            }
            projectorLensChange.force = false;
        }

        private void projectorQueueChangeAspect(float aspect, bool force=false)
        {
            if (labelProjectorPower.Text == "On")
            {
                if (timerProjectorLensControl.Enabled == true)
                {
                    // Wait for the last Aspect change to finish
                    writeLog("Projector:  Queueing Aspect Ratio change - " + aspect.ToString());
                    projectorLensChange.newAspect = aspect;
                    projectorLensChange.force = force;
                }
                else
                {
                    projectorChangeAspect(aspect, force);
                }
            }
        }

        private void timerProjectorLensControl_Tick(object sender, EventArgs e)
        {
            timerProjectorLensControl.Enabled = false;
            if (projectorLensChange.newAspect > 0)
            {
                // A queued Aspect change is waiting
                projectorQueueChangeAspect(projectorLensChange.newAspect, projectorLensChange.force);
                projectorLensChange.newAspect = 0;
                projectorLensChange.force = false;
                
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
            if (labelProjectorLensAspect.Text == "Narrow")
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