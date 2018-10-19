using System;
using System.Windows.Forms;
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
            var mediaChannel = castSender.GetChannel<IMediaChannel>();
            await castSender.LaunchAsync(mediaChannel);
            string url = "http://" + localIP +  ":" + Properties.Settings.Default.webServerPort + "/" + speechAudioFile;
            Logging.writeLog("Serving announcement '" + text + "' at: " + url);

            try
            {
                var mediaStatus = await mediaChannel.LoadAsync(new MediaInformation() { ContentId = url });
            }
            catch (Exception ex)
            {
                Logging.writeLog("Google Cast:  Timeout casting: " + ex.ToString());
            }
        }
    }
}
