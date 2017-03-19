using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void loadVoiceCommands()
        {
            GrammarBuilder gb = new GrammarBuilder();
            Choices commandChoice = new Choices();

            SemanticResultValue commandSemantic = new SemanticResultValue("turn on projector", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("projector on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("home theater on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("let's watch a movie", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("start theater", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on projector", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power on theater", "Turn on Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn off projector", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("projector off", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("home theater off", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("shutdown theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn off theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power down theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("power off theater", "Turn off Theatre");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("show yourself", "Show Application");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("show application", "Show Application");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("hide yourself", "Hide Application");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("hide application", "Hide Application");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("are you there", "Presense");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("are you listening", "Presense");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Hello Ronda", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("OK Ronda", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("dim the lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn down lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn down the lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn on lights", "Lights On");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("turn on the lights", "Lights On");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("raise the lights", "Lights On");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            recognitionEngine.LoadGrammar(grammar);

            loadWaves();
        }

        private void loadWaves()
        {
            greetingsEvening.Add("Good evening.wav");
            greetingsEvening.Add("Greetings.wav");
            greetingsEvening.Add("Hello.wav");
            greetingsEvening.Add("Welcome.wav");
            greetingsEvening.Add("welcome back.wav");

            greetingsMorning.Add("Good morning.wav");
            greetingsMorning.Add("Greetings.wav");
            greetingsMorning.Add("Hello.wav");
            greetingsMorning.Add("Welcome.wav");
            greetingsMorning.Add("welcome back.wav");

            greetingsAfternoon.Add("Greetings.wav");
            greetingsAfternoon.Add("Hello.wav");
            greetingsAfternoon.Add("Welcome.wav");
            greetingsAfternoon.Add("welcome back.wav");

            greetingsPresense.Add("I'm here.wav");
            greetingsPresense.Add("I'm here 2.wav");
            greetingsPresense.Add("Standing by.wav");
            greetingsPresense.Add("Yes.wav");
            greetingsPresense.Add("Yes 2.wav");
            greetingsPresense.Add("I am around.wav"); 
        }

        private void kodiPlayWave(string file)
        {
            StreamWriter fileHandle = new StreamWriter(waveFile);
            fileHandle.WriteLine(Path.Combine(wavePath,file));
            fileHandle.Close();
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
            float minConfidence = Properties.Settings.Default.voiceConfidence / 100;
            if (e.Result.Alternates != null && e.Result.Confidence > minConfidence && labelKodiStatus.Text != "Playing")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    formMain.writeLog("Voice:  Recognized Speech '" + e.Result.Text + "' Confidence " + confidence+"%");
                    formMain.toolStripStatus.Text = "Heard:  '" + e.Result.Text + "' (" + confidence + "%)";
                }
                ));
                RecognizedPhrase phrase = e.Result.Alternates[0];

                string topPhrase = phrase.Semantics.Value.ToString();
     
                switch (topPhrase)
                {
                    case "Turn on Theatre":

                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.startActivityByName(Properties.Settings.Default.voiceActivity);
                            formMain.timerStartLights.Enabled = true;
                            formMain.writeLog("Voice:  Processed " + topPhrase);
                        }
                        ));
                        break;
                    case "Turn off Theatre":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.startActivityByName("PowerOff");
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
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToEnteringLevel();
                            formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                        }
                        ));
                        break;
                    case "Dim Lights":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToStoppedLevel();
                            formMain.writeLog("Voice:  Processed '" + topPhrase + "'");
                        }
                        ));
                        break;
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