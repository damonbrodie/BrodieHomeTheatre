using System;
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
            }));
            try
            {
                Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
                await doDelay(2000);
                currentActivityID = await Program.Client.GetCurrentActivityAsync();
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Harmony:  Connected to Hub, current activity ID is '" + currentActivityID + "'");
                }));
                if (currentActivityID != "-1")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        if (formMain.labelRoomOccupancy.Text != "Occupied")
                        {
                            formMain.insteonDoMotion(false);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                formMain.writeLog("Harmony:  Cannot connect to Harmony Hub '" + ex.ToString());
                }));
                error = true;
            }
            if (!error && shouldUpdate)
            {
                await doDelay(3000);
                formMain.BeginInvoke(new Action(() =>
                { 
                    formMain.harmonyUpdateActivities(currentActivityID);
                }));           
            }
        }

        private async void harmonyClient_OnActivityChanged(object sender, string activityID)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.writeLog("Harmony:  Hub message received with current activity ID '" + activityID + "'");
                formMain.harmonyUpdateActivities(activityID);
            }));
            if (activityID == "-1")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.projectorPowerOff();
                    formMain.lightsToEnteringLevel();
                }));
            }
            else
            {
                await doDelay(3000);
                formMain.BeginInvoke(new Action(() =>
                {
                    if (formMain.labelRoomOccupancy.Text != "Occupied")
                    {
                        formMain.insteonDoMotion(false);
                    }
                    formMain.projectorPowerOn();
                }));
            }
        }

        private async void harmonyUpdateActivities(string currentActivityID)
        {
            int counter = 0;
            while (counter < 3)
            {
                try
                { 
                    var harmonyConfig = await Program.Client.GetConfigAsync();
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.listBoxActivities.Items.Clear();
                    }));
                    foreach (var activity in harmonyConfig.Activities)
                    {
                        if (activity.Id == currentActivityID)
                        {
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.labelCurrentActivity.Text = activity.Label;
                            }));
                            if (labelProjectorPower.Text == "On" && activity.Id == "-1")
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.writeLog("Harmony:  Activity disabled - Powering off projector");
                                    formMain.projectorPowerOff();
                                }));
                            }
                            else if (labelProjectorPower.Text == "Off" && activity.Id != "-1")
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.writeLog("Harmony:  Activity enabled - Powering on projector");
                                    formMain.projectorPowerOn();
                                }));
                            }
                            if (Convert.ToInt32(activity.Id) >= 0)
                            {
                                formMain.BeginInvoke(new Action(() =>
                                {
                                    formMain.resetGlobalTimer();
                                }));
                            }
                        }
                        else
                        {
                            Activities item = new Activities();
                            item.Id = activity.Id;
                            item.Text = activity.Label;
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.listBoxActivities.Items.Add(item);
                            }));
                        }
                    }
                    return;
                }
                catch
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Harmony:  Cannot update Harmony Activities");
                    }));
                    counter += 1;
                }
            }
        }

        public bool harmonyIsActivityStarted()
        {
            if (labelCurrentActivity.Text == "PowerOff" || labelCurrentActivity.Text == String.Empty)
            {
                return false;
            }
            return true;
        }

        private async void harmonySendCommand(string device, string deviceFunction)
        {
            int counter = 0;
            while (counter < 3)
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
                                            formMain.writeLog("Harmony:  Sent Command '" + function.Name + "' to ID '" + currDevice.Id + "'");
                                        }));
                                        return;
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
                    }));
                    Program.Client.Dispose();
                    await harmonyConnectAsync(false);
                    counter += 1;
                }
            }
        }

        // Start Harmony Activity
        private async void harmonyStartActivity(string activityName, string activityId, bool affectLights = true)
        {
            int counter = 0;
            while (counter < 3)
            {
                try
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Harmony:  Starting Activity '" + activityName + "' Id '" + activityId + "'");
                        formMain.toolStripStatus.Text = "Starting Harmony activity - " + activityName;
                        formMain.labelCurrentActivity.Text = activityName;
                    }));
                    // Activities > 0 are those that are user driven. -1 means poweroff
                    if (Convert.ToInt32(activityId) >= 0)
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.projectorPowerOn();
                        
                            //An activity is starting wait for Projector to power up then dim the lights
                            if (affectLights)
                            {
                                formMain.timerStartLights.Enabled = true;
                            }
                        }));

                        // Delay the harmony activity to let the projector start.  Having the Amp and projector start
                        // at the same time sometimes causes the Intel graphics to go crazy
                        await doDelay(5000);
                        await Program.Client.StartActivityAsync(activityId);
                    }
                    else //Power Off
                    {
                        //await Program.Client.StartActivityAsync(activityId);
                        await Program.Client.TurnOffAsync();
                        await doDelay(1000);
                        formMain.BeginInvoke(new Action(() =>
                        {
                            //Turn up the ligths so occupants can find their way out
                            if (affectLights)
                            {
                                formMain.lightsToEnteringLevel();
                            }
                            formMain.projectorPowerOff();
                        }));
                    }
                    return;
                }
                catch
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Harmony Timeout - reconnecting";
                        formMain.writeLog("Harmony:  Error starting activity");
                    }));
                    Program.Client.Dispose();
                    await harmonyConnectAsync(false);
                    counter += 1;
                }
            }
        }

        private void harmonyStartActivityByName(string activityName, bool affectLights = true)
        {
            if (activityName == "PowerOff")
            {
                harmonyStartActivity("PowerOff", "-1", affectLights);
                return;
            }
            for (int i = 0; i < listBoxActivities.Items.Count; i++)
            {
                Activities currItem = (Activities)listBoxActivities.Items[i];
                if (currItem.Text == activityName)
                {      
                    harmonyStartActivity(activityName, currItem.Id, affectLights);
                    return;
                }
            }
            writeLog("Harmony:  Unknown Activity - cound not start by Name");
        }

        private async void timerHarmonyPoll_Tick(object sender, EventArgs e)
        {

            timerHarmonyPoll.Interval = 60000;
            
            if (labelHarmonyStatus.Text == "Connected")
            {
                var currentActivityID = "";
                bool error = false;
                try
                {
                    currentActivityID = await Program.Client.GetCurrentActivityAsync();
                }
                catch
                {
                    if (debugHarmony)
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.writeLog("Harmony:  Error Polling Hub");
                        }));
                    }
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Error polling Harmony Hub";
                    }));
                    error = true;
                }
                
                if (!error)
                {
                    await doDelay(3000);
                    formMain.BeginInvoke(new Action(() =>
                    {
                        if (debugHarmony)
                        {
                            formMain.writeLog("Harmony:  Poll Hub - current activity '" + currentActivityID + "'");
                        }
                        formMain.toolStripStatus.Text = "Poll Harmony Hub for updated activities";
                        formMain.harmonyUpdateActivities(currentActivityID);
                    }));
                    return;
                }
            }
            await harmonyConnectAsync(true);
        }
    }
}