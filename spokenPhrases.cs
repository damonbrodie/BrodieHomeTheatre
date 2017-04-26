using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        List<string> ttsWarningPhrases = new List<string>(new string[] 
        {
            "Turning off lights in one minutes",
            "Shutting down lights in a minute"
        });
        List<string> ttsGreetingPhrases = new List<string>(new string[] 
        {
            "hello there",
            "Welcome",
            "Hello",
            "Welcome back"
        });
        List<string> ttsPresensePhrases = new List<string>(new string[] 
        {
            "I'm here",
            "Standing by",
            "Yes",
            "At your service",
            "Ready"
        });
        List<string> ttsAcknowledgementPhrases = new List<string>(new string[] {
            "Okay",
            "Doing that now",
            "One moment"
        });
        List<string> ttsFoundMoviePhrases = new List<string>(new string[]
        {
            "I found",
            "We have"
        });
        List<string> ttsStartMoviePhrases = new List<string>(new string[]
        {
            "Starting movie",
            "Queuing up"
        });
        List<string> ttsCancelPlaybackPhrases = new List<string>(new string[] 
        {
            "Cancelling playback",
            "Playback aborted",
            "Ok cancelling that"
        });
        List<string> ttsUnableToStartPhrases = new List<string>(new string[] 
        {
            "Unable to start that movie"
        });
    }
}
