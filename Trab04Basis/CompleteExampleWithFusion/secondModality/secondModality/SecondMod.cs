using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.Windows;

namespace secondModality
{
    public class SecondMod
    {



        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        public SecondMod()
        {
            //init LifeCycleEvents..


            // CHANGED FOR FUSION ---------------------------------------

            lce = new LifeCycleEvents("TOUCH", "FUSION", "touch-1", "touch", "command");
            mmic = new MmiCommunication("localhost", 9876, "User1", "TOUCH");  //CHANGED To user1

            // END CHANGED FOR FUSION------------------------------------

            mmic.Send(lce.NewContextRequest());





        }

        //  NEW 
        //  A TEIXEIRA , 16 MAY 2018

        internal void sendToFusion(string value)
        {
             
        

        //SEND
        string json = "{ \"recognized\": [";
        //foreach (var resultSemantic in e.Result.Semantics)

        String key = "color";
         
        json += "\"" + key + "\",\"" + value + "\", ";

        json = json.Substring(0, json.Length - 2);


        json += " ] }";

        //  start time, end time, confidence, json  TO BE COMPLETED
        var exNot = lce.ExtensionNotification("-1", "-1", 1.0f, json);

            
        mmic.Send(exNot);


        }


        internal void sendToFusion(string value, string value2)
        {



            //SEND
            string json = "{ \"recognized\": [";
            //foreach (var resultSemantic in e.Result.Semantics)

            String key = "shape";

            json += "\"" + key + "\",\"" + value + "\", ";

            key = "color";

            json += "\"" + key + "\",\"" + value2 + "\", ";
        
            json = json.Substring(0, json.Length - 2);
              
  
            json += "] }";

            //  start time, end time, confidence, json  TO BE COMPLETED
            var exNot = lce.ExtensionNotification("-1", "-1", 1.0f, json);
            
            mmic.Send(exNot);


        }

        /*
            private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
            {
                onRecognized(new SpeechEventArg(){Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true});


                // CHANGED FOR FUSION ---------------------------------------
                //SEND
                string json = "{ \"recognized\": [";
                foreach (var resultSemantic in e.Result.Semantics)
                {
                    json+= "\""+resultSemantic.Key + "\",\"" + resultSemantic.Value.Value +"\", ";
                }
                json = json.Substring(0, json.Length - 2);
                json += "] }";

                // END CHANGED FOR FUSION ---------------------------------------

                var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime+"", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration)+"",e.Result.Confidence, json);
                mmic.Send(exNot);
            }

        */

    }
}
