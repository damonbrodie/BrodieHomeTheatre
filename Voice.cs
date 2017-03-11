﻿using System;
using System.Windows.Forms;
using Microsoft.Speech.Recognition;


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

            gb.Append(commandChoice);

            Grammar grammar = new Grammar(gb);

            grammar.Name = "commands";
            recognitionEngine.LoadGrammar(grammar);
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Alternates != null && labelRoomOccupancy.Text == "Occupied" && labelKodiStatus.Text != "Playing")
            {
                toolStripStatus.Text = e.Result.Text;
                RecognizedPhrase phrase = e.Result.Alternates[0];

                string topPhrase = phrase.Semantics.Value.ToString();

                switch (topPhrase)
                {
                    case "Turn on Theatre":

                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelLastVoiceCommand.Text = topPhrase;
                            toolStripStatus.Text = "Starting Home Theatre";
                            startActivityByName(Properties.Settings.Default.voiceActivity);
                            formMain.timerStartLights.Enabled = true;
                        }
                        ));
                        break;
                    case "Turn off Theatre":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            labelLastVoiceCommand.Text = phrase.Semantics.Value.ToString();
                            toolStripStatus.Text = "Stopping Home Theatre";
                            startActivityByName("PowerOff");
                            formMain.toolStripStatus.Text = "Turning on pot lights";
                            formMain.setLightLevel(Properties.Settings.Default.potsAddress, (Properties.Settings.Default.potsEnteringLevel * 10));
                            formMain.trackBarPots.Value = Properties.Settings.Default.potsEnteringLevel;
                            formMain.toolStripStatus.Text = "Turning on tray lights";
                            formMain.setLightLevel(Properties.Settings.Default.trayAddress, (Properties.Settings.Default.trayEnteringLevel * 10));
                            formMain.trackBarTray.Value = Properties.Settings.Default.trayEnteringLevel;
                        }
                        ));
                        break;
                    case "Show Application":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            if (formMain.WindowState == FormWindowState.Minimized)
                            {
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
                                formMain.WindowState = FormWindowState.Minimized;
                            }
                        }
                        ));
                        break;
                    case "Greeting":
                        formMain.BeginInvoke(new Action(() =>
                        {
                            sayGreeting();
                        }
                        ));
                        break;
                }
            }
        }
    }
}