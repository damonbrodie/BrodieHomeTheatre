using System.Collections.Generic;
using System.Windows.Forms;

namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        List<string> warning = new List<string>(new string[] 
        {
            "Turning off lights in one minutes",
            "Shutting down lights in a minute"
        });
        List<string> greetings = new List<string>(new string[] 
        {
            "hello there",
            "Welcome",
            "Hello",
            "Welcome Back"
        });
        List<string> presense = new List<string>(new string[] 
        {
            "I'm here",
            "Standing by",
            "Yes",
            "At your service",
            "Ready"
        });
        List<string> ack = new List<string>(new string[] {
            "Okay",
            "Doing that now",
            "One Moment"
        });
        List<string> foundMovie = new List<string>(new string[]
        {
            "I found ",
            "We have "
        });
        List<string> startMovie = new List<string>(new string[]
        {
            "Starting Movie ",
            "Queuing up "
        });
        List<string> cancel = new List<string>(new string[] 
        {
            "Cancelling Playback",
            "Playback Aborted",
            "Ok"
        });
        List<string> unableToStart = new List<string>(new string[] 
        {
            "Unable to start that movie"
        });
    }
}
