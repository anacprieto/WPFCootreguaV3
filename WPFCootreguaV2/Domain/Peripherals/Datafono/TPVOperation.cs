using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trx;

namespace WPFCootreguaV2.Domain.Peripherals
{
    public delegate void DatafonoResponseHandler(string responseTPV);
    public class TPVOperation
    {
        public static int Quotas { get; set; }

        TEFTransactionManager _transactionManager;

        public static event DatafonoResponseHandler DatafonoResponse;

        public static string UnlockCommand = "[R,61,0]38";
        public TPVOperation()
        {
            _transactionManager = new TEFTransactionManager();
        }

        public void SendRequest(string data)
        {
            string response;
            try
            {
                response = _transactionManager.getTEFAuthorization(data);
            }

            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución {ex.Message}", ex);
                response = ex.Message;
            }

            if (data != UnlockCommand) DatafonoResponse?.Invoke(response);
           

        }

        public void SendWaitingRequest()
        {
            var response =  _transactionManager.getTEFAuthorization();
            DatafonoResponse?.Invoke(response);
            
        }

        public string CalculateLRC(string s)
        {
            int checksum = 0;
            foreach (Char c in GetStringFromHex(s))
            {
                checksum = checksum ^ Convert.ToByte(c);
            }
            string nuevaCadena = string.Concat("[", s, checksum.ToString("X2"));
            return nuevaCadena;
        }

        private string GetStringFromHex(string s2)
        {
            string result = string.Empty;
            var result2 = string.Join("", s2.Select(c => ((int)c).ToString("X2")));
            for (int i = 0; i < result2.Length; i = i + 2)
            {
                result += Convert.ToChar(int.Parse(result2.Substring(i, 2),
               System.Globalization.NumberStyles.HexNumber));
            }
            return result;
        }
    }
}
