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
        public MovieEntry kodiPlayNext = null;
        public TcpClient tcpClient;
        public NetworkStream kodiSocketStream;
        public StreamReader kodiStreamReader;
        public StreamWriter kodiStreamWriter;
        public char[] kodiReadBuffer = new char[1000000];
        public int kodiReadBufferPos = 0;
        public class MovieEntry
        {
            public string file { get; set; }
            public string name { get; set; }
            public string cleanName { get; set; }
            public List<string> shortNames = new List<string>();
            public bool resume { get; set; } = false;
        }
        public List<MovieEntry> kodiMovies = new List<MovieEntry>();
        public class PartialMovieEntry
        {
            public string file { get; set; }
            public string name { get; set; }
        } 
        
        public class tvShowEntry
        {
            public string name { get; set; }
            public string cleanName { get; set; }
            public int id { get; set; }
            public string file { get; set; }
        }

        public List<PartialMovieEntry> moviesFullNames          = new List<PartialMovieEntry>();
        public List<PartialMovieEntry> moviesAfterColonNames    = new List<PartialMovieEntry>();
        public List<PartialMovieEntry> moviesPartialNames       = new List<PartialMovieEntry>();
        public List<PartialMovieEntry> moviesDuplicateNames     = new List<PartialMovieEntry>();

        public List<tvShowEntry> kodiTVShows            = new List<tvShowEntry>();
        public List<tvShowEntry> tvshowPartialNames     = new List<tvShowEntry>();
        public List<tvShowEntry> tvhowDuplicateNames    = new List<tvShowEntry>();

        public bool kodiLoadingMedia = false;

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
                            writeLog("Kodi:  Connected to Kodi JSON port");
                        }
                        kodiIsConnected = true;
                        labelKodiStatus.Text = "Connected";
                        labelKodiStatus.ForeColor = System.Drawing.Color.ForestGreen;
                        kodiSendGetMoviesRequest();
                        kodiSendGetTVShowsRequest();
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
                formMain.writeLog("Kodi:  Exiting JSON read thread");
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
            Dictionary<string, dynamic> result = null;
            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonText);
            }
            catch
            {
                writeLog("Kodi:  Unable to decode JSON:  " + jsonText);
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
                        writeLog("Kodi:  Playback status incorrect - No players active");
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] != 0 && labelKodiPlaybackStatus.Text != "Playing")
                    {
                        labelKodiPlaybackStatus.Text = "Playing";
                        writeLog("Kodi:  Playback status incorrect - Player is running");
                    }
                    else if (result["result"]["currentvideostream"]["codec"] != string.Empty && result["result"]["speed"] == 0 && labelKodiPlaybackStatus.Text != "Paused")
                    {
                        labelKodiPlaybackStatus.Text = "Paused";
                        writeLog("Kodi:  Playback status incorrect - Player is paused");
                    }
                }
                catch
                {
                    writeLog("Kodi:  Error parsing Kodi JSON");
                }
            }
            else if (result.ContainsKey("id") && result["id"] == "98")
            {
                //writeLog("Kodi:  Received list of movies");
                kodiLoadingMedia = true;
                kodiMovies.Clear();
                int movieCounter = 0;

                moviesFullNames.Clear();
                moviesAfterColonNames.Clear();
                moviesDuplicateNames.Clear();
                moviesPartialNames.Clear();
                bool error = false;
                try
                {
                    foreach (JObject movie in result["result"]["movies"])
                    {
                        MovieEntry movieEntry = new MovieEntry();
                        if (movie["file"] != null && movie["label"] != null)
                        {
                            movieEntry.file = movie["file"].ToString();
                            movieEntry.name = movie["label"].ToString();
                            //writeLog("Kodi:  Processing '" + movieEntry.name + "'");
                            string cleanName = cleanString(movieEntry.name);
                            movieEntry.cleanName = cleanName;
                            kodiMovies.Add(movieEntry);
                            PartialMovieEntry tempEntry = new PartialMovieEntry();
                            tempEntry.file = movieEntry.file;
                            tempEntry.name = cleanName;
                            moviesFullNames.Add(tempEntry);

                            // Some movies are more easily known by the part that comes after the : in a title
                            // For Example:  The Lord of the Rings: The Fellowship of the Ring
                            if (movieEntry.name.Contains(":"))
                            {
                                string[] splitted = movieEntry.name.Split(new char[] { ':' }, 2);
                                string afterColon = cleanString(splitted[1]);

                                if (!searchMovieList(moviesFullNames, afterColon))
                                {
                                    PartialMovieEntry tempColonEntry = new PartialMovieEntry();
                                    tempColonEntry.file = movieEntry.file;
                                    tempColonEntry.name = afterColon;
                                    if (searchMovieList(moviesAfterColonNames, afterColon))
                                    {
                                        moviesDuplicateNames.Add(tempColonEntry);
                                    }
                                    else
                                    {
                                        moviesAfterColonNames.Add(tempColonEntry);
                                    }
                                }
                            }
                            List<string> cutNames = getShortTitles(cleanName);
                            foreach (string partName in cutNames)
                            {
                                PartialMovieEntry tempPrefixEntry = new PartialMovieEntry();
                                tempPrefixEntry.file = movieEntry.file;
                                tempPrefixEntry.name = partName;
                                if (searchMovieList(moviesPartialNames, partName))
                                {
                                    moviesDuplicateNames.Add(tempPrefixEntry);
                                }
                                else
                                {
                                    moviesPartialNames.Add(tempPrefixEntry);
                                }
                            }
                            movieCounter += 1;
                        }
                        else
                        {
                            error = true;
                        }
                    }
                    if (!error)
                    {
                        toolStripStatus.Text = "Kodi movie list updated: " + movieCounter.ToString() + " movies";
                        kodiLoadingMedia = false;
                        labelKodiMediaAvailable.Text = kodiMovies.Count.ToString() + " movies && " + kodiTVShows.Count.ToString() + " shows";
                    }
                    else
                    {
                        writeLog("Kodi:  There was an error decoding the Kodi movie library JSON");
                    }
                }
                catch
                {
                    writeLog("Kodi:  Failed to process Movie JSON");
                }
                kodiLoadingMedia = false;
            }
            else if (result.ContainsKey("id") && result["id"] == "97")
            {
                //writeLog("Kodi:  Received list of tv shows");

                //writeLog("Kodi:  " + jsonText);
                kodiLoadingMedia = true;
                kodiTVShows.Clear();

                tvhowDuplicateNames.Clear();
                tvshowPartialNames.Clear();
                bool error = false;
                try
                {
                    foreach (JObject tvshow in result["result"]["tvshows"])
                    {
                        tvShowEntry tvShowEntry = new tvShowEntry();
                        if (tvshow["tvshowid"] != null && tvshow["title"] != null)
                        {
                            tvShowEntry.name = tvshow["title"].ToString();
                            tvShowEntry.id = (int)tvshow["tvshowid"];
                            //writeLog("Kodi:  Processing '" + tvShowEntry.name + "'");
                            string cleanName = cleanString(tvShowEntry.name);
                            tvShowEntry.cleanName = cleanName;
                            kodiTVShows.Add(tvShowEntry);

                           
                            List<string> cutNames = getShortTitles(cleanName);
                            foreach (string partName in cutNames)
                            {
                                tvShowEntry tempPrefixEntry = new tvShowEntry();
                                tempPrefixEntry.id = tvShowEntry.id;
                                tempPrefixEntry.name = partName;
                                if (searchTVShowList(tvshowPartialNames, partName))
                                {
                                    tvhowDuplicateNames.Add(tempPrefixEntry);
                                }
                                else
                                {
                                    tvshowPartialNames.Add(tempPrefixEntry);
                                }
                            }
                        }
                        else
                        {
                            error = true;
                        }
                    }
                    if (!error)
                    {
                        toolStripStatus.Text = "Kodi tv show list updated: " + kodiTVShows.Count.ToString() + " shows";
                        kodiLoadingMedia = false;
                        labelKodiMediaAvailable.Text = kodiMovies.Count.ToString() + " movies && " + kodiTVShows.Count.ToString() + " shows";
                    }
                    else
                    {
                        writeLog("Kodi:  There was an error decoding the Kodi tv show library JSON");
                    }
                }
                catch
                {
                    writeLog("Kodi:  Failed to process TV show JSON");
                }
                kodiLoadingMedia = false;
            }
            else if (result.ContainsKey("method"))
            {
                switch (result["method"])
                {
                    case "Player.OnPause":
                        writeLog("Kodi:  Kodi status changed to 'Paused'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Paused";
                        lightsToPausedLevel();
                        resetGlobalTimer();
                        break;
                    case "Player.OnPlay":
                        writeLog("Kodi:  Kodi status changed to 'Playing'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Playing";
                        lightsToPlaybackLevel();
                        resetGlobalTimer();
                        break;
                    case "Player.OnStop":
                        writeLog("Kodi:  Kodi status changed to 'Stopped'");
                        insteonDoMotion(false);
                        labelKodiPlaybackStatus.Text = "Stopped";
                        lightsToStoppedLevel();
                        resetGlobalTimer();
                        break;
                    case "System.OnQuit":
                        writeLog("Kodi:  Kodi is exiting");
                        kodiStatusDisconnect();
                        break;
                    case "VideoLibrary:OnScanStarted":
                        writeLog("Kodi:  Kodi library updated");
                        kodiSendGetMoviesRequest();
                        kodiSendGetTVShowsRequest();
                        break;
                    case "Other.aspectratio":
                        if (result["params"]["sender"] == "brodietheatre")
                        {
                            string kodiAspectRatio = result["params"]["data"];
                            try
                            {
                                projectorQueueChangeAspect(float.Parse(kodiAspectRatio));
                            }
                            catch (FormatException)
                            {
                                writeLog("Kodi:  Invalid Aspect Ratio: '" + kodiAspectRatio + "'");
                            }
                        }
                        break;
                }
            }
            else
            {
                writeLog("Kodi:  Received unknown JSON:  " + jsonText);
            }
        }

        public bool searchMovieList(List<PartialMovieEntry> theList, string searchTerm)
        {
            bool found = false;
            foreach (PartialMovieEntry entry in theList)
            {
                if (entry.name == searchTerm)
                {
                    return true;
                }
            }
            return found;
        }

        public bool searchTVShowList(List<tvShowEntry> theList, string searchTerm)
        {
            bool found = false;
            foreach (tvShowEntry entry in theList)
            {
                if (entry.name == searchTerm)
                {
                    return true;
                }
            }
            return found;
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
                writeLog("Kodi:  Connection closed to Kodi JSON port");
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

        public void kodiSendGetMoviesRequest()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"VideoLibrary.GetMovies\", \"params\": { \"properties\" : [\"file\"] }, \"id\": \"98\"}");
        }

        public void kodiSendGetTVShowsRequest()
        {
            kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"VideoLibrary.GetTVShows\", \"params\": {\"properties\": [\"title\"] }, \"id\": \"97\"}");
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

        private void timerKodiStartPlayback_Tick(object sender, EventArgs e)
        {
            timerKodiStartPlayback.Enabled = false;
            if (kodiPlayNext != null)
            {
                string resume = "";
                if (kodiPlayNext.resume)
                {
                    resume = ", \"options\": {\"resume\": true }";
                }
                kodiSendJson("{\"jsonrpc\": \"2.0\", \"method\": \"Player.Open\", \"params\": { \"item\": {\"file\": \"" + kodiPlayNext.file + "\" } " + resume + "}, \"id\": \"1\"}");
                writeLog("Kodi:  Starting movie: " + kodiPlayNext.name + " " + kodiPlayNext.file);
                kodiPlayNext = null;
            }
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
                kodiSendGetMoviesRequest();
                kodiSendGetTVShowsRequest();
            }
        }
    }
}