﻿using System;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;

namespace BrodieTheatre
{
    public partial class FormMain : Form
    {

        public Choices choicesPolite = new Choices(new string[]
        {
            "Please",
            " " // This makes the please optional
        });

        public Choices choicesDimLights = new Choices(new String[]
        {
            "dim the lights",
            "dim lights",
            "turn down the lights",
            "turn down lights"
        });

        public Choices choicesLightsOn = new Choices(new String[]
        {
            "raise lights",
            "raise the ligths",
            "brighten lights",
            "brighten the lights",
            "turn on the lights",
            "turn on lights"
        });

        public Choices choicesCheckMovie = new Choices(new String[]
        {
            "do we have the movie",
            "do we have movie",
            "check for the movie",
            "check for movie",
        });

        public Choices choicesStartMovie = new Choices(new String[]
        {
            "play the movie",
            "play movie",
            "start the movie",
            "start movie",
            "let's watch movie",
            "let's watch the movie"
        });

        public Choices choicesStartTheatre = new Choices(new String[]
        {
            "turn on theatre",
            "turn on the theatre",
            "turn on projector",
            "turn on the projector",
            "power on theatre",
            "power on the theatre"
        });

        public Choices choicesShutdownTheatre = new Choices(new String[]
        {
            "turn off theatre",
            "turn off the theatre",
            "turn off projector",
            "turn off the projector",
            "power off theatre",
            "power off the theatre",
            "shut down theatre",
            "shut down the theatre",
            "shut down projector",
            "shut down the projector",
        });

        public Choices choicesShowApplication = new Choices(new String[]
        {
            "show application",
            "show yourself"
        });

        public Choices choicesHideApplication = new Choices(new String[]
        {
            "hide application",
            "hide yourself"
        });

        public Choices choicesPresense = new Choices(new String[]
        {
            "are you there",
            "are you listening",
        });

        public Choices choicesPausePlayback = new Choices(new String[]
        {
            "pause playback",
            "pause the playback",
            "pause movie",
            "pause the movie",
            "pause movie playback"
        });

        public Choices choicesStopPlayback = new Choices(new String[]
        {
            "stop playback",
            "stop the playback",
            "stop movie",
            "stop the movie",
            "stop movie playback"
        });

        public Choices choicesResumePlayback = new Choices(new String[]
        {
            "resume playback",
            "resume the playback",
            "resume movie",
            "resume the movie",
            "resume movie playback",
            "play the movie",
            "continue playback",
            "unpause movie",
            "unpause playback"
        });

        public Choices choicesCancelPlayback = new Choices(new String[]
        {
            "cancel that",
            "don't play that",
            "cancel playback"
        });

        public Choices choicesPlayMovie = new Choices(new String[]
        {
            "watch movie",
            "watch the movie",
            "play movie",
            "play the movie",
        });
    }
}