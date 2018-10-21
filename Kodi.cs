using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string kodiBehindScreen = @"smb://10.0.0.7/Pictures/ht_0.jpg";
        public string mediaDolbyDemo   = @"smb://10.0.0.7/Demos/Intros/Dolby Atmos - Amaze.m2ts";
        public string mediaTHXDemo     = @"smb://10.0.0.7/Demos/Intros/THX - Amazing Life.mkv";

        public bool kodiIsConnected = false;
        public string currentKodiIP = "";
        public int currentKodiPort = 0;
        public TcpClient tcpClient;
        public NetworkStream kodiSocketStream;
        public StreamReader kodiStreamReader;
        public StreamWriter kodiStreamWriter;
        public char[] kodiReadBuffer = new char[1000000];
        public int kodiReadBufferPos = 0;
        private void kodiConnect()
        {
            if (Properties.Settings.Default.kodiIP != String.Empty && Properties.Settings.Default.kodiJSONPort != 0)
            {
                try
                {
                    kodiStatusDisconnect(false);
                    tcpClient = new TcpClient();
                    tcpClient.ReceiveTimeout = 500;
                    var result = tcpClient.BeginConnect(Properties.Settings.Default.kodiIP, Properties.Settings.Default.kodiJSONPort, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    if (success)
                    {
                        kodiSocketStream = tcpClient.GetStream();
                        kodiStreamReader = new StreamReader(kodiSocketStream);
                        kodiStreamWriter = new StreamWriter(kodiSocketStream);
                        kodiSocketStream.Flush();
                        Thread thread = new Thread(kodiReadStream);
                        thread.Start();
                        if (!kodiIsConnected)
                        {
                            Logging.writeLog("Kodi:  Connected to Kodi JSON port");
                        }
                        kodiIsConnected = true;
                        labelKodiStatus.Text = "Connected";
                        labelKodiStatus.ForeColor = System.Drawing.Color.ForestGreen;
                        timerKodiConnect.Interval = 2000;
                        return;
                    }
                }
                catch { }
            }
            kodiStatusDisconnect(); 
        }

        public async void kodiReadStream()
        {
            char[] buffer = new char[100000];
            //int bytesRead = 0;
            bool ended = false;
            bool gotSome = false;
            while (!ended)
            {
                try
                {
                    int bytesRead = await kodiStreamReader.ReadAsync(buffer, 0, 100000);
                    Array.Copy(buffer, 0, kodiReadBuffer, kodiReadBufferPos, bytesRead);
                    kodiReadBufferPos += bytesRead;
                    if (bytesRead > 0)
                    {
                        gotSome = true;
                    }
                    else
                    {
                        gotSome = false;
                    }
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiFindJson();
                    }));
                }
                catch
                {
                    ended = true;
                }
                if (gotSome)
                {
                    await doDelay(100);
                }
                else
                {
                    await doDelay(1000);
                }
            }
            formMain.BeginInvoke(new Action(() =>
            {
                Logging.writeLog("Kodi:  Exiting JSON read thread");
            }));
        }

        public void kodiFindJson()
        {
            int braces = 0;
            bool inQ = false;
            char lastB = ' ';

            int curPos = 0;
            int startPos = 0;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            while (curPos < kodiReadBufferPos)
            {
                char b = kodiReadBuffer[curPos];
                curPos += 1;
                sb.Append(b);

                if (b == '"' && lastB != '\\')
                {
                    inQ = !inQ;
                }
                else if (b == '{' && !inQ)
                {
                    braces += 1;
                }
                else if (b == '}' && !inQ)
                {
                    braces -= 1;
                }
                lastB = (char)b;
                if (braces == 0)
                {
                    int newBufferLength = kodiReadBufferPos - curPos;
                    string currJson = sb.ToString();
                    sb = new System.Text.StringBuilder();
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiProcessJson(currJson);
                    }));
                    startPos = curPos;
                }
            }
            if (braces > 0)
            {
                int newBufferLength = kodiReadBufferPos - startPos;
                Array.Copy(kodiReadBuffer, startPos, kodiReadBuffer, 0, newBufferLength);
                kodiReadBufferPos = newBufferLength;
            }
            else
            {
                kodiReadBufferPos = 0;
            }
        }

        public void kodiProcessJson(string jsonText)
        {      
            Dictionary<string, dynamic> result = null;
            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonText);
            }
            catch
            {
                Logging.writeLog("Kodi:  Unable to decode JSON:  " + jsonText);
                return;
            }
            if (result.ContainsKey("id") && result["id"] == "99")
            {
                // Our submitted request for Player Get Properties
                // PLAYING {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"vc1","height":1080,"index":0,"language":"","name":"FraMeSToR VC-1 Video","width":1920},"speed":1,"type":"video"}}
                // PAUSED  {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"vc1","height":1080,"index":0,"language":"","name":"FraMeSToR VC-1 Video","width":1920},"speed":0,"type":"video"}}
                // STOPPED {"id":"99","jsonrpc":"2.0","result":{"currentvideostream":{"codec":"","height":0,"index":0,"language":"","name":"","width":0},"speed":0,"type":"video"}}
                try
                {
                    if (result["result"]["currentvideostream"]["codec"] == string.Empty && labelKodiPlaybackStatus.Text != "Stopped")
                    {
                        labelKodiPlaybackStatus.Text = "Stopped";
                        Logging.writeLog("Kodi:  Playback status incorrect - No players active");
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] != 0 && labelKodiPlaybackStatus.Text != "Playing")
                    {
                        labelKodiPlaybackStatus.Text = "Playing";
                        Logging.writeLog("Kodi:  Playback status incorrect - Player is running");
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] == 0 && labelKodiPlaybackStatus.Text != "Paused")
                    {
                        labelKodiPlaybackStatus.Text = "Paused";
                        Logging.writeLog("Kodi:  Playback status incorrect - Player is paused");
                    }
                }
                catch
                {
                    Logging.writeLog("Kodi:  Error parsing Kodi JSON: '" + jsonText + "'");
                }
            }
            
            else if (result.ContainsKey("id") && result["id"] == "95")
            {
                // Don't process this any further at the moment
            }
            else if (result.ContainsKey("id") && result["id"] == "96")
            {
                // Don't process this any further at the moment
            }
  
            else if (result.ContainsKey("method"))
            {
                switch (result["method"])
                {
                    case "Player.OnPause":
                        Logging.writeLog("Kodi:  Kodi status changed to 'Paused'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Paused";
                        lightsToPausedLevel();
                        ResetGlobalTimer();
                        break;
                    case "Player.OnPlay":
                        Logging.writeLog("Kodi:  Kodi status changed to 'Playing'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Playing";
                        lightsToPlaybackLevel();
                        ResetGlobalTimer();
                        break;
                    case "Player.OnStop":
                        Logging.writeLog("Kodi:  Kodi status changed to 'Stopped'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Stopped";
                        lightsToStoppedLevel();
                        ResetGlobalTimer();
                        break;
                    case "System.OnQuit":
                        Logging.writeLog("Kodi:  Kodi is exiting");
                        kodiStatusDisconnect();
                        break;
                    case "Other.aspectratio":
                        if (result["params"]["sender"] == "brodietheatre")
                        {
                            string kodiAspectRatio = result["params"]["data"];
                            try
                            {
                                projectorQueueChangeAspect(float.Parse(kodiAspectRatio));
                                Logging.writeLog("Kodi:  Received Aspect Ratio: '" + kodiAspectRatio + "'");                            }
                            catch (FormatException)
                            {
                                Logging.writeLog("Kodi:  Invalid Aspect Ratio: '" + kodiAspectRatio + "'");
                            }
                        }
                        break;
                }
            }
            else
            {
                Logging.writeLog("Kodi:  Received unknown JSON:  " + jsonText);
            }
        }

        public List<string> getShortTitles(string name)
        {
            List<string> nameList = new List<string>();
            bool ended = false;
            bool firstOne = true;
            int startPos = 0;
            while (! ended)
            {
                int firstPos = name.IndexOf(" ", startPos);
                startPos = firstPos + 1;
                if (!firstOne)
                {
                    if (firstPos > 0)
                    {
                        string cut = name.Substring(0, firstPos);
                        nameList.Add(cut);
                    }
                    else
                    {
                        ended = true;
                    }
                }
                if (startPos >= name.Length)
                {
                    ended = true;
                }
                firstOne = false;
            }
            return nameList;
        }

        public string cleanString(string name)
        {
            string newName = Regex.Replace(name, @"\&", " and ");
            newName = Regex.Replace(newName, @"[^a-zA-Z0-9\s\']", " ");
            newName = Regex.Replace(newName, @"\s+", " ");
            return newName;
        }

        public void kodiSendJson(string command)
        {
            try
            {
                kodiStreamWriter.WriteLine(command);
                kodiStreamWriter.Flush();
            }
            catch (IOException)
            {
                timerKodiConnect.Enabled = true;
            }
            catch (NullReferenceException) { }
        }

        public void kodiStatusDisconnect(bool enableTimer = true)
        {
            if (kodiIsConnected)
            {
                Logging.writeLog("Kodi:  Connection closed to Kodi JSON port");
            }
            kodiIsConnected = false;
            labelKodiStatus.Text = "Disconnected";
            labelKodiStatus.ForeColor = System.Drawing.Color.Maroon;
            timerKodiConnect.Enabled = enableTimer;
        }

        public void kodiToggleFullscreen()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Input.ExecuteAction\", \"params\": { \"action\" : \"togglefullscreen\" }, \"id\": \"96\"}");
        }

        private void kodiPlaybackControl(string command, string media=null)
        {
            // It seems the Active Player is always "1".  Use this if we need to query it.
            //  "{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetActivePlayers\", \"id\": \"1\"}"
            switch (command)
            {
                case "Pause":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
                case "Play":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.PlayPause\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
                case "Stop":
                    kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Stop\", \"params\": { \"playerid\" : 1 }, \"id\": \"1\"}");
                    break;
            }
        }

        private void timerKodiConnect_Tick(object sender, EventArgs e)
        {
            kodiConnect();
        }

        private void kodiShowNotification(string title, string message, int displaytime = 5000)
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"GUI.ShowNotification\", \"params\": { \"title\" : \"" + title + "\", \"message\" : \"" + message + "\", \"displaytime\" : " + displaytime.ToString() + "}, \"id\": \"1\"}");
        }

        private void kodiShowBehindScreen()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\": {\"file\": \"" + kodiBehindScreen + "\" }}, \"id\": \"1\"}");
        }

        private void timerKodiPoll_Tick(object sender, EventArgs e)
        {
            // Periodically poll Kodi and retrieve the Player properties.  Use this to keep consistency
            // in our view of the current Kodi status

            if (labelKodiStatus.Text == "Connected")
            {
                kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetProperties\", \"params\": {\"playerid\": 1, \"properties\" : [\"type\", \"currentvideostream\", \"speed\"]}, \"id\": \"99\"}");
            }
        }
    }
}
