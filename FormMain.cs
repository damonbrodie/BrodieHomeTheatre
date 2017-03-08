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
        //41.66.88 Motion Sensor

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        public static LowLevelKeyboardProc proc = HookCallback;
        public static IntPtr hookID = IntPtr.Zero;

        public string currentHarmonyIP;
        public string currentPLMport;
        public bool plmConnected;

        public DateTime GlobalShutdown;
        public bool globalShutdownActive;

        public bool projectorConnected;
        public string projectorLastCommand;

        public DateTime LatchTime;
        public bool latchActive;

        public SpeechRecognitionEngine recognitionEngine = new SpeechRecognitionEngine();

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
                            formMain.toolStripStatus.Text = "Keypress F12 captured - Turning on lights";
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
                            formMain.toolStripStatus.Text = "Keypress F11 captured - Turning off lights";
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
                            formMain.toolStripStatus.Text = "Keypress F9 captured - Dimming lights - Stopped Level";
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
                            formMain.toolStripStatus.Text = "Keypress F7 captured - Dimming lights - Playback Level";
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
            resetGlobalTimer();
            LatchTime = DateTime.Now;
            latchActive = false;
            projectorConnected = false;

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
                await ConnectHarmonyAsync();
            }
        }

        public int processDimmerMessage(string message, string address)
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

        public bool processMotionSensorMessage(string message, string address)
        {
            bool state = false;
            switch (message)
            {
                case "Turn On":
                    state = true;
                    break;
            }
            return state;
        }

        private async void FormMain_Load(object sender, EventArgs e)
        {
            await ConnectHarmonyAsync();
            Program.Client.OnActivityChanged += harmonyClient_OnActivityChanged;

            currentPLMport = Properties.Settings.Default.plmPort;
            connectPLM();

            ConnectProjector();

            if (projectorConnected)
            {
                checkProjectorPower();
            }

            currentHarmonyIP = Properties.Settings.Default.harmonyHubIP;
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

        private async Task ConnectHarmonyAsync()
        {
            Program.Client = await HarmonyClient.Create(Properties.Settings.Default.harmonyHubIP);
            var currentActivityID = await Program.Client.GetCurrentActivityAsync();
            formMain.BeginInvoke(new Action(() =>
            {
                formMain.harmonyUpdateActivities(currentActivityID);
            }
            ));
        }

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
                    break;
            }
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Alternates != null && labelRoomOccupancy.Text == "Occupied" && labelKodiStatus.Text != "Playing")
            {
                toolStripStatus.Text = e.Result.Text;
                RecognizedPhrase phrase = e.Result.Alternates[0];
                
                string topPhrase = phrase.Semantics.Value.ToString();

                switch (topPhrase)
                {
                    case "Turn on Theatre":

                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelLastVoiceCommand.Text = topPhrase;
                            toolStripStatus.Text = "Starting Home Theatre";
                            startActivityByName(Properties.Settings.Default.voiceActivity);
                            formMain.timerStartLights.Enabled = true;
                        }
                        ));
                        break;
                    case "Turn off Theatre":
                        formMain.BeginInvoke(new Action(() =>
                        {

                            labelLastVoiceCommand.Text = phrase.Semantics.Value.ToString();
                            toolStripStatus.Text = "Stopping Home Theatre";
                            startActivityByName("PowerOff");
                            formMain.toolStripStatus.Text = "Turning on pot lights";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
                            formMain.toolStripStatus.Text = "Turning on tray lights";
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                        }
                        ));
                        break;
                }
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
                                formMain.timerGlobal.Enabled = false;
                                globalShutdownActive = false;
                            }
                            else
                            {
                                formMain.timerGlobal.Enabled = true;
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
                        level = processDimmerMessage(desc, address);
                        if (level >= 0)
                        {
                            trackBarTray.Value = level;
                        }
                    }

                    else if (address == Properties.Settings.Default.potsAddress)
                    {
                        level = processDimmerMessage(desc, address);
                        if (level >= 0)
                        {
                            trackBarPots.Value = level;
                        }
                    }
                    else if (address == Properties.Settings.Default.motionSensorAddress)
                    {
                        if (processMotionSensorMessage(desc, address))
                        { //Motion Detected
                            timerMotionLatch.Enabled = false;
                            latchActive = false;
                            labelMotionSensorStatus.Text = "Motion Detected";
                            labelRoomOccupancy.Text = "Occupied";
                            progressBarMotionLatch.Value = progressBarMotionLatch.Maximum;
                        }
                        else
                        { //No Motion Detected
                            if (labelRoomOccupancy.Text == "Occupied")
                            {
                                labelMotionSensorStatus.Text = "No Motion";
                                LatchTime = DateTime.Now.AddMinutes(Properties.Settings.Default.motionSensorLatch);
                                latchActive = true;
                                timerMotionLatch.Enabled = false;
                                timerMotionLatch.Enabled = true;
                            }
                            else
                            {
                                labelRoomOccupancy.Text = "Vacant";
                            }
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
            if (powerlineModem != null && powerlineModem.Network.TryConnectToDevice(address, out device))
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
            if (File.Exists("kodi_ar.txt"))
            {
                string kodiAspectRatio = File.ReadAllText("kodi_ar.txt").Trim().ToLower();
                File.Delete("kodi_ar.txt");
                projectorChangeAspect(float.Parse(kodiAspectRatio));
            }
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
 
                        resetGlobalTimer();

                        break;
                    case "stopped":
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;

                        resetGlobalTimer();

                        break;
                    case "paused":
                        labelKodiStatus.Text = "Paused";
                        toolStripStatus.Text = "Setting lights to Paused Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsPausedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsPausedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayPausedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayPausedLevel;

                        resetGlobalTimer();

                        break;
                    default:
                        labelKodiStatus.Text = "Stopped";
                        toolStripStatus.Text = "Setting lights to Stopped Level";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsStoppedLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsStoppedLevel;
                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayStoppedLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayStoppedLevel;

                        resetGlobalTimer();

                        break;
                }
            }
        }

        private void timerClearStatus_Tick(object sender, EventArgs e)
        {
            toolStripStatus.Text = "";
            //timerClearStatus.Enabled = false;
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

        /*
         * This timer runs when ever:
         *   - The lights get turned on
         *   - A Harmony Activity is enabled
         */

        private void timerGlobal_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime globalShutdownStart = GlobalShutdown.AddHours(Properties.Settings.Default.globalShutdown * -1);
            var totalSeconds = (GlobalShutdown - globalShutdownStart).TotalSeconds;
            var progress = (now - globalShutdownStart).TotalSeconds;
            if (globalShutdownActive && GlobalShutdown > now && labelCurrentActivity.Text != "PowerOff")
            {
                int percentage = (100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1));
                if (percentage <= 1)
                {
                    globalShutdownActive = false;
                    timerGlobal.Enabled = false;
                    startActivityByName("PowerOff");
                    toolStripProgressBarGlobal.Value = 0;
                }
                else
                {
                    toolStripProgressBarGlobal.Value = percentage;
                }
            }
            else
            {
                toolStripProgressBarGlobal.Value = toolStripProgressBarGlobal.Maximum;
            }
        }

        private void timerMotionLatch_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            DateTime latchStart = LatchTime.AddMinutes(Properties.Settings.Default.motionSensorLatch * -1);
            var totalSeconds = (LatchTime - latchStart).TotalSeconds;
            var progress = (now - latchStart).TotalSeconds;

            if (latchActive && LatchTime > now)
            {
                int percentage = 100 - (Convert.ToInt32((progress / totalSeconds) * 100) + 1);
                if (percentage <= 1)
                {
                    labelRoomOccupancy.Text = "Vacant";
                    latchActive = false;
                }
                else
                {
                    progressBarMotionLatch.Value = percentage;
                }
            }
            else
            {
                progressBarMotionLatch.Value = 0;
            }
        }

        private async void labelRoomOccupancy_TextChanged(object sender, EventArgs e)
        {
            if (labelRoomOccupancy.Text == "Occupied")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    resetGlobalTimer();
                    timerShutdown.Enabled = false;
                    toolStripStatus.Text = "Room is now occupied";

                    // Power on the Amplifier
                    harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyEnterCommand);
                }
                ));


                if (labelCurrentActivity.Text == "PowerOff")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        toolStripStatus.Text = "Turning on lights";
                        setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                        trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;

                        setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                        trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                    }
                    ));
                }
            }
            else if (labelRoomOccupancy.Text == "Vacant")
            {
                if (labelKodiStatus.Text == "Stopped")
                {
                    if (labelCurrentActivity.Text != "PowerOff")
                    {
                        // Turn off active Harmony Activity
                        await Program.Client.StartActivityAsync("-1");
                    }
                    else
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            // Power off the Amplifier
                            harmonySendCommand(Properties.Settings.Default.occupancyDevice, Properties.Settings.Default.occupancyExitCommand);
                        }
                        ));
                    }

                    formMain.BeginInvoke(new Action(() =>
                    {
                        toolStripStatus.Text = "Room is now vacated";
                        toolStripStatus.Text = "Turning off lights";
                        setLightLevel(Properties.Settings.Default.potsAddress, 0);
                        trackBarPots.Value = 0;
                        setLightLevel(Properties.Settings.Default.trayAddress, 0);
                        trackBarTray.Value = 0;
                    }
                    ));
                }
                else // There is playback or it is paused.  Start the timer to shut this off after 2 hours
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        resetGlobalTimer();
                    }
                    ));
                }
            }
        }

        private void resetGlobalTimer()
        {
            GlobalShutdown = DateTime.Now.AddHours(Properties.Settings.Default.globalShutdown);
            globalShutdownActive = true;
            timerGlobal.Enabled = false;
            timerGlobal.Interval = 1000;
            timerGlobal.Enabled = true;
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
                projectorChangeAspect((float)1.0); //Narrow
            }
            else
            {
                projectorChangeAspect((float)2.0); //Wide
            }

        }

        private void timerCheckProjector_Tick(object sender, EventArgs e)
        {
            checkProjectorPower();
        }

        private void projectorChangeAspect(float aspect)
        {
            List<string> pj_codes = new List<string> {
                "VXX:LMLI0=+00000" ,
                "VXX:LMLI0=+00001",
                "VXX:LMLI0=+00002",
                "VXX:LMLI0=+00003" ,
                "VXX:LMLI0=+00004",
                "VXX:LMLI0=+00005" };
            projectorLastCommand = "Lens";
            if (aspect < 1.8)
            {
                ProjectorSendCommand(pj_codes[0]);
                labelLensAspect.Text = "Narrow";
            }
            else
            {
                ProjectorSendCommand(pj_codes[1]);
                labelLensAspect.Text = "Wide";
            }

        }
        private void projectorPowerOn()
        {
            ProjectorSendCommand("PON");
            labelProjectorPower.Text = "Powering On";
        }

        private void projectorPowerOff()
        {
            ProjectorSendCommand("POF");
            labelProjectorPower.Text = "Powering Off";
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
