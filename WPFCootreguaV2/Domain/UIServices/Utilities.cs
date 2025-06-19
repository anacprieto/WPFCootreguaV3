using Encryptor.Ecity.Dll;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.UIServices
{
    public class Utilities
    {
        private static SpeechSynthesizer speechSynthesizer;
        private static Transaction _ts;



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
        public static decimal RoundValue(decimal Total, bool arriba)
        {
            try
            {
                decimal roundTotal = 0;

                if (arriba)
                {
                    roundTotal = Math.Ceiling(Total / 100) * 100;
                }
                else
                {
                    roundTotal = Math.Floor(Total / 100) * 100;
                }

                return roundTotal;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex);
                return Total;
            }
        }

        public static bool ValidateModule(decimal module, decimal amount)
        {
            try
            {
                var result = (amount % module);
                return result == 0 ? true : false;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex);
                return false;
            }
        }
    }
}
