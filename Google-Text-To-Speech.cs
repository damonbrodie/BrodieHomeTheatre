using System.IO;
using System.Windows.Forms;
using Google.Cloud.TextToSpeech.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;

namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public void text_to_mp3(string text)
        {
            GoogleCredential credential;

            credential = GoogleCredential.FromFile(@"C:\\Users\damon\Documents\brodie-theatre-google-key.json");

            var channel = new Grpc.Core.Channel(TextToSpeechClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());

            TextToSpeechClient client = TextToSpeechClient.Create(channel);

            var input = new SynthesisInput
            {
                Text = text

            };

            var voiceSelection = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                SsmlGender = SsmlVoiceGender.Female

            };
            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            var response = client.SynthesizeSpeech(input, voiceSelection, audioConfig);

            using (var output = File.Create("output.mp3"))
            {
                response.AudioContent.WriteTo(output);
            }

        }
    }

}
