using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        public SpeechRecognitionEngine recognitionEngine;

        public List<string> greetingsEvening = new List<string>();
        public List<string> greetingsMorning = new List<string>();
        public List<string> greetingsAfternoon = new List<string>();
        public List<string> greetingsPresense = new List<string>();

        public string wavePath = @"c:\Users\damon\Documents\Shared\Wavs";
        public string startupWave = @"c:\Users\damon\Documents\Shared\Wavs\Powering up.wav";
        public string ackWave = @"c:\Users\damon\Documents\Shared\Wavs\Ack sound.wav";
        private void loadVoiceCommands()
        {
            GrammarBuilder gb = new GrammarBuilder();
            Choices commandChoice = new Choices();

            expandCommands(ref commandChoice, "turn on projector", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "projector on", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "home theater on", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "let's watch a movie", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "start theater", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "power on projector", "Turn on Theatre", true, true);
            expandCommands(ref commandChoice, "power on theater", "Turn on Theatre", true, true);

            expandCommands(ref commandChoice, "turn off projector", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "projector off", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "home theater off", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "shutdown theater", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "turn off theater", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "power down theater", "Turn off Theatre", true, true);
            expandCommands(ref commandChoice, "power off theater", "Turn off Theatre", true, true);

            expandCommands(ref commandChoice, "show yourself", "Show Application", true, true);
            expandCommands(ref commandChoice, "show application", "Show Application", true, true);

            expandCommands(ref commandChoice, "hide yourself", "Hide Application", true, true);
            expandCommands(ref commandChoice, "hide application", "Hide Application", true, true);

            expandCommands(ref commandChoice, "are you there", "Presense", false, true);
            expandCommands(ref commandChoice, "are you listening", "Presense", false, true);

            expandCommands(ref commandChoice, "Hello " + Properties.Settings.Default.computerName, "Greeting", false, false);
            expandCommands(ref commandChoice, "OK " + Properties.Settings.Default.computerName, "Greeting", false, false);

            expandCommands(ref commandChoice, "dim the lights", "Dim Lights", true, true);
            expandCommands(ref commandChoice, "turn down lights", "Dim Lights", true, true);
            expandCommands(ref commandChoice, "turn down the lights", "Dim Lights", true, true);

            expandCommands(ref commandChoice, "turn on lights", "Lights On", true, true);
            expandCommands(ref commandChoice, "turn on the lights", "Lights On", true, true);
            expandCommands(ref commandChoice, "raise the lights", "Lights On", true, true);

            expandCommands(ref commandChoice, "pause playback", "Pause Playback", true, true);
            expandCommands(ref commandChoice, "pause movie", "Pause Playback", true, true);
            expandCommands(ref commandChoice, "pause the movie", "Pause Playback", true, true);
            expandCommands(ref commandChoice, "pause the playback", "Pause Playback", true, true);
            expandCommands(ref commandChoice, "pause the movie playback", "Pause Playback", true, true);

            expandCommands(ref commandChoice, "resume playback", "Resume Playback", true, true);
            expandCommands(ref commandChoice, "continue playing", "Resume Playback", true, true);
            expandCommands(ref commandChoice, "unpause playback", "Resume Playback", true, true);
            expandCommands(ref commandChoice, "unpause movie", "Resume Playback", true, true);
            expandCommands(ref commandChoice, "unpause movie playback", "Resume Playback", true, true);
            expandCommands(ref commandChoice, "unpause the movie playback", "Resume Playback", true, true);

            expandCommands(ref commandChoice, "stop playback", "Stop Playback", true, true);
            expandCommands(ref commandChoice, "stop playback", "Stop Playback", true, true);
            expandCommands(ref commandChoice, "stop movie", "Stop Playback", true, true);
            expandCommands(ref commandChoice, "stop movie playback", "Stop Playback", true, true);
            expandCommands(ref commandChoice, "stop the movie playback", "Stop Playback", true, true);

            foreach (MovieEntry movieEntry in kodiMovies)
            {
                expandCommands(ref commandChoice, "play movie|" + movieEntry.name, "play movie " + movieEntry.file, false, false);
            }

            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            recognitionEngine.LoadGrammar(grammar);

            loadWaves();
        }

        private void expandCommands(ref Choices commandChoice, string spoken, string command, bool bePolite=false, bool useName=false)
        {
            SemanticResultValue commandSemantic = new SemanticResultValue(spoken, command);
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            if (bePolite)
            {
                commandSemantic = new SemanticResultValue("please " + spoken, command);
                commandChoice.Add(new GrammarBuilder(commandSemantic));

                commandSemantic = new SemanticResultValue(spoken + " please", command);
                commandChoice.Add(new GrammarBuilder(commandSemantic));
            }
            if (useName)
            {
                commandSemantic = new SemanticResultValue(Properties.Settings.Default.computerName + " " + spoken, command);
                commandChoice.Add(new GrammarBuilder(commandSemantic));
            }
            if (useName && bePolite)
            {
                commandSemantic = new SemanticResultValue(Properties.Settings.Default.computerName + " please "  + spoken, command);
                commandChoice.Add(new GrammarBuilder(commandSemantic));

                commandSemantic = new SemanticResultValue(Properties.Settings.Default.computerName + " " + spoken + " please", command);
                commandChoice.Add(new GrammarBuilder(commandSemantic));
            }
        }

        private void loadWaves()
        {
            greetingsEvening.Clear();
            greetingsEvening.Add("Good evening.wav");
            greetingsEvening.Add("Greetings.wav");
            greetingsEvening.Add("Hello.wav");
            greetingsEvening.Add("Welcome.wav");
            greetingsEvening.Add("welcome back.wav");

            greetingsMorning.Clear();
            greetingsMorning.Add("Good morning.wav");
            greetingsMorning.Add("Greetings.wav");
            greetingsMorning.Add("Hello.wav");
            greetingsMorning.Add("Welcome.wav");
            greetingsMorning.Add("welcome back.wav");

            greetingsAfternoon.Clear();
            greetingsAfternoon.Add("Greetings.wav");
            greetingsAfternoon.Add("Hello.wav");
            greetingsAfternoon.Add("Welcome.wav");
            greetingsAfternoon.Add("welcome back.wav");

            greetingsPresense.Clear();
            greetingsPresense.Add("I'm here.wav");
            greetingsPresense.Add("I'm here 2.wav");
            greetingsPresense.Add("Standing by.wav");
            greetingsPresense.Add("Yes.wav");
            greetingsPresense.Add("Yes 2.wav");
            greetingsPresense.Add("I am around.wav"); 
        }

        private void sayGreeting()
        {
            int hour = DateTime.Now.Hour;
            if (hour <= 4 || hour >= 17)
            {
                writeLog("Voice:  Saying evening greeting");
                kodiPlayWave (greetingsEvening[random.Next(greetingsEvening.Count)]);
            }
            else if (hour < 12)
            {
                writeLog("Voice:  Saying morning greeting");
                kodiPlayWave(greetingsMorning[random.Next(greetingsMorning.Count)]);
            }
            else
            {
                writeLog("Voice:  Saying afternoon greeting");
                kodiPlayWave(greetingsAfternoon[random.Next(greetingsAfternoon.Count)]);
            }
        }

        private void sayPresense()
        {     
            writeLog("Voice:  Saying acknowledgement");
            kodiPlayWave(greetingsPresense[random.Next(greetingsPresense.Count)]);
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
                    string kodiMovieFile = topPhrase.Split('|')[1];
                    MessageBox.Show(kodiMovieFile);
                }
                else
                {
                    switch (topPhrase)
                    {
                        case "Turn on Theatre":

                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.harmonyStartActivityByName(Properties.Settings.Default.voiceActivity);
                                formMain.timerStartLights.Enabled = true;
                                formMain.writeLog("Voice:  Processed " + topPhrase);
                            }
                            ));
                            break;
                        case "Turn off Theatre":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
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
                                    formMain.kodiPlayWave(ackWave);
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
                                    formMain.kodiPlayWave(ackWave);
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
                                formMain.kodiPlayWave(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.lightsToEnteringLevel();
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Pause Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Pause");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Resume Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Play");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Stop Playback":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
                                formMain.labelLastVoiceCommand.Text = topPhrase;
                                formMain.kodiPlaybackControl("Stop");
                                formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                            }
                            ));
                            break;
                        case "Dim Lights":
                            formMain.BeginInvoke(new Action(() =>
                            {
                                formMain.kodiPlayWave(ackWave);
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