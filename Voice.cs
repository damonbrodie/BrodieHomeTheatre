using System;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;
using System.Collections.Generic;


namespace BrodieTheatre
{
    public partial class FormMain : Form
    {
        private void speakText(string tts)
        {
            speechSynthesizer.SpeakAsync(tts);
        }

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

            commandSemantic = new SemanticResultValue("Are you There", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Hello Ronda", "Greeting");
            commandChoice.Add(new GrammarBuilder(commandSemantic));

            commandSemantic = new SemanticResultValue("Hello Ronda", "Greeting");
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


            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            recognitionEngine.LoadGrammar(grammar);
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
            List<string> greetings = new List<string>(new string[] { timeGreeting, "hello there", "how can I help you" });
            Random rnd = new Random();
            int r = rnd.Next(greetings.Count);
            speakText(greetings[r]);
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Alternates != null && labelKodiStatus.Text != "Playing")
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
                        }
                        ));
                        break;
                    case "Turn off Theatre":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.startActivityByName("PowerOff");
                            formMain.lightsToEnteringLevel();
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
                            }
                        }
                        ));
                        break;
                    case "Greeting":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.sayGreeting();
                        }
                        ));
                        break;

                    case "Lights On":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToEnteringLevel();
                        }
                        ));
                        break;
                    case "Dim Lights":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            formMain.labelLastVoiceCommand.Text = topPhrase;
                            formMain.lightsToStoppedLevel();
                        }
                        ));
                        break;
                }
            }
        }
    }
}