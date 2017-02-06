using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HarmonyHub;
using System.IO;
using SoapBox.FluentDwelling;
using SoapBox.FluentDwelling.Devices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;



namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        static FormMain formMain;
        public class Activities
        {
            public string Text;
            public string Id;

            public override string ToString()
            {
                return Text;
            }
        }

        Plm powerlineModem;

        //42.22.B8 Pot
        //42.20.F8 Tray

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc proc = HookCallback;
        public static IntPtr hookID = IntPtr.Zero;

        public string currentHarmonyIP;
        public string currentPLMport;
        public bool plmConnected;

        public KinectSensor kinect;
        public bool kinectIsAvailable;

        public SpeechRecognitionEngine recognitionEngine;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeysConverter kc = new KeysConverter();
                string keyCode = kc.ConvertToString(vkCode);
                switch (keyCode)
                {
                    case "F12":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Turning on lights";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                        }
                        ));
                        break;
                    case "F11":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Turning off lights";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, 0);
                            formMain.trackBarPots.Value = 0;
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, 0);
                            formMain.trackBarTray.Value = 0;
                        }
                        ));
                        break;
                    case "F9":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Dimming lights - Stopped Level";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
                        }
                        ));
                        break;
                    case "F7":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Dimming lights - Playback Level";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPlaybackLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPlaybackLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;
                        }
                        ));
                        break;
                        
                }
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        public FormMain()
        {
            hookID = SetHook(proc);
            formMain = this;
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form formSettings = new FormSettings();
            formSettings.ShowDialog();
            if (currentPLMport != Properties.Settings.Default.plmPort)
            {
                currentPLMport = Properties.Settings.Default.plmPort;
                connectPLM();
            }

            if (currentHarmonyIP != Properties.Settings.Default.harmonyHubIP)
            {
                currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
                await ConnectAsync();
            }

        }

        public int processReceivedMessage(string message, string address)
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
                    level = getLightLevel(address);
                    break;
                default:
                    level = getLightLevel(address);
                    break;
            }
            return level;
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            await ConnectAsync();
            Program.Client.OnActivityChanged += Client_OnActivityChanged;

            formMain.kinectIsAvailable = false;
            formMain.currentPLMport = Properties.Settings.Default.plmPort;
            formMain.connectPLM();

            formMain.currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;

            SpeechRecognitionEngine recognitionEngine = new SpeechRecognitionEngine();
            recognitionEngine.SetInputToDefaultAudioDevice();
            //recognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
            //recognitionEngine.EndSilenceTimeout = TimeSpan.FromSeconds(1.5);
            //recognitionEngine.BabbleTimeout = TimeSpan.FromSeconds(5);

            GrammarBuilder gb = new GrammarBuilder();
            Choices commandChoice = new Choices();

            SemanticResultValue commandSemantic = new SemanticResultValue("Turn on Projector", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("projector on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("home theater on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("let's watch a movie", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("start theater", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on projector", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on theater", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn off projector", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("projector off", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("home theater off", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("shutdown theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn off theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power down theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power off theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            recognitionEngine.LoadGrammar(grammar);
            recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;

            recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            if (Program.Client.Token != "")
            {
                formMain.labelHarmonyStatus.Text = "Connected";
                formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }
            else
            {
                formMain.labelHarmonyStatus.Text = "Disconnected";
                formMain.labelHarmonyStatus.ForeColor = System.Drawing.Color.Maroon;
            }
 
        }

        private async Task ConnectAsync()
        {
            Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
            var currentActivityID = await Program.Client.GetCurrentActivityAsync();
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.updateActivities(currentActivityID);
            }
            ));
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Alternates != null && labelRoomStatus.Text == "Occupied")
            {
                toolStripStatus.Text = e.Result.Text;
                RecognizedPhrase phrase = e.Result.Alternates[0];
                string topPhrase = phrase.Semantics.Value.ToString();
                switch (topPhrase)
                {
                    case "Turn on Theatre":
                        labelLastVoiceCommand.Text = phrase.Semantics.Value.ToString();
                        toolStripStatus.Text = "Starting Home Theatre";
                        startActivityByName("Turn on Theatre");
                        break;
                    case "Turn off Theatre":
                        if (labelKodiStatus.Text == "Stopped")
                        {
                            labelLastVoiceCommand.Text = phrase.Semantics.Value.ToString();
                            toolStripStatus.Text = "Stopping Home Theatre";
                            startActivityByName("PowerOff");
                        }
                        break;
                }
            }
        }

        private async void Client_OnActivityChanged(object sender, string e)
        {
            var activity = await Program.Client.GetCurrentActivityAsync();    
            formMain.BeginInvoke(new Action(() => formMain.updateActivities(activity)));
        }

        private async void updateActivities(string currentActivityID)
        {
            bool keep_looping = true;
            while (keep_looping)
            {
                //Fetch our config
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
                    await ConnectAsync();
                }
            }
        }

        private void listBoxActivities_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Activities activity = (Activities)listBoxActivities.SelectedItem;
            startActivity(activity);
        }

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
                    await ConnectAsync();
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
                            formMain.toolStripStatus.Text = "Failed to start Harmony activity - " + currItem.Text;
                        }
                        ));
                    }
                }
            }
        }

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
                //powerlineModem.OnError += PowerlineModem_OnError;

                //MessageBox.Show("Received Command: " + eventReceive.Description
                //+ ", from " + eventReceive.PeerId.ToString());

                powerlineModem.Network.StandardMessageReceived
                    += new StandardMessageReceivedHandler((s, eventReceive) =>
                {
                    string desc = eventReceive.Description;
                    string address = eventReceive.PeerId.ToString();

                    int level;

                    if (address == Properties.Settings.Default.trayAddress)
                    {
                        level = processReceivedMessage(desc, address);
                        if (level >= 0)
                        {
                            trackBarTray.Value = level;
                        }
                    }

                    else if (address == Properties.Settings.Default.potsAddress)
                    {
                        level = processReceivedMessage(desc, address);
                        if (level >= 0)
                        {
                            trackBarPots.Value = level;
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
                float decLevel = (float)integerLevel / 254 *10;
                level = (int)decLevel;
            }
            
            return level;
        }

        public void setLightLevel(string address, int level)
        {
            DeviceBase device;
            if (powerlineModem.Network.TryConnectToDevice(address, out device))
            {
                var lightingControl = device as DimmableLightingControl;
                float theVal = level * 254 / 10;               
                int toInt = (int)theVal;
                Boolean retVal = lightingControl.RampOn((byte)toInt);
                Thread.Sleep (500);
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

        private void trackBarTray_ValueChanged(object sender, EventArgs e)
        {
            labelTray.Text = (trackBarTray.Value * 10).ToString() + "%";
        }

        private void trackBarPots_ValueChanged(object sender, EventArgs e)
        {
            labelPots.Text = (trackBarPots.Value * 10).ToString() + "%";
        }

        private void timerPotTrack_Tick(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            setLightLevel(Properties.Settings.Default.potsAddress, trackBarPots.Value);
        }

        private void timerTrayTrack_Tick(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            setLightLevel(Properties.Settings.Default.trayAddress, trackBarTray.Value);
        }

        private void trackBarTray_Scroll(object sender, EventArgs e)
        {
            timerTrayTrack.Enabled = false;
            timerTrayTrack.Enabled = true;
        }

        private void trackBarPots_Scroll(object sender, EventArgs e)
        {
            timerPotTrack.Enabled = false;
            timerPotTrack.Enabled = true;
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

        private void timerKodiPoller_Tick(object sender, EventArgs e)
        {
            if (File.Exists("kodi_status.txt"))
            {
                string kodiPlayback = File.ReadAllText("kodi_status.txt").Trim().ToLower();
                File.Delete("kodi_status.txt");
                switch (kodiPlayback)
                {
                    case "playing":
                        labelKodiStatus.Text = "Playing";
                        toolStripStatus.Text = "Setting lights to Playback Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPlaybackLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsPlaybackLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPlaybackLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayPlaybackLevel;
                        break;
                    case "stopped":
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
                        break;
                    case "paused":
                        labelKodiStatus.Text = "Paused";
                        toolStripStatus.Text = "Setting lights to Paused Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPausedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPausedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;
                        break;
                    default:
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
                        break;
                }
            }
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
            timerClearStatus.Enabled = false;
        }

        private void timercheckKinect_Tick(object sender, EventArgs e)
        {
            if (kinectIsAvailable == true && KinectSensor.KinectSensors.Count == 0)
            {
                kinectIsAvailable = false;
                labelKinectStatus.Text = "Disconnected";
                labelKinectStatus.ForeColor = System.Drawing.Color.Maroon;
                timerSkeletonTracker.Enabled = false;
            }
            else if (kinectIsAvailable == false && KinectSensor.KinectSensors.Count > 0)
            {
                kinectIsAvailable = true;
                labelKinectStatus.Text = "Connected";
                labelKinectStatus.ForeColor = System.Drawing.Color.ForestGreen;
                kinect = KinectSensor.KinectSensors[0];
                kinect.Start();
                kinect.SkeletonStream.Enable();
                
                kinect.SkeletonFrameReady += Kinect_SkeletonFrameReady;
            }
        }

        private void Kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            timerSkeletonTracker.Enabled = true;
            if (labelRoomStatus.Text == "Unoccupied")
            {
                //Room is state changing to occupied
                labelRoomStatus.Text = "Occupied";

            }
        }

        private void timerSkeletonTracker_Tick(object sender, EventArgs e)
        {
            if (kinect.SkeletonStream.FrameSkeletonArrayLength == 0)
            {
                //Room state is changing to unoccupied
                labelRoomStatus.Text = "Unoccupied";
            }
        }

        private void labelRoomStatus_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomStatus.Text == "Occupied")
            {
                timerUnoccupiedRoom.Enabled = false;
                timerShutdown.Enabled = false;
                if (labelCurrentActivity.Text == "PowerOff")
                {
                    toolStripStatus.Text = "Turning on pot lights";
                    setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                    trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
                    toolStripStatus.Text = "Turning on tray lights";
                    setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                    trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                }
            }
            else if (labelRoomStatus.Text == "Unoccupied")
            {
                toolStripStatus.Text = "Starting shutdown timer";
                timerUnoccupiedRoom.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            labelRoomStatus.Text = "Occupied";
        }

        private async void timerUnoccupiedRoom_Tick(object sender, EventArgs e)
        {
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.timerUnoccupiedRoom.Enabled = false;
            }
            ));
            if (labelKodiStatus.Text == "Stopped")
            { 
                if (labelCurrentActivity.Text != "PowerOff")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.toolStripStatus.Text = "Turning off Home Theatre";
                    }
                    ));
                    try
                    {
                        await Program.Client.TurnOffAsync();
                    }
                    catch
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.toolStripStatus.Text = "Failed to turn off Harmony activity";
                        }
                        ));
                    }

                }
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.timerShutdown.Interval = (Properties.Settings.Default.shutdownTimer * 1000 * 60) + 1000;
                    formMain.timerShutdown.Enabled = true;
                }
                ));
            }
        }

        private void timerShutdown_Tick(object sender, EventArgs e)
        {
            timerShutdown.Enabled = false;
            toolStripStatus.Text = "Turning off lights";
            setLightLevel(Properties.Settings.Default.potsAddress, 0);
            trackBarPots.Value = 0;
            setLightLevel(Properties.Settings.Default.trayAddress, 0);
            trackBarTray.Value = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            labelRoomStatus.Text = "Unoccupied";
        }

        private void toolStripStatus_TextChanged(object sender, EventArgs e)
        {
            timerClearStatus.Enabled = false;
            timerClearStatus.Enabled = true;
        }

        private void timerStartLights_Tick(object sender, EventArgs e)
        {
            timerStartLights.Enabled = false;
            toolStripStatus.Text = "Setting lights to Stopped Level";
            setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
            trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
            setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
            trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Client.Dispose();
            if (powerlineModem != null)
            {
                powerlineModem.Dispose();
            }
            UnhookWindowsHookEx(hookID);
        }
    }
}
