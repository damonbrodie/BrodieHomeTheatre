using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HarmonyHub;
using HarmonyHub.Entities.Response;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public class Activities
        {
            public string Text;
            public string Id;

            public override string ToString()
            {
                return Text;
            }
        }
        public string currentHarmonyIP;

        private async Task harmonyConnectAsync(bool shouldUpdate = true)
        {
            bool error = false;
            var currentActivityID = "";
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.writeLog("Harmony:  Connecting to Hub");
            }
            ));
            try
            {
                Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
                Thread.Sleep(1000);
                currentActivityID = await Program.Client.GetCurrentActivityAsync();
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Harmony:  Connected to Hub");
                }
                ));
            }
            catch
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Harmony:  Cannot connect to Harmony Hub");
                }
                ));
                error = true;
            }
            if (!error && shouldUpdate)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    Thread.Sleep(3000);
                    formMain.harmonyUpdateActivities(currentActivityID);
                }
                ));
            }
        }

        private async void harmonyClient_OnActivityChanged(object sender, string e)
        {
            var activity = await Program.Client.GetCurrentActivityAsync();
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.writeLog("Harmony:  Hub message received");
                formMain.harmonyUpdateActivities(activity);
                if (activity == "-1")
                {
                    formMain.projectorPowerOff();
                    formMain.lightsToEnteringLevel();
                }
                else
                {
                    formMain.projectorPowerOn();
                }
            }
            ));
        }

        private async void harmonyUpdateActivities(string currentActivityID)
        {
            try
            {
                var harmonyConfig = await Program.Client.GetConfigAsync();

                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.toolStripStatus.Text = "Updating Activities";
                    formMain.writeLog("Harmony:  Updating Activities");
                    formMain.listBoxActivities.Items.Clear();
                    foreach (var activity in harmonyConfig.Activities)
                    {
                        if (activity.Id == currentActivityID)
                        {
                            formMain.labelCurrentActivity.Text = activity.Label;
                            if (Convert.ToInt32(activity.Id) >= 0)
                            {
                                formMain.resetGlobalTimer();
                            }
                        }
                        else
                        {
                            Activities item = new Activities();
                            item.Id = activity.Id;
                            item.Text = activity.Label;
                            formMain.listBoxActivities.Items.Add(item);
                        }
                    }
                    formMain.writeLog("Harmony:  Activities updated");            
                }
                ));
            }

            catch (Exception ex)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Harmony:  Cannot update Harmony Activities " + ex.ToString());
                }
                ));
            }
        }

        private async void harmonySendCommand(string device, string deviceFunction)
        {
            bool success = false;
            int counter = 0;
            while (! success && counter < 3)
            {
                try
                {
                    var harmonyConfig = await Program.Client.GetConfigAsync();
                    foreach (Device currDevice in harmonyConfig.Devices)
                    {
                        if (currDevice.Label == device)
                        {
                            foreach (ControlGroup controlGroup in currDevice.ControlGroups)
                            {
                                foreach (Function function in controlGroup.Functions)
                                {
                                    if (function.Name == deviceFunction)
                                    {
                                        await Program.Client.SendCommandAsync(currDevice.Id, function.Name);
                                        formMain.BeginInvoke(new Action(() =>
                                        {
                                            formMain.writeLog("Harmony:  Sent Command '" + function.Name + "' to Id '" + currDevice.Id + "'");
                                        }
                                        ));
                                        success = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Harmony:  Failed to send Harmony Command");
                    }
                    ));
                    Program.Client.Dispose();
                    await harmonyConnectAsync(false);
                    counter += 1;
                }
            }
        }

        private void listBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            harmonyStartActivity(activity.Text, activity.Id);
        }

        // Start Harmony Activity
        private async void harmonyStartActivity(string activityName, string activityId)
        {
            bool success = false;
            int counter = 0;
            while (! success && counter < 3)
            {
                try
                {
                    await Program.Client.StartActivityAsync(activityId);
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Harmony:  Starting Activity '" + activityName + "' Id '" + activityId + "'");
                        formMain.toolStripStatus.Text = "Starting Harmony activity - " + activityName;
                        if (Convert.ToInt32(activityId) >= 0)
                        {
                            formMain.projectorPowerOn();
                            //An activity is starting wait for Projector to power up then dim the lights
                            formMain.timerStartLights.Enabled = true;
                        }
                        else //Power Off
                        {
                            //Turn up the ligths so occupants can find their way out
                            formMain.lightsToEnteringLevel();
                            formMain.projectorPowerOff();
                            
                        }
                    }
                    ));
                    success = true;
                }
                catch
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Harmony Timeout - reconnecting";
                        formMain.writeLog("Harmony:  Error starting activity");
                    }
                    ));
                    Program.Client.Dispose();
                    await harmonyConnectAsync(false);
                    counter += 1;
                }
            }
        }

        private void harmonyStartActivityByName(string activityName)
        {
            if (activityName == "PowerOff")
            {
                harmonyStartActivity("PowerOff", "-1");
                return;
            }
            for (int i = 0; i < listBoxActivities.Items.Count; i++)
            {
                Activities currItem = (Activities)listBoxActivities.Items[i];
                if (currItem.Text == activityName)
                {      
                    harmonyStartActivity(activityName, currItem.Id);
                    return;
                }
            }
            writeLog("Harmony:  Unknown Activity - cound not start by Name");
        }
    }
}