using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public string kodiWaveFile = @"c:\Users\damon\Documents\Shared\wavefile.txt";
        public TcpClient tcpClient;
        NetworkStream kodiSocketStream;
        public StreamReader kodiStreamReader;
        public StreamWriter kodiStreamWriter;
        public char[] kodiReadBuffer = new char[1000000];
        public int kodiReadBufferPos = 0;
        class MovieEntry
        {
            public string file { get; set; }
            public string name { get; set; }
        }
        List<MovieEntry> kodiMovies = new List<MovieEntry>();

        private void kodiPlayWave(string file)
        {
            bool success = false;
            int counter = 0;
            while (!success && counter < 3)
            {
                try
                {
                    StreamWriter fileHandle = new StreamWriter(kodiWaveFile);
                    fileHandle.WriteLine(Path.Combine(wavePath, file));
                    fileHandle.Close();
                    success = true;
                }
                catch
                {
                    Thread.Sleep(50);
                    counter += 1;
                }
            }
        }

        private void kodiConnect()
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
                labelKodiStatus.Text = "Connected";
                labelKodiStatus.ForeColor = System.Drawing.Color.ForestGreen;
            }
            else
            {
                kodiStatusDisconnect();
            }
        }

        public async void kodiReadStream()
        {
            char[] buffer = new char[1000];
            //int bytesRead = 0;
            bool ended = false;
            while (!ended)
            {
                int bytesRead = await kodiStreamReader.ReadAsync(buffer, 0, 1000);
                Array.Copy(buffer, 0, kodiReadBuffer, kodiReadBufferPos, bytesRead);
                kodiReadBufferPos += bytesRead;
                kodiFindJson();
            }
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
                    }
                    ));

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
            Dictionary<string, dynamic > result = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonText);
            if (result.ContainsKey("method"))
            {
                switch (result["method"])
                {
                    case "Player.OnPause":
                        writeLog("Kodi:  Kodi status changed to 'Paused'");
                        labelKodiPlaybackStatus.Text = "Paused";
                        lightsToPausedLevel();
                        resetGlobalTimer();
                        break;
                    case "Player.OnPlay":
                        writeLog("Kodi:  Kodi status changed to 'Playing'");
                        labelKodiPlaybackStatus.Text = "Playing";
                        lightsToPlaybackLevel();
                        resetGlobalTimer();
                        break;
                    case "Player.OnStop":
                        writeLog("Kodi:  Kodi status changed to 'Stopped'");
                        labelKodiPlaybackStatus.Text = "Stopped";
                        lightsToStoppedLevel();
                        resetGlobalTimer();
                        break;
                    case "Other.aspectratio":
                        if (result["params"]["sender"] == "brodietheatre")
                        {
                            string kodiAspectRatio = result["params"]["data"];
                            projectorQueueChangeAspect(float.Parse(kodiAspectRatio));
                        }
                        break;
                }
            }
            else if(result.ContainsKey("result") && result["result"]["movies"] != null)
            {
                writeLog("Kodi:  Received list of movies");
                kodiMovies.Clear();
                foreach (JObject movie in result["result"]["movies"])
                {
                    MovieEntry movieEntry = new MovieEntry();
                    movieEntry.file = movie["file"].ToString();
                    movieEntry.name = movie["label"].ToString();
                    kodiMovies.Add(movieEntry);              
                }
                loadVoiceCommands();
            }
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
            labelKodiStatus.Text = "Disconnected";
            labelKodiStatus.ForeColor = System.Drawing.Color.Maroon;
            timerKodiConnect.Enabled = enableTimer;
        }

        public void kodiSendGetMoviesRequest()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"VideoLibrary.GetMovies\", \"params\": { \"properties\" : [\"file\"] }, \"id\": \"1\"}");
        }

        private void kodiPlaybackControl(string command, string media=null)
        {
            //kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.GetActivePlayers\", \"id\": \"1\"}");
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
    }
}