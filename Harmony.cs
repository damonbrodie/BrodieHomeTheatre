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
            Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
            var currentActivityID = await Program.Client.GetCurrentActivityAsync();
            Thread.Sleep(3000);
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.harmonyUpdateActivities(currentActivityID);
            }
            ));
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
                }));
            }
            catch
            {
            }
        }

        private async void harmonySendCommand(string device, string button)
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
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

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
                        else
                        {
                            // Powering off
                            formMain.toolStripStatus.Text = "Turning on pot lights";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
                            formMain.toolStripStatus.Text = "Turning on tray lights";
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
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