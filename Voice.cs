using System;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;
using SlimDX.DirectSound;
using SlimDX.Multimedia;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void loadVoiceCommands()
        {
            GrammarBuilder gb = new GrammarBuilder();
            Choices commandChoice = new Choices();

            SemanticResultValue commandSemantic = new SemanticResultValue("Turn on Projector", "Turn on Theatre");
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

            commandSemantic = new SemanticResultValue("Are you There", "Presense");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Are you Listening", "Presense");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Hello Ronda", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("OK Ronda", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Dim the Lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Turn Down Lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Turn Down the Lights", "Dim Lights");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Turn on Lights", "Lights On");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Turn on the Lights", "Lights On");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Raise the Lights", "Lights On");
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
            greetingsPresense.Add("Standing_by.wav");
            greetingsPresense.Add("Yes.wav");
            greetingsPresense.Add("Yes 2.wav");
            greetingsPresense.Add("I am around.wav");  
        }

        private void sayGreeting()
        {
            Random rnd = new Random();
            int hour = DateTime.Now.Hour;
            if (hour <= 4 || hour >= 17)
            {
                writeLog("Voice:  Saying evening greeting");
                //SecondarySoundBuffer currBuffer = greetingsEvening[rnd.Next(greetingsEvening.Count)];
                //currBuffer.Play(0, PlayFlags.None);
            }
            else if (hour < 12)
            {
                writeLog("Voice:  Saying morning greeting");
                //SecondarySoundBuffer currBuffer = greetingsMorning[rnd.Next(greetingsMorning.Count)];
                //currBuffer.Play(0, PlayFlags.None);
            }
            else
            {
                writeLog("Voice:  Saying afternoon reeting");
               // SecondarySoundBuffer currBuffer = greetingsAfternoon[rnd.Next(greetingsAfternoon.Count)];
                //currBuffer.Play(0, PlayFlags.None);
            }

        }

        private void sayPresense()
        {
            Random rnd = new Random();
            writeLog("Voice:  Saying acknowledgement");
            //SecondarySoundBuffer currBuffer = presense[rnd.Next(presense.Count)];
            //currBuffer.Play(0, PlayFlags.None);
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            writeLog("Voice:  Recognized Speech '" + e.Result.Text + "' Confidence " + e.Result.Confidence.ToString());
            if (e.Result.Alternates != null && e.Result.Confidence > (float)0.90 && labelKodiStatus.Text != "Playing")
            {
                formMain.BeginInvoke(new Action(() =>
                {
                    toolStripStatus.Text = e.Result.Text;
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
                            writeLog("Recognized: " + topPhrase);
                        }
                        ));
                        break;
                    case "Turn off Theatre":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.startActivityByName("PowerOff");
                            formMain.lightsToEnteringLevel();
                            writeLog("Recognized: " + topPhrase);
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
                                writeLog("Recognized: " + topPhrase);
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
                                writeLog("Recognized: " + topPhrase);
                            }
                        }
                        ));
                        break;
                    case "Greeting":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.sayGreeting();
                            writeLog("Recognized: " + topPhrase);
                        }
                        ));
                        break;
                    case "Presense":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.sayPresense();
                            writeLog("Recognized: " + topPhrase);
                        }
                        ));
                        break;
                    case "Lights On":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToEnteringLevel();
                            writeLog("Recognized: " + topPhrase);
                        }
                        ));
                        break;
                    case "Dim Lights":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToStoppedLevel();
                            writeLog("Recognized: " + topPhrase);
                        }
                        ));
                        break;
                }
            }
        }
    }
}