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
        private async Task ConnectHarmonyAsync()
        {
            bool error = false;
            var currentActivityID = "";
            writeLog("Harmony:  Connecting to Hub");
            try
            {
                Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
                Thread.Sleep(4000);
                currentActivityID = await Program.Client.GetCurrentActivityAsync();
                writeLog("Harmony:  Connected");
            }
            catch
            {
                writeLog("Error:  Cannot connect to Harmony Hub");
                error = true;
            }
            if (!error)
            {

                formMain.BeginInvoke(new Action(() =>
                {
                    Thread.Sleep(3000);
                    writeLog("Harmony:  Update Activities");
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
                formMain.harmonyUpdateActivities(activity);
                if (activity == "-1")
                {
                    formMain.projectorPowerOff();
                    formMain.lightsOn();
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
            bool notLoaded = true;
            int count = 0;
            while (notLoaded && count < 3)
            { 
                try
                {
                    var harmonyConfig = await Program.Client.GetConfigAsync();

                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Updating Activities";
                        formMain.listBoxActivities.Items.Clear();
                        foreach (var activity in harmonyConfig.Activities)
                        {
                            if (activity.Id == currentActivityID)
                            {
                                formMain.labelCurrentActivity.Text = activity.Label;
                                if (Convert.ToInt32(activity.Id) < 0)
                                {
                                    formMain.disableGlobalShutdown();
                                }
                                else
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
                        writeLog("Harmony:  Activities updated");
                        notLoaded = false;                   
                    }));
                }

                catch
                {         
                    writeLog("Error:  Cannot update Harmony Activities");
                    Thread.Sleep(3000);
                    count++;
                }
            }
        }

        private async void harmonySendCommand(string device, string button)
        {
            bool success = false;
            int counter = 0;
            while (!success && counter < 3)
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
                                    if (function.Name == button)
                                    {
                                        await Program.Client.SendCommandAsync(currDevice.Id, function.Name);
                                        success = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    writeLog("Error:  Failed to send Harmony Command");
                    counter += 1;
                }
            }
        }

        private void listBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            startActivity(activity);
        }

        // Start Harmony Activity
        private async void startActivity(Activities activity)
        {
            bool keep_looping = true;
            while (keep_looping)
            {
                try
                {
                    await Program.Client.StartActivityAsync(activity.Id);
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Starting Harmony activity - " + activity.Text;
                        if (Convert.ToInt32(activity.Id) >= 0)
                        {
                            //An activity is starting
                            formMain.timerStartLights.Enabled = true;
                        }
                        else //Power Off
                        {
                            //Turn up the ligths so occupants can find their way out
                            lightsToEnteringLevel();
                        }
                    }
                    ));
                    keep_looping = false;
                }
                catch
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Harmony Timeout - reconnecting";
                    }
                    ));
                    Program.Client.Dispose();
                    await ConnectHarmonyAsync();
                }
            }
        }

        private async void startActivityByName(string activityName)
        {
            for (int i = 0; i < listBoxActivities.Items.Count; i++)
            {
                Activities currItem = (Activities)listBoxActivities.Items[i];
                if (currItem.Text == activityName)
                {
                    try
                    {
                        await Program.Client.StartActivityAsync(currItem.Id);
                        writeLog("Harmony:  Starting Activity " + activityName);
                    }
                    catch
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Failed to start Harmony activity - " + activityName;
                        }
                        ));
                    }
                }
            }
        }
    }
}