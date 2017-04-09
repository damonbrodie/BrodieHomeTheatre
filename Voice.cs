using System;
using System.IO;
using System.Media;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Speech.Synthesis;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public SpeechRecognitionEngine recognitionEngine;
        public SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public string startupWave = "Powering up.wav";
        public string ackWave = "Ack sound.wav";
        public string wavePath = @"c:\Users\damon\Documents\Shared\Wavs";

        private void playSound(string soundFile)
        {
            string fullPath = Path.Combine(wavePath, soundFile);
            if (File.Exists(fullPath))
            {
                SoundPlayer simpleSound = new SoundPlayer(fullPath);
                simpleSound.Play();
            }
        }

        private void speakText(string tts)
        {
            speechSynthesizer.SpeakAsync(tts);
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
            writeLog("Voice:  Saying greeting");
            List<string> greetings = new List<string>(new string[] { timeGreeting, "hello there", "Welcome", "Hello", "Welcome Back" });
            int r = random.Next(greetings.Count);
            speakText(greetings[r]);
        }

        private void sayPresense()
        {
            writeLog("Voice:  Saying acknowledgement");
            List<string> presense = new List<string>(new string[] { "I'm here", "Standing by", "Yes", "I am around"});
            int r = random.Next(presense.Count);
            speakText(presense[r]);
        }

        private static void loadVoiceCommands()
        {
            GrammarBuilder gb = new GrammarBuilder();
            Choices commandChoice = new Choices();

            int grammarCount = 0;
            formMain.writeLog("Voice:  Loading base grammars");
            grammarCount += formMain.expandCommands(ref commandChoice, "turn on projector", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "projector on", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "home theater on", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "let's watch a movie", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "start theater", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "power on projector", "Turn on Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "power on theater", "Turn on Theatre", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "turn off projector", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "projector off", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "home theater off", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "shutdown theater", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "turn off theater", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "power down theater", "Turn off Theatre", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "power off theater", "Turn off Theatre", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "show yourself", "Show Application", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "show application", "Show Application", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "hide yourself", "Hide Application", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "hide application", "Hide Application", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "are you there", "Presense", false, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "are you listening", "Presense", false, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "Hello " + Properties.Settings.Default.computerName, "Greeting", false, false);
            grammarCount += formMain.expandCommands(ref commandChoice, "OK " + Properties.Settings.Default.computerName, "Greeting", false, false);

            grammarCount += formMain.expandCommands(ref commandChoice, "dim the lights", "Dim Lights", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "turn down lights", "Dim Lights", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "turn down the lights", "Dim Lights", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "turn on lights", "Lights On", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "turn on the lights", "Lights On", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "raise the lights", "Lights On", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "pause playback", "Pause Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "pause movie", "Pause Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "pause the movie", "Pause Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "pause the playback", "Pause Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "pause the movie playback", "Pause Playback", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "resume playback", "Resume Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "continue playing", "Resume Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "unpause playback", "Resume Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "unpause movie", "Resume Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "unpause movie playback", "Resume Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "unpause the movie playback", "Resume Playback", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "stop playback", "Stop Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "stop playback", "Stop Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "stop movie", "Stop Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "stop movie playback", "Stop Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "stop the movie playback", "Stop Playback", true, true);

            grammarCount += formMain.expandCommands(ref commandChoice, "don't play that", "Cancel Playback", true, true);
            grammarCount += formMain.expandCommands(ref commandChoice, "cancel playback", "Cancel Playback", true, true);
            if (!formMain.kodiLoadingMovies)
            {
                foreach (MovieEntry movieEntry in formMain.kodiMovies)
                {
                    grammarCount += formMain.expandCommands(ref commandChoice, "play movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);
                    //grammarCount += formMain.expandCommands(ref commandChoice, "play the movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);

                    //grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);
                    //grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch the movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);

                    //grammarCount += formMain.expandCommands(ref commandChoice, "watch movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);
                    //grammarCount += formMain.expandCommands(ref commandChoice, "watch the movie - " + movieEntry.cleanName, "play movie|" + movieEntry.file, true, true);
                }
                foreach (PartialMovieEntry entry in formMain.moviesAfterColonNames)
                {
                    if (!formMain.searchMovieList(formMain.moviesDuplicateNames, entry.name))
                    {
                        grammarCount += formMain.expandCommands(ref commandChoice, "play movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "play the movie - " + entry.name, "play movie|" + entry.file, true, true);

                       // grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch the movie - " + entry.name, "play movie|" + entry.file, true, true);

                        //grammarCount += formMain.expandCommands(ref commandChoice, "watch movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "watch the movie - " + entry.name, "play movie|" + entry.file, true, true);
                    }
                }
                foreach (PartialMovieEntry entry in formMain.moviesPartialNames)
                {
                    if (!formMain.searchMovieList(formMain.moviesDuplicateNames, entry.name))
                    {
                        //grammarCount += formMain.expandCommands(ref commandChoice, "play movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "play the movie - " + entry.name, "play movie|" + entry.file, true, true);

                        //grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "Let's watch the movie - " + entry.name, "play movie|" + entry.file, true, true);

                        //grammarCount += formMain.expandCommands(ref commandChoice, "watch movie - " + entry.name, "play movie|" + entry.file, true, true);
                        //grammarCount += formMain.expandCommands(ref commandChoice, "watch the movie - " + entry.name, "play movie|" + entry.file, true, true);
                    }
                }
                formMain.writeLog("Voice:  " + grammarCount.ToString() + " grammar entries loaded");
            }
            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            formMain.recognitionEngine.LoadGrammar(grammar);
            formMain.toolStripStatus.Text = "Speech recognition grammars loaded";
        }

        private int expandCommands(ref Choices commandChoice, string spoken, string command, bool bePolite=false, bool useName=false)
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
            harmonyStartActivityByName(Properties.Settings.Default.voiceActivity);
            timerStartLights.Enabled = true;
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string confidence = Math.Round((e.Result.Confidence * 100), 2).ToString();
            float minConfidence = (float)Properties.Settings.Default.voiceConfidence / (float)10.0;

            if (e.Result.Alternates != null && e.Result.Confidence > minConfidence && labelKodiPlaybackStatus.Text != "Playing")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Voice:  Recognized Speech '" + e.Result.Text + "' Confidence " + confidence+"%");
                    formMain.toolStripStatus.Text = "Heard:  '" + e.Result.Text + "' (" + confidence + "%)";
                }
                ));
                RecognizedPhrase phrase = e.Result.Alternates[0];
                
                string topPhrase = phrase.Semantics.Value.ToString();
                if (topPhrase.StartsWith("play movie|"))
                {
                    formMain.BeginInvoke(new Action(() =>
                    {
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
                    ));
                    string kodiMovieFile = topPhrase.Split('|')[1];
                    kodiPlayNext = null;
                    foreach (MovieEntry movieEntry in kodiMovies)
                    {
                        if (movieEntry.file == kodiMovieFile)
                        {
                            kodiPlayNext = movieEntry;
                        }
                    }
                    speakText("Starting movie: " + kodiPlayNext.name);
                    formMain.BeginInvoke(new Action(() =>
                    {
                        formMain.timerKodiStartPlayback.Enabled = false;
                        formMain.timerKodiStartPlayback.Enabled = true;
                    }
                    ));
                }
                else
                {
                    switch (topPhrase)
                    {
                        case "Turn on Theatre":

                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.voiceStartTheatre();
                                formMain.writeLog("Voice:  Processed " + topPhrase);
                            }
                            ));
                            break;
                        case "Turn off Theatre":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.harmonyStartActivityByName("PowerOff");
                                formMain.lightsToEnteringLevel();
                                formMain.writeLog("Voice:  Processed " + topPhrase);
                            }
                            ));
                            break;
                        case "Show Application":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                if (formMain.WindowState == FormWindowState.Minimized)
                                {
                                    formMain.playSound(ackWave);
                                    formMain.labelLastVoiceCommand.Text = topPhrase;
                                    formMain.WindowState = FormWindowState.Normal;
                                    formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                                }
                            }
                            ));
                            break;
                        case "Hide Application":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                if (formMain.WindowState == FormWindowState.Normal)
                                {
                                    formMain.playSound(ackWave);
                                    formMain.labelLastVoiceCommand.Text = topPhrase;
                                    formMain.WindowState = FormWindowState.Minimized;
                                    formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                                }
                            }
                            ));
                            break;
                        case "Greeting":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.sayGreeting();
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Presense":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.sayPresense();
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Lights On":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.lightsToEnteringLevel();
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Pause Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Pause");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Resume Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Play");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Stop Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Stop");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Cancel Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                if (kodiPlayNext != null)
                                {
                                    formMain.kodiPlayNext = null;
                                    List<string> cancel = new List<string>(new string[] { "Cancelling Playback", "Playback Aborted", "Ok"});
                                    int r = random.Next(cancel.Count);
                                    speakText(cancel[r]);

                                }
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Dim Lights":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.playSound(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.lightsToStoppedLevel();
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
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