using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;
using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundOut;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public SpeechRecognitionEngine recognitionEngine;
        public SpeechSynthesizer speechSynthesizer;
        public bool voicePlaybackControlDisabled = true;

        private void setVoice()
        {
            if (Properties.Settings.Default.speechVoice != String.Empty)
            {
                foreach (InstalledVoice voice in formMain.speechSynthesizer.GetInstalledVoices())
                {
                    VoiceInfo info = voice.VoiceInfo;
                    if (info.Id == Properties.Settings.Default.speechVoice)
                    {
                        speechSynthesizer.SelectVoice(info.Name);
                        writeLog("Voice:  Select Speech Voice '" + info.Name + "'");
                    }
                }
            }
        }

        private void speakText(string tts)
        {
            if (Properties.Settings.Default.speechDevice == "Default Audio Device")
            {
                try
                {
                    if (speechSynthesizer != null)
                    {
                        speechSynthesizer.SetOutputToDefaultAudioDevice();
                    }
                }
                catch
                {
                    writeLog("Voice:  Unable to attach to default audio output device");
                }
                try
                {
                    speechSynthesizer.SpeakAsync(tts);
                }
                catch
                {
                    writeLog("Voice:  Unable to perform to TTS");
                }
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    int deviceID = -1;
                    foreach (var device in WaveOutDevice.EnumerateDevices())
                    {
                        if (device.Name == Properties.Settings.Default.speechDevice)
                        {
                            deviceID = device.DeviceId;
                        }

                    }
                    speechSynthesizer.SetOutputToWaveStream(stream);
                    speechSynthesizer.Speak(tts);

                    using (var waveOut = new WaveOut { Device = new WaveOutDevice(deviceID) })
                    using (var waveSource = new MediaFoundationDecoder(stream))
                    {
                        waveOut.Initialize(waveSource);
                        waveOut.Play();
                        waveOut.WaitForStopped();
                    }
                }
            }
        }

        private void sayGreeting()
        {
            int currHour = DateTime.Now.Hour;
            string timeGreeting;
            if (currHour >= 5 && currHour <= 11)
            {
                timeGreeting = "good morning";
            }
            else if (currHour >= 12 && currHour <= 17)
            {
                timeGreeting = "good afternoon";
            }
            else
            {
                timeGreeting = "good evening";
            }
            writeLog("Voice:  Speak greeting");
            List<string> allGreetings = ttsGreetingPhrases;
            allGreetings.Add(timeGreeting);
            int r = random.Next(allGreetings.Count);
            speakText(allGreetings[r]);
        }

        private void sayPresense()
        {
            writeLog("Voice:  Speak presense acknowledgement");
            int r = random.Next(ttsPresensePhrases.Count);
            speakText(ttsPresensePhrases[r]);
        }

        private void sayAcknowledgement()
        {
            writeLog("Voice:  Speak acknowledgement");
            int r = random.Next(ttsAcknowledgementPhrases.Count);
            speakText(ttsAcknowledgementPhrases[r]);
        }

        private GrammarBuilder commandGreeting ()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            Choices choicesHello = new Choices(new String[]
            {
                "hello " + Properties.Settings.Default.computerName,
                "ok "    + Properties.Settings.Default.computerName,
                "hey "   + Properties.Settings.Default.computerName,
                "hi "    + Properties.Settings.Default.computerName
            });

            grammarBuilder.Append(new SemanticResultKey("Greeting", choicesHello));
            return grammarBuilder;
        }

        private Choices choicesComputerName ()
        {
            Choices choice = new Choices(new String[]
            {
                Properties.Settings.Default.computerName,
                " "
            });

            return choice;
        }

        private GrammarBuilder buildCommand(string command, Choices choicesCommand, bool useName=true, bool bePolite=true)
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();

            if (useName)
            {
                grammarBuilder.Append(choicesComputerName());
            }
            if (bePolite)
            {
                grammarBuilder.Append(choicesPolite);
            }
            grammarBuilder.Append(new SemanticResultKey(command, choicesCommand));
            if (bePolite)
            {
                grammarBuilder.Append(choicesPolite);
            }
            return grammarBuilder;
        }

        private Tuple<GrammarBuilder, GrammarBuilder, GrammarBuilder> commandMovies()
        {
            Choices movies = new Choices();
            foreach (MovieEntry movieEntry in kodiMovies)
            {
                movies.Add(new SemanticResultKey(movieEntry.file, movieEntry.cleanName));
            }
            foreach (PartialMovieEntry entry in formMain.moviesAfterColonNames)
            {
                if (!searchMovieList(formMain.moviesDuplicateNames, entry.name))
                {
                    movies.Add(new SemanticResultKey(entry.file, entry.name));
                }
            }
            foreach (PartialMovieEntry entry in formMain.moviesPartialNames)
            {
                if (!formMain.searchMovieList(formMain.moviesDuplicateNames, entry.name))
                {
                    movies.Add(new SemanticResultKey(entry.file, entry.name));
                }
            }
            GrammarBuilder grammarBuilderPlay = new GrammarBuilder();
            grammarBuilderPlay.Append(choicesComputerName());
            grammarBuilderPlay.Append(choicesPolite);
            grammarBuilderPlay.Append(new SemanticResultKey("Play Movie", choicesPlayMovie));
            grammarBuilderPlay.Append(movies);
            grammarBuilderPlay.Append(choicesPolite);

            GrammarBuilder grammarBuilderCheck = new GrammarBuilder();
            grammarBuilderCheck.Append(choicesComputerName());
            grammarBuilderCheck.Append(choicesPolite);
            grammarBuilderCheck.Append(new SemanticResultKey("Check Movie", choicesCheckMovie));
            grammarBuilderCheck.Append(movies);
            grammarBuilderCheck.Append(choicesPolite);

            GrammarBuilder grammarBuilderResume = new GrammarBuilder();
            grammarBuilderResume.Append(choicesComputerName());
            grammarBuilderResume.Append(choicesPolite);
            grammarBuilderResume.Append(new SemanticResultKey("Resume Movie", choicesResumeMovie));
            grammarBuilderResume.Append(movies);
            grammarBuilderResume.Append(choicesPolite);

            return Tuple.Create(grammarBuilderPlay, grammarBuilderCheck, grammarBuilderResume);
        }

        private void loadVoiceCommands()
        {
            try
            {
                writeLog("Voice:  Loading base grammars");
                Choices commands = new Choices(new GrammarBuilder[]
                {
                    commandGreeting(),
                    buildCommand("Turn on Theatre", choicesStartTheatre),
                    buildCommand("Show THX Demo", choicesShowTHXDemo),
                    buildCommand("Show Dolby Demo", choicesShowDolbyDemo),
                    buildCommand("Transparent Screen", choicesTransparentScreen),
                    buildCommand("Turn off Theatre", choicesShutdownTheatre),
                    buildCommand("Show Application", choicesShowApplication),
                    buildCommand("Hide Application", choicesHideApplication),
                    buildCommand("Presense", choicesPresense),
                    buildCommand("Dim Lights", choicesDimLights),
                    buildCommand("Lights On", choicesLightsOn),
                    buildCommand("Pause Playback", choicesPausePlayback),
                    buildCommand("Resume Playback", choicesResumePlayback),
                    buildCommand("Stop Playback", choicesStopPlayback),
                    buildCommand("Cancel Playback", choicesCancelPlayback),
                    buildCommand("Enable Voice Playback Control", choicesEnableVoicePlayback),
                    buildCommand("Disable Voice Playback Control", choicesDisableVoicePlayback)
                });

                if (kodiLoadingMedia)
                {
                    writeLog("Voice:  Skipping movie grammar - movie loading underway");
                }
                else if (kodiMovies.Count == 0)
                {
                    writeLog("Voice:  Skipping movie grammar - movie count is zero");
                }
                else
                {
                    Tuple<GrammarBuilder, GrammarBuilder, GrammarBuilder> commandsTuple = commandMovies();
                    commands.Add(commandsTuple.Item1);
                    commands.Add(commandsTuple.Item2);
                    commands.Add(commandsTuple.Item3);
                    writeLog("Voice:  Adding movie grammars");
                }
                
                Grammar grammar = new Grammar(commands);
                grammar.Name = "commands";
                try
                {
                    recognitionEngine.LoadGrammarAsync(grammar);
                }
                catch
                {
                    writeLog("Voice:  Unable to load grammars");
                    return;
                }
                toolStripStatus.Text = "Speech recognition grammars loaded";
                writeLog("Voice:  Grammar entries loaded");
            }
            catch (Exception ex)
            {
                writeLog("Voice:  Error loading grammar - " + ex.ToString());
                Application.Exit();
            }
        }

        public void voiceStartTheatre()
        {
            writeLog("Voice:  Starting 'Watch Movie' Harmony Activity");
            harmonyStartActivityByName(Properties.Settings.Default.voiceActivity);
            timerStartLights.Enabled = true;
        }

        public bool toggleVoicePlaybackControl()
        {
            if (voicePlaybackControlDisabled)
            {
                writeLog("Voice:  Toggling voice playback control to On");
                voicePlaybackControlDisabled = false;
                return false;
            }
            else
            {
                writeLog("Voice:  Toggling voice playback control to Off");
                voicePlaybackControlDisabled = true;
                return true;
            }
        }

        public bool enableVoicePlaybackControl()
        {
            if (voicePlaybackControlDisabled)
            {
                writeLog("Voice:  Enabling voice playback control");
                voicePlaybackControlDisabled = false;
                return true;
            }
            return false;
        }

        public bool disableVoicePlaybackControl()
        {
            if (! voicePlaybackControlDisabled)
            {
                writeLog("Voice:  Disabling voice playback control");
                voicePlaybackControlDisabled = true;
                return true;
            }
            return false;
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string confidence = Math.Round((e.Result.Confidence * 100), 2).ToString();
            float minConfidence = (float)Properties.Settings.Default.voiceConfidence / (float)10.0;

            if (e.Result.Alternates != null && e.Result.Confidence > minConfidence)
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Voice:  Recognized Speech '" + e.Result.Text + "' Confidence " + confidence + "%");
                    formMain.toolStripStatus.Text = "Heard:  '" + e.Result.Text + "' (" + confidence + "%)";
                }));
                RecognizedPhrase phrase = e.Result.Alternates[0];

                if (e.Result.Semantics.ContainsKey("Check Movie") && labelKodiPlaybackStatus.Text != "Playing")
                {
                    string kodiMovieFile = null;
                    formMain.BeginInvoke(new Action(() =>
                    {
                        foreach (KeyValuePair<string, SemanticValue> items in e.Result.Semantics)
                        {
                            if (items.Key != "Check Movie")
                            {
                                kodiMovieFile = items.Key;
                            }
                        }
                        foreach (MovieEntry movieEntry in kodiMovies)
                        {
                            if (movieEntry.file == kodiMovieFile)
                            {
                                formMain.writeLog("Voice: Checking for movie '" + movieEntry.name + "'");
                                int r = random.Next(ttsFoundMoviePhrases.Count);
                                speakText(ttsFoundMoviePhrases[r] + " " + movieEntry.name);
                            }
                        }
                    }));
                }
                else if ((e.Result.Semantics.ContainsKey("Play Movie") || e.Result.Semantics.ContainsKey("Resume Movie")) && labelKodiPlaybackStatus.Text != "Playing")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        string kodiMovieFile = null;
                        foreach (KeyValuePair<string, SemanticValue> items in e.Result.Semantics)
                        {
                            if (items.Key != "Play Movie")
                            {
                                kodiMovieFile = items.Key;
                            }
                        }
                        formMain.kodiPlayNext = new MovieEntry();
                        foreach (MovieEntry movieEntry in kodiMovies)
                        {
                            if (movieEntry.file == kodiMovieFile)
                            {
                                formMain.kodiPlayNext = movieEntry;
                            }
                        }

                        if (kodiPlayNext != null)
                        {
                            
                            if (e.Result.Semantics.ContainsKey("Resume Movie"))
                            {
                                int r = random.Next(ttsResumeMoviePhrases.Count);
                                formMain.speakText(ttsResumeMoviePhrases[r] + " " + kodiPlayNext.name);
                                kodiPlayNext.resume = true;
                            }
                            else
                            {
                                int r = random.Next(ttsStartMoviePhrases.Count);
                                formMain.speakText(ttsStartMoviePhrases[r] + " " + kodiPlayNext.name);
                                kodiPlayNext.resume = false;
                            }
                            formMain.timerKodiStartPlayback.Enabled = false;
                            formMain.timerKodiStartPlayback.Enabled = true;

                            if (!formMain.harmonyIsActivityStarted())
                            {
                                formMain.writeLog("Voice: Starting delay timer for movie to 30 seconds");
                                // Wait for the projector to warm up.
                                formMain.timerKodiStartPlayback.Interval = 30000;
                                formMain.voiceStartTheatre();
                            }
                            else
                            {
                                formMain.writeLog("Voice: Starting delay timer for movie to 5 seconds");
                                formMain.timerKodiStartPlayback.Interval = 5000;
                            }

                        }
                        else
                        {
                            int r = random.Next(ttsUnableToStartPhrases.Count);
                            formMain.speakText(ttsUnableToStartPhrases[r]);
                            formMain.writeLog("Voice:  Unable to start movie playback for '" + kodiMovieFile + "'");
                        }
                    }));
                }
                else if (e.Result.Semantics.ContainsKey("Turn on Theatre"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing" && !harmonyIsActivityStarted())
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.sayAcknowledgement();
                            formMain.labelLastVoiceCommand.Text = "Turn on theatre";
                            formMain.voiceStartTheatre();
                            formMain.writeLog("Voice:  Processed 'Turn on theatre'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Show THX Demo") && labelKodiPlaybackStatus.Text != "Playing")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiPlayNext = new MovieEntry();
                        formMain.kodiPlayNext.cleanName = "THX Demo";
                        formMain.kodiPlayNext.name = "THX Demo";
                        formMain.kodiPlayNext.file = mediaTHXDemo;
                        formMain.kodiPlayNext.resume = false;

                        int r = random.Next(ttsTHXDemoPhrases.Count);
                        formMain.speakText(ttsTHXDemoPhrases[r]);
                        formMain.timerKodiStartPlayback.Enabled = false;
                        formMain.timerKodiStartPlayback.Enabled = true;

                        if (!formMain.harmonyIsActivityStarted())
                        {
                            formMain.writeLog("Voice: Starting delay timer for THX Demo to 30 seconds");
                            // Wait for the projector to warm up.
                            formMain.timerKodiStartPlayback.Interval = 30000;
                            formMain.voiceStartTheatre();
                        }
                        else
                        {
                            formMain.writeLog("Voice: Starting delay timer for THX Demo to 5 seconds");
                            formMain.timerKodiStartPlayback.Interval = 5000;
                        }
                    }));
                }
                else if (e.Result.Semantics.ContainsKey("Show Dolby Demo") && labelKodiPlaybackStatus.Text != "Playing")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.kodiPlayNext = new MovieEntry();
                        formMain.kodiPlayNext.cleanName = "Dolby Demo";
                        formMain.kodiPlayNext.name = "Dolby Demo";
                        formMain.kodiPlayNext.file = mediaDolbyDemo;
                        formMain.kodiPlayNext.resume = false;

                        int r = random.Next(ttsDolbyDemoPhrases.Count);
                        formMain.speakText(ttsDolbyDemoPhrases[r]);
                        formMain.timerKodiStartPlayback.Enabled = false;
                        formMain.timerKodiStartPlayback.Enabled = true;

                        if (!formMain.harmonyIsActivityStarted())
                        {
                            formMain.writeLog("Voice: Starting delay timer for Dolby Demo to 30 seconds");
                            // Wait for the projector to warm up.
                            formMain.timerKodiStartPlayback.Interval = 30000;
                            formMain.voiceStartTheatre();
                        }
                        else
                        {
                            formMain.writeLog("Voice: Starting delay timer for Dolby Demo to 5 seconds");
                            formMain.timerKodiStartPlayback.Interval = 5000;
                        }
                    }));
                }
                else if (e.Result.Semantics.ContainsKey("Turn on Theatre"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing" && !harmonyIsActivityStarted())
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.sayAcknowledgement();
                            formMain.labelLastVoiceCommand.Text = "Turn on theatre";
                            formMain.voiceStartTheatre();
                            formMain.writeLog("Voice:  Processed 'Turn on theatre'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Transparent Screen"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing" && harmonyIsActivityStarted())
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Make screen transparent";
                            formMain.kodiShowBehindScreen();
                            formMain.writeLog("Voice:  Processed 'Make screen transparent'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Turn off Theatre"))
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.writeLog("Voice:  Debug - in turn off theatre, playback status is '" +
                            labelKodiPlaybackStatus.Text + "' harmony status is '" +
                            harmonyIsActivityStarted() + "'"
                            );
                    }));
                    if (labelKodiPlaybackStatus.Text != "Playing" && harmonyIsActivityStarted())
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.sayAcknowledgement();
                            formMain.labelLastVoiceCommand.Text = "Turn off theatre";
                            formMain.harmonyStartActivityByName("PowerOff");
                            formMain.writeLog("Voice:  Processed 'Turn off theatre'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Show Application"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (formMain.WindowState == FormWindowState.Minimized)
                            {
                                formMain.labelLastVoiceCommand.Text = "Show application";
                                formMain.WindowState = FormWindowState.Normal;
                                formMain.writeLog("Voice:  Processed 'Show application'");
                            }
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Hide Application"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (formMain.WindowState == FormWindowState.Normal)
                            {
                                formMain.labelLastVoiceCommand.Text = "Hide application";
                                formMain.WindowState = FormWindowState.Minimized;
                                formMain.writeLog("Voice:  Processed 'Hide application'");
                            }
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Greeting"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Greeting";
                            formMain.sayGreeting();
                            formMain.writeLog("Voice:  Processed 'Greeting'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Presense"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Presense";
                            formMain.sayPresense();
                            formMain.writeLog("Voice:  Processed 'Presense'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Pause Playback"))
                {
                    if (labelKodiPlaybackStatus.Text == "Playing")
                    {
                        if (voicePlaybackControlDisabled)
                        {
                            formMain.writeLog("Voice:  Voice playback controls disabled - Not processing 'Pause'");
                        }
                        else
                        {
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.labelLastVoiceCommand.Text = "Pause playback";
                                formMain.kodiPlaybackControl("Pause");
                                formMain.writeLog("Voice:  Processed 'Pause playback'");
                            }));
                        }
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Resume Playback"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Resume playback";
                            formMain.kodiPlaybackControl("Play");
                            formMain.writeLog("Voice:  Processed 'Resume playback'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Stop Playback"))
                {
                    if (labelKodiPlaybackStatus.Text == "Playing")
                    {
                        if (voicePlaybackControlDisabled)
                        {
                            formMain.writeLog("Voice:  Voice playback controls disabled - Not processing 'Stop'");
                        }
                        else
                        {
                            formMain.labelLastVoiceCommand.Text = "Stop Playback";
                            formMain.kodiPlaybackControl("Stop");
                            formMain.writeLog("Voice:  Processed 'Stop playback'");
                        }
                    }
                    else if (labelKodiPlaybackStatus.Text == "Paused")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Stop Playback";
                            formMain.kodiPlaybackControl("Stop");
                            formMain.writeLog("Voice:  Processed 'Stop playback'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Cancel Playback"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (kodiPlayNext != null)
                            {
                                formMain.kodiPlayNext = null;
                                int r = random.Next(ttsCancelPlaybackPhrases.Count);
                                speakText(ttsCancelPlaybackPhrases[r]);
                            }
                            formMain.labelLastVoiceCommand.Text = "Cancel playback";
                            formMain.writeLog("Voice:  Processed 'Cancel playback'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Dim Lights"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Dim lights";
                            formMain.lightsToStoppedLevel();
                            formMain.writeLog("Voice:  Processed 'Dim lights'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Lights On"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = "Lights on";
                            formMain.lightsToEnteringLevel();
                            formMain.writeLog("Voice:  Processed 'Lights on'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Enable Voice Playback Control"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            int r = random.Next(ttsVoiceControlsEnabled.Count);
                            formMain.speakText(ttsVoiceControlsEnabled[r]);
                            formMain.labelLastVoiceCommand.Text = "Enabling voice playback controls";
                            formMain.voicePlaybackControlDisabled = false;
                            formMain.writeLog("Voice:  Processed 'Enable voice playback controls'");
                        }));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Disable Voice Playback Control"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing")
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            int r = random.Next(ttsVoiceControlsDisabled.Count);
                            formMain.speakText(ttsVoiceControlsDisabled[r]);

                            formMain.labelLastVoiceCommand.Text = "Disabling voice playback controls";
                            formMain.voicePlaybackControlDisabled = true;
                            formMain.writeLog("Voice:  Processed 'Disable voice playback controls'");
                        }));
                    }
                }
            }
            else
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Voice:  (Not Processed) Recognized Speech '" + e.Result.Text + "' Confidence " + confidence + "%");
                    formMain.toolStripStatus.Text = "Heard:  (Not Processed) '" + e.Result.Text + "' (" + confidence + "%)";
                }));
            }
        }
    }
}