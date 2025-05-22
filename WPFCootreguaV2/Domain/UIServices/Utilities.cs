using Encryptor.Ecity.Dll;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.UIServices
{
    public class Utilities
    {
        private static SpeechSynthesizer speechSynthesizer;

        public static string GetConfiguration(string key, bool decodeString = false)
        {
            try
            {
                string value = "";
                AppSettingsReader reader = new AppSettingsReader();
                value = reader.GetValue(key, typeof(String)).ToString();
                if (decodeString)
                {
                    value = EncryptorEcity.Decrypt(value, "WPFCootregua");
                }
                return value;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex, ex.ToString());
                return string.Empty;
            }
        }
        public static void Speak(string text)
        {
            try
            {
                if (GetConfiguration("ActivateSpeak").ToUpper() == "SI")
                {
                    if (speechSynthesizer == null)
                    {
                        speechSynthesizer = new SpeechSynthesizer();
                    }

                    speechSynthesizer.SpeakAsyncCancelAll();
                    speechSynthesizer.SelectVoice("Microsoft Sabina Desktop");
                    speechSynthesizer.SpeakAsync(text);
                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex, ex.ToString());
            }
        }


    }
}
