using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using GoogleCast;
using GoogleCast.Channels;
using GoogleCast.Models.Media;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private async void announce(string text)
        {
            string speechAudioFile = text_to_mp3(text, googleCloudChannel);
            var castSender = new Sender();

            await castSender.ConnectAsync(googleHomeReceiver);
            var mediaChannel = castSSender.GetChannel<IMediaChannel>();
            await tempSender.LaunchAsync(mediaChannel);
            string url = "http://" + localIP +  ":" + Properties.Settings.Default.webServerPort + "/" + speechAudioFile;
            Logging.writeLog("Serving announcement '" + text + "' at: " + url);

            try
            {
                var mediaStatus = await mediaChannel.LoadAsync(new MediaInformation() { ContentId = url });
            }
            catch (Exception ex)
            {
                Logging.writelog("Google Cast:  Timeout casting");
            }
        }
    }
}
