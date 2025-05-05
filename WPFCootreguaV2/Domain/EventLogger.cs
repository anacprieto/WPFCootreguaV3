using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using WPFCootreguaV2.Domain.Variables;

namespace WPFCootreguaV2.Domain
{
    public static class EventLogger
    {
        public static void SaveLog(EventType type, string msg, object? obj = null,
            [CallerMemberName] string method = "", [CallerFilePath] string callerPath = "")
        {
            // TODO: poner error de cierre de aplicación en caso de que falle el log de eventos
            var _class = Path.GetFileNameWithoutExtension(callerPath);
            var _event = new Event
            {
                Time = DateTime.Now.ToString("hh:mm:ss.fff tt"),
                IdTransaction = 0,
                Type = type.ToString(),
                Class = $"{_class}",
                Method = method,
                Message= msg,
                Obj = obj,
            };

            if (type.ToString().StartsWith("P"))
                WriteFile(_event, "Log_peripherals");
            else if (type.ToString().Contains("Integration"))
                WriteFile(_event, "Log_integration");
            else
                WriteFile(_event, "Log_app");
        }

        
        private static void WriteFile(Event evt, string folder)
        {
            try
            {
                var json = JsonConvert.SerializeObject(evt, Formatting.Indented);

                
                var logDir = Path.Combine(AppInfo.APP_DIR, folder) ;
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                var fileName = "Log" + DateTime.Now.ToString("yyyy-MM-dd") + ".json";
                var filePath = Path.Combine(logDir, fileName);

                if (!File.Exists(filePath))
                {
                    var archivo = File.CreateText(filePath);
                    archivo.Close();
                }

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(json);
                }
            }
            catch (Exception ex)
            {

            }
            
        }
    }

    public class Event
    {
        public string Time { get; set; }
        public int IdTransaction { get; set; }
        public string Type { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public string Message { get; set; }
        public object? Obj { get; set; }
    }

    public enum EventType
    {
        FatalError,
        Error,
        Warning,
        Info,
        P_Acceptor,
        P_Arduino,
        Integration,
        P_Dispenser

    }
}
