using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using multimodal;

namespace speechModality
{
    public class SpeechMod
    {
        private SpeechRecognitionEngine sre;
        private Grammar gr;
        public event EventHandler<SpeechEventArg> Recognized;

        private Tts t;
        private string[] commandsNotUnderstand = new string[5];
        private Random rnd = new Random();

        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION","speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);

            
            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

            t = new Tts();
            commandsNotUnderstand[0] = "Desculpe, pode repetir?";
            commandsNotUnderstand[1] = "Parece que estou a ficar surda, importa-se de repetir?";
            commandsNotUnderstand[2] = "Pode repetir o que disse?";
            commandsNotUnderstand[3] = "Não queria ser inconveniente, mas pode repetir?";
            commandsNotUnderstand[4] = "Não percebi o que disse.";

        }

        private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = false });
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg(){Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true});

            // CHANGED FOR FUSION ---------------------------------------
            //SEND
            // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
            string json = "{ \"recognized\": [";
            //json += "\"" + e.Result.Confidence + "\", ";
            foreach (var resultSemantic in e.Result.Semantics)
            {
                json+= "\"" + resultSemantic.Value.Value +"\", ";
            }
            json = json.Substring(0, json.Length - 2);
            json += "] }";
            // END CHANGED FOR FUSION ---------------------------------------

            var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime+"", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration)+"",e.Result.Confidence, json);
            if (e.Result.Confidence > 0.65)
            {
                mmic.Send(exNot);
            }
            else
            {
                t.Speak(commandsNotUnderstand[rnd.Next(0, 5)]);
            }
        }
    }
}
