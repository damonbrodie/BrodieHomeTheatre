using System.Collections.Generic;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        List<string> ttsWarningPhrases = new List<string>(new string[] 
        {
            "Turning off lights in one minutes",
            "Shutting down lights in a minute",
            "One minute until shutdown"
        });
        List<string> ttsGreetingPhrases = new List<string>(new string[] 
        {
            "Hello there",
            "Welcome",
            "Hello",
            "Welcome back"
        });
        List<string> ttsDolbyDemoPhrases = new List<string>(new string[]
        {
            "Queueing up the dolby demo",
            "Getting the dolby demo ready",
            "Starting up the dolby demo"
        });
        List<string> ttsTHXDemoPhrases = new List<string>(new string[]
        {
            "Queueing up the T H X demo",
            "Getting the T H X demo ready",
            "Starting up the T H X demo"
        });
        List<string> ttsVoiceControlsEnabled = new List<string>(new string[]
        {
            "Voice controls enabled",
            "Voice playback controls enabled",
        });
        List<string> ttsVoiceControlsDisabled = new List<string>(new string[]
        {
            "Voice controls disabled",
            "Voice playback controls disabled",
            "Disabling voice controls"
        });
        List<string> ttsPresensePhrases = new List<string>(new string[] 
        {
            "I'm here",
            "Standing by",
            "Yes",
            "At your service",
            "Ready",
            "I'm ready",
            "How can I help"
        });
        List<string> ttsAcknowledgementPhrases = new List<string>(new string[] {
            "Okay",
            "Doing that now",
            "One moment",
            "Give me a moment",
        });
        List<string> ttsFoundMoviePhrases = new List<string>(new string[]
        {
            "I found",
            "We have",
            "I found the movie",
            "Okay, I found the movie",
            "Okay, I found"
        });
        List<string> ttsStartMoviePhrases = new List<string>(new string[]
        {
            "Starting movie",
            "Queuing up",
            "Playing the movie"
        });
        List<string> ttsResumeMoviePhrases = new List<string>(new string[]
        {
            "Resume movie",
            "Resuming the movie"
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
