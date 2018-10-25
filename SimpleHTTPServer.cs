// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html

using System;
using System.Net;
using System.IO;
using System.Threading;

namespace BrodieTheatre
{
    public class SimpleHTTPServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        public SimpleHTTPServer(int port)
        {
            this.Initialize(port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            bool serverStarted = false;
            try
            {
                _listener.Start();
                serverStarted = true;
                Logging.writeLog("Web server starting on port: " + _port.ToString());
            }
            catch (Exception ex)
            {
                Logging.writeLog("Error:  Unable to start web server on port: " + _port.ToString() + " Netsh acl rule missing?");
                Logging.writeLog(ex.ToString());
            }
            while (serverStarted)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Logging.writeLog("Web Server Failed: " + ex.ToString());
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            // Knock off the initial slash
            string url = context.Request.Url.AbsolutePath.Substring(1);
            Logging.writeLog("HTTP Received:  " + url);

            if (url.Contains("/"))
            {
                string[] parts = url.Split(new char[] { '/' }, 2);
                switch (parts[0])
                {
                    case "command":
                        BrodieTheatre.Logging.writeLog("Received web request for command:  " + parts[1]);
                        switch (parts[1])
                        {
                            case "transparent":
                                Logging.writeLog("Received web request for showing behind the screen");
                                FormMain.kodiShowBehindScreen();
                                break;
                        }
                        break;
                    case "cast":
                        string filename = parts[1];

                        if (string.IsNullOrEmpty(filename))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }


                        Logging.writeLog("Received web request for casting:  " + filename);
                        if (FormMain.textToSpeechFiles.ContainsKey(filename))
                        {
                            try
                            {
                                Stream input = FormMain.textToSpeechFiles[filename];

                                input.Seek(0, 0);

                                //Adding permanent http response headers
                                context.Response.ContentType = "audio/mpeg";
                                context.Response.ContentLength64 = input.Length;
                                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                                context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));

                                byte[] buffer = new byte[1024 * 16];
                                int nbytes;

                                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                                }
                                input.Close();

                                context.Response.StatusCode = (int)HttpStatusCode.OK;
                                context.Response.OutputStream.Flush();

                                // Dispose of the audio Stream
                                FormMain.textToSpeechFiles.Remove(filename);
                            }
                            catch (Exception ex)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                Logging.writeLog("Can't Process message for web server: " + ex.ToString());
                            }
                        }
                        break;

                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }

            context.Response.OutputStream.Close();
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }
    }
}
