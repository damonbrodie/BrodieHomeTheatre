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
            greetingsEvening.Add(waveToBuffer(Properties.Resources.Good_evening));
            greetingsEvening.Add(waveToBuffer(Properties.Resources.Greetings));
            greetingsEvening.Add(waveToBuffer(Properties.Resources.Hello));
            greetingsEvening.Add(waveToBuffer(Properties.Resources.Welcome));
            greetingsEvening.Add(waveToBuffer(Properties.Resources.welcome_back));

            greetingsMorning.Add(waveToBuffer(Properties.Resources.Good_morning));
            greetingsMorning.Add(waveToBuffer(Properties.Resources.Greetings));
            greetingsMorning.Add(waveToBuffer(Properties.Resources.Hello));
            greetingsMorning.Add(waveToBuffer(Properties.Resources.Welcome));
            greetingsMorning.Add(waveToBuffer(Properties.Resources.welcome_back));

            greetingsAfternoon.Add(waveToBuffer(Properties.Resources.Greetings));
            greetingsAfternoon.Add(waveToBuffer(Properties.Resources.Hello));
            greetingsAfternoon.Add(waveToBuffer(Properties.Resources.Welcome));
            greetingsAfternoon.Add(waveToBuffer(Properties.Resources.welcome_back));

            presense.Add(waveToBuffer(Properties.Resources.I_m_here));
            presense.Add(waveToBuffer(Properties.Resources.I_m_here_2));
            presense.Add(waveToBuffer(Properties.Resources.Standing_by));
            presense.Add(waveToBuffer(Properties.Resources.Yes));
            presense.Add(waveToBuffer(Properties.Resources.Yes_2));
            presense.Add(waveToBuffer(Properties.Resources.I_am_around));

            soundPoweringUp = waveToBuffer(Properties.Resources.long_Powering_Up);
        }

        private SecondarySoundBuffer waveToBuffer(System.IO.UnmanagedMemoryStream wave)
        {
            WaveStream waveFile = new WaveStream(wave);
            SoundBufferDescription soundBufferDescription = new SoundBufferDescription();
            soundBufferDescription.SizeInBytes = (int)waveFile.Length;
            soundBufferDescription.Flags = BufferFlags.None;
            soundBufferDescription.Format = waveFile.Format;
            SecondarySoundBuffer buffer = new SecondarySoundBuffer(directSound, soundBufferDescription);
            byte[] data = new byte[soundBufferDescription.SizeInBytes];
            waveFile.Read(data, 0, (int)waveFile.Length);
            buffer.Write(data, 0, LockFlags.None);
            return buffer;
        }

        private void sayGreeting()
        {
            Random rnd = new Random();
            int hour = DateTime.Now.Hour;
            if (hour <= 4 || hour >= 17)
            {
                SecondarySoundBuffer currBuffer = greetingsEvening[rnd.Next(greetingsEvening.Count)];
                currBuffer.Play(0, PlayFlags.None);
            }
            else if (hour < 12)
            {
                SecondarySoundBuffer currBuffer = greetingsMorning[rnd.Next(greetingsMorning.Count)];
                currBuffer.Play(0, PlayFlags.None);
            }
            else
            {
                SecondarySoundBuffer currBuffer = greetingsAfternoon[rnd.Next(greetingsAfternoon.Count)];
                currBuffer.Play(0, PlayFlags.None);
            }

        }

        private void sayPresense()
        {
            Random rnd = new Random();
            SecondarySoundBuffer currBuffer = presense[rnd.Next(presense.Count)];
            currBuffer.Play(0, PlayFlags.None);
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