using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public SpeechRecognitionEngine recognitionEngine;
        public SpeechSynthesizer speechSynthesizer;

        private void speakText(string tts)
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
            List<string> greetings = new List<string>(new string[] { timeGreeting, "hello there", "Welcome", "Hello", "Welcome Back" });
            int r = random.Next(greetings.Count);
            speakText(greetings[r]);
        }

        private void sayPresense()
        {
            writeLog("Voice:  Speak presense acknowledgement");
            List<string> presense = new List<string>(new string[] { "I'm here", "Standing by", "Yes", "At your service", "Ready" });
            int r = random.Next(presense.Count);
            speakText(presense[r]);
        }

        private void sayAcknowledgement()
        {
            writeLog("Voice:  Speak acknowledgement");
            List<string> ack = new List<string>(new string[] { "Okay", "Doing that now", "One Moment" });
            int r = random.Next(ack.Count);
            speakText(ack[r]);
        }

        private GrammarBuilder commandGreeting ()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            Choices choicesHello = new Choices(new String[]
            {
            "hello " + Properties.Settings.Default.computerName,
            "ok " + Properties.Settings.Default.computerName,
            "hey " + Properties.Settings.Default.computerName
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

        private GrammarBuilder commandStartTheatre()
        {
            return buildCommand("Turn on Theatre", choicesStartTheatre);
        }

        private GrammarBuilder commandShutdownTheatre()
        {
            return buildCommand("Turn off Theatre", choicesShutdownTheatre);
        }

        private GrammarBuilder commandShowApplication()
        {
            return buildCommand("Show Application", choicesShowApplication);
        }

        private GrammarBuilder commandHideApplication()
        {
            return buildCommand("Hide Application", choicesHideApplication);
        }

        private GrammarBuilder commandPresense()
        {
            return buildCommand("Presense", choicesPresense);
        }

        private GrammarBuilder commandDimLights()
        {
            return buildCommand("Dim Lights", choicesDimLights);
        }

        private GrammarBuilder commandLightsOn()
        {
            return buildCommand("Lights On", choicesLightsOn);
        }

        private GrammarBuilder commandPausePlayback()
        {
            return buildCommand("Pause Playback", choicesPausePlayback);
        }

        private GrammarBuilder commandResumePlayback()
        {
            return buildCommand("Resume Playback", choicesResumePlayback);
        }

        private GrammarBuilder commandStopPlayback()
        {
            return buildCommand("Stop Playback", choicesStopPlayback);
        }

        private GrammarBuilder commandCancelPlayback()
        {
            return buildCommand("Cancel Playback", choicesCancelPlayback);
        }

        private Tuple<GrammarBuilder, GrammarBuilder> commandMovies()
        {
            GrammarBuilder grammarBuilderPlay = new GrammarBuilder();
            GrammarBuilder grammarBuilderCheck = new GrammarBuilder();
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
            
            grammarBuilderPlay.Append(choicesComputerName());
            grammarBuilderPlay.Append(choicesPolite);
            grammarBuilderPlay.Append(new SemanticResultKey("Play Movie", choicesPlayMovie));
            grammarBuilderPlay.Append(movies);
            grammarBuilderPlay.Append(choicesPolite);

            grammarBuilderCheck.Append(choicesComputerName());
            grammarBuilderCheck.Append(choicesPolite);
            grammarBuilderCheck.Append(new SemanticResultKey("Check Movie", choicesCheckMovie));
            grammarBuilderCheck.Append(movies);
            grammarBuilderCheck.Append(choicesPolite);
            return Tuple.Create(grammarBuilderPlay, grammarBuilderCheck);
        }

        private void loadVoiceCommands()
        {
            try
            {
                writeLog("Voice:  Loading base grammars");
                Choices commands = new Choices(new GrammarBuilder[]
                {
                    commandGreeting(),
                    commandStartTheatre(),
                    commandShutdownTheatre(),
                    commandShowApplication(),
                    commandHideApplication(),
                    commandPresense(),
                    commandDimLights(),
                    commandLightsOn(),
                    commandPausePlayback(),
                    commandResumePlayback(),
                    commandStopPlayback(),
                    commandCancelPlayback()
                });

                if (!kodiLoadingMovies && kodiMovies.Count > 0)
                {
                    Tuple<GrammarBuilder, GrammarBuilder> commandsTuple = commandMovies();
                    commands.Add(commandsTuple.Item1);
                    commands.Add(commandsTuple.Item2);
                }

                Grammar grammar = new Grammar(commands);

                grammar.Name = "commands";
                recognitionEngine.LoadGrammarAsync(grammar);
                toolStripStatus.Text = "Speech recognition grammars loaded";
                writeLog("Voice:  Grammar entries loaded");
            }
            catch (Exception ex)
            {
                writeLog("Voice:  Error loading grammar - " + ex.ToString());
                Application.Exit();
            }
        }

        private int expandCommands(ref Choices commandChoice, string spoken, string command, bool bePolite = false, bool useName = false)
        {
            bool debugVoice = false;
            string phrase = "";
            SemanticResultValue commandSemantic = new SemanticResultValue(spoken, command);
            if (debugVoice) writeLog("Voice Grammar:  '" + spoken + "'");
            commandChoice.Add(new GrammarBuilder(commandSemantic));
            int counter = 1;

            if (bePolite)
            {
                phrase = "please " + spoken;
                commandSemantic = new SemanticResultValue(phrase, command);
                if (debugVoice) writeLog("Voice Grammar:  '" + phrase + "'");
                commandChoice.Add(new GrammarBuilder(commandSemantic));

                phrase = spoken + " please";
                commandSemantic = new SemanticResultValue(phrase, command);
                if (debugVoice) writeLog("Voice Grammar:  '" + phrase + "'");
                commandChoice.Add(new GrammarBuilder(commandSemantic));
                counter += 2;
            }
            if (useName)
            {
                phrase = Properties.Settings.Default.computerName + " " + spoken;
                commandSemantic = new SemanticResultValue(phrase, command);
                if (debugVoice) writeLog("Voice Grammar:  '" + phrase + "'");
                commandChoice.Add(new GrammarBuilder(commandSemantic));
                counter += 1;
            }
            if (useName && bePolite)
            {
                phrase = Properties.Settings.Default.computerName + " please " + spoken;
                commandSemantic = new SemanticResultValue(phrase, command);
                if (debugVoice) writeLog("Voice Grammar:  '" + phrase + "'");
                commandChoice.Add(new GrammarBuilder(commandSemantic));

                phrase = Properties.Settings.Default.computerName + " " + spoken + " please";
                commandSemantic = new SemanticResultValue(phrase, command);
                if (debugVoice) writeLog("Voice Grammar:  '" + phrase + "'");
                commandChoice.Add(new GrammarBuilder(commandSemantic));
                counter += 2;
            }
            return counter;
        }

        public void voiceStartTheatre()
        {
            writeLog("Voice:  Starting 'Watch Movie' Harmony Activity");
            harmonyStartActivityByName(Properties.Settings.Default.voiceActivity);
            timerStartLights.Enabled = true;
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
                }
                ));
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
                                List<string> foundMovie = new List<string>(new string[]
                                {
                                    "I found ",
                                    "We have "
                                });
                                int r = random.Next(foundMovie.Count);
                                speakText(foundMovie[r] + movieEntry.name);
                            }
                        }
                    }
                    ));
                }
                else if (e.Result.Semantics.ContainsKey("Play Movie") && labelKodiPlaybackStatus.Text != "Playing")
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        string kodiMovieFile = null;
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

                        foreach (KeyValuePair<string, SemanticValue> items in e.Result.Semantics)
                        {
                            if (items.Key != "Play Movie")
                            {
                                kodiMovieFile = items.Key;
                            }
                        }
                        kodiPlayNext = null;
                        foreach (MovieEntry movieEntry in kodiMovies)
                        {
                            if (movieEntry.file == kodiMovieFile)
                            {
                                kodiPlayNext = movieEntry;
                            }
                        }

                        if (kodiPlayNext != null)
                        {
                            List<string> startMovie = new List<string>(new string[]
                            {
                                    "Starting Movie ",
                                    "Queuing up "
                            });
                            int r = random.Next(startMovie.Count);

                            formMain.speakText(startMovie[r] + kodiPlayNext.name);
                            formMain.timerKodiStartPlayback.Enabled = false;
                            formMain.timerKodiStartPlayback.Enabled = true;
                        }
                        else
                        {
                            formMain.writeLog("Voice:  Unable to start movie playback for '" + kodiMovieFile + "'");
                        }
                    }
                    ));
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
                        }
                        ));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Turn off Theatre"))
                {
                    if (labelKodiPlaybackStatus.Text != "Playing" && harmonyIsActivityStarted())
                    {
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.sayAcknowledgement();
                            formMain.labelLastVoiceCommand.Text = "Turn off theatre";
                            formMain.harmonyStartActivityByName("PowerOff");
                            formMain.writeLog("Voice:  Processed 'Turn off theatre'");
                        }
                        ));
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
                        }
                        ));
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
                        }
                        ));
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
                        }
                        ));
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
                        }
                        ));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Pause Playback"))
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.labelLastVoiceCommand.Text = "Pause playback";
                        formMain.kodiPlaybackControl("Pause");
                        formMain.writeLog("Voice:  Processed 'Pause playback'");
                    }
                    ));
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
                        }
                        ));
                    }
                }
                else if (e.Result.Semantics.ContainsKey("Stop Playback"))
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.labelLastVoiceCommand.Text = "Stop Playback";
                        formMain.kodiPlaybackControl("Stop");
                        formMain.writeLog("Voice:  Processed 'Stop playback'");
                    }
                    ));
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
                                List<string> cancel = new List<string>(new string[] { "Cancelling Playback", "Playback Aborted", "Ok" });
                                int r = random.Next(cancel.Count);
                                speakText(cancel[r]);

                            }
                            formMain.labelLastVoiceCommand.Text = "Cancel playback";
                            formMain.writeLog("Voice:  Processed 'Cancel playback'");
                        }
                        ));
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
                        }
                        ));
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
                        }
                        ));
                    }
                }
            }
            else
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Voice:  (Not Processed) Recognized Speech '" + e.Result.Text + "' Confidence " + confidence + "%");
                    formMain.toolStripStatus.Text = "Heard:  (Not Processed) '" + e.Result.Text + "' (" + confidence + "%)";
                }
                ));
            }
        }
    }
}