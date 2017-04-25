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
            public string powerCommand = null;
        }

        public ProjectorLensChange projectorCommand = new ProjectorLensChange();

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
                toolStripStatus.Text = "Could not open projector serial port";
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
                    // Projector is in Power On State
                    if (response.Contains("001"))
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelProjectorPower.Text = "On";
                            formMain.buttonProjectorPower.Text = "Power Off";
                                                    }
                        ));
                    }
                    //  Projector is in Power off State
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
                        formMain.toolStripStatus.Text = "Lens change - received: " + response;
                        formMain.writeLog("Projector:  Received lens change response '" + response + "'");
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
            timerProjectorControl.Enabled = true;
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
                writeLog("Projector:  Changing lens aspect ratio to 'narrow'");
            }
            else if (aspect >= 1.9 && (force || labelProjectorLensAspect.Text != "Wide"))
            {
                projectorSendCommand(pj_codes[1]);
                labelProjectorLensAspect.Text = "Wide";
                writeLog("Projector:  Changing lens aspect ratio to 'wide'");
            }
            projectorCommand.force = false;
        }

        private void projectorQueueChangeAspect(float aspect, bool force=false)
        {
            if (labelProjectorPower.Text == "On")
            {
                if (timerProjectorControl.Enabled == true)
                {
                    // Wait for the last Aspect change to finish
                    writeLog("Projector:  Queueing Aspect Ratio change - " + aspect.ToString());
                    projectorCommand.newAspect = aspect;
                    projectorCommand.force = force;
                }
                else
                {
                    projectorChangeAspect(aspect, force);
                }
            }
        }

        private void timerProjectorControl_Tick(object sender, EventArgs e)
        {
            timerProjectorControl.Enabled = false;
            if (projectorCommand.powerCommand != null)
            {
                if (projectorCommand.powerCommand == "001")
                {
                    projectorPowerOn();
                }
                else if (projectorCommand.powerCommand == "000")
                {
                    projectorPowerOff();
                }
            }
            else if (projectorCommand.newAspect > 0)
            {
                // A queued projector lens aspect ratio change is waiting
                projectorQueueChangeAspect(projectorCommand.newAspect, projectorCommand.force);
            }
            else if (projectorCommand.newAspect == 0)
            {
                buttonProjectorChangeAspect.Enabled = true;
            }

            // Reset commands
            projectorCommand.powerCommand = null;
            projectorCommand.newAspect = 0;
            projectorCommand.force = false;
        }

        private void projectorPowerOn()
        {
            writeLog("Projector:  Powering On");
            labelProjectorPower.Text = "Powering On";
            // Set the Projector to the currently AR in the UI to ensure we are in sync.
            projectorCommand.newAspect = 1;
            projectorCommand.force = true;
            if (timerProjectorControl.Enabled == true)
            {
                projectorCommand.powerCommand = "001";
            }
            else
            {
                projectorSendCommand("PON");
      
                timerProjectorControl.Enabled = true;
            }
        }

        private void projectorPowerOff()
        {
            labelProjectorPower.Text = "Powering Off";
            writeLog("Projector:  Powering Off");
            if (timerProjectorControl.Enabled == true)
            {
                projectorCommand.powerCommand = "000";
            }
            else
            {
                projectorSendCommand("POF");
                timerProjectorControl.Enabled = true;
            }
            
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