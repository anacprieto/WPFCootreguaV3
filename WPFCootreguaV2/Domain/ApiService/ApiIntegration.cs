using ControlzEx.Standard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.UIServices.Integrations;
using System.IO;
using System.Net;
using System.Security.Cryptography.Xml;
using Microsoft.VisualBasic;


namespace WPFCootreguaV2.Domain.ApiService
{
    public static class ApiIntegration
    {
        private static GenericAPI _httpHelper;
        private static string _baseAddress;
        private static string _apiKey;
        //private static string _clientId;
        //private static string _token;
        //private static HttpClient _client;
        private static Dictionary<string, string> _defaultHeaders;
        //private static HttpClient client;
        //private static string BasseAddress;
        //private static string Token;
        private static string Key;

        static ApiIntegration()
        {
            _httpHelper = new GenericAPI(baseAddress: AppConfig.Get("basseAddressCootregua"), apiName: "API cootregua");

            //_apiKey = AppConfig.Get("DobleClickInternaKey");

            CleanHeaders();

        }

        public static void CleanHeaders()
        {

            _defaultHeaders = new Dictionary<string, string>
            {
                { "X-API-KEY", _apiKey }
            };
        }

        public static async Task<string> CallApiCootregua(string controller, object data)
        {
            try
            {
                // Configuración de URL base constante si AppConfig no está funcionando
                string baseAddress = AppConfig.Get("basseAddressCootregua");

                // Si la configuración no está disponible, usa la URL conocida
                if (string.IsNullOrEmpty(baseAddress))
                {
                    baseAddress = "https://apicootregua.e-city.co";
                    EventLogger.SaveLog(EventType.Warning, "Usando URL base predeterminada porque la configuración está vacía");
                }

                // Obtener el endpoint del controlador
                string endpoint = AppConfig.Get(controller);

                // Si el endpoint está vacío, construirlo según el parámetro del controlador
                if (string.IsNullOrEmpty(endpoint))
                {
                    // Ejemplo: Si controller es "ValidateUsers", construir "/Login/ValidateUsers"
                    if (controller == "ValidateUsers")
                    {
                        endpoint = "/Login/ValidateUsers";
                    }
                    else
                    {
                        // Puedes agregar más mapeos según los endpoints que necesites
                        EventLogger.SaveLog(EventType.Error, $"No se pudo determinar el endpoint para el controlador '{controller}'");
                        return string.Empty;
                    }
                }

                // Asegurarse de que la URL base termina con "/" si el endpoint no comienza con "/"
                if (!baseAddress.EndsWith("/") && !endpoint.StartsWith("/"))
                {
                    baseAddress += "/";
                }

                // Construir la URL completa
                string url = string.Concat(baseAddress, endpoint);

                // Log para depuración
                EventLogger.SaveLog(EventType.Info, $"URL construida: {url}");

                // Validar que la URL no esté vacía y sea válida
                if (string.IsNullOrEmpty(url))
                {
                    EventLogger.SaveLog(EventType.Error, "La URL construida está vacía");
                    return string.Empty;
                }

                // Intentar validar formato de URL
                try
                {
                    // Esto lanzará excepción si la URL no tiene un formato válido
                    var uri = new Uri(url);
                }
                catch (UriFormatException ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"La URL construida no es válida: {url}. Error: {ex.Message}");
                    return string.Empty;
                }

                // Log de datos enviados
                EventLogger.SaveLog(EventType.Info, "DATA ENVIADA " + " " + url + JsonConvert.SerializeObject(data));

                // Encriptar los datos
                var requestClient = EncryptorEcity.Encrypt(JsonConvert.SerializeObject(data), Key);
                EventLogger.SaveLog(EventType.Info, $"DATA ENVIADA ENCRYPTADA {endpoint}: {requestClient}");

                // Crear el objeto RequestGlobal
                RequestGlobal requestGlobal = new RequestGlobal
                {
                    Data = requestClient,
                    CallDate = DateTime.Now
                };

                // Serializar el cuerpo de la solicitud
                string payload = JsonConvert.SerializeObject(requestGlobal);

                // CORRECCIÓN: Crear el StringContent con el tipo de contenido correcto directamente
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                // Configura otros encabezados HTTP (excepto Content-Type)
                var headers = new Dictionary<string, string>();
                // Puedes agregar otros encabezados que necesites, pero NO Content-Type
                // Por ejemplo:
                // headers.Add("Authorization", "Bearer miToken");

                // Realizar la solicitud usando _httpHelper
                var response = await _httpHelper.DoPostRequest<ResponseCootregua>(url, content, headers);

                // Procesar la respuesta
                switch (response.ResponseCode)
                {
                    case EResponseCode.Error:
                        EventLogger.SaveLog(EventType.Error, $"Error de API Cootregua: {response.ResponseMessage}");
                        break;
                    case EResponseCode.Ok:
                        EventLogger.SaveLog(EventType.Info, $"DATA RECIBIDA ENCRYPTADA: {response.ResponseData}");
                        string dataApi = EncryptorEcity.Decrypt(response.ResponseData.ToString(), Key);
                        EventLogger.SaveLog(EventType.Info, $"DATA RECIBIDA DESENCRYPTADA: {dataApi}");
                        return dataApi;
                    default:
                        EventLogger.SaveLog(EventType.Warning, $"Código de respuesta no manejado: {response.ResponseCode}");
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"ERROR EN EL METODO CallApiCootregua: {ex.Message}");
                EventLogger.SaveLog(EventType.Error, $"Stack Trace: {ex.StackTrace}");
            }

            return string.Empty;
        }
        /*
        public static async Task<string> CallApiCootregua(string controller, object data)
        {
            try
            {
                // Construir la URL
                string url = string.Concat(AppConfig.Get("basseAddressCootregua"), AppConfig.Get(controller));

                // Log de datos enviados
                EventLogger.SaveLog(EventType.Info, "DATA ENVIADA " + " " + url + JsonConvert.SerializeObject(data));


                // Encriptar los datos
                var requestClient = EncryptorEcity.Encrypt(JsonConvert.SerializeObject(data), Key);
                EventLogger.SaveLog(EventType.Info, "DATA ENVIADA ENCRYPTADA {endpoint}: " + requestClient, "");

                // Crear el objeto RequestGlobal
                RequestGlobal requestGlobal = new RequestGlobal
                {
                    Data = requestClient,
                    CallDate = DateTime.Now
                };

                // Serializar el cuerpo de la solicitud
                string payload = JsonConvert.SerializeObject(requestGlobal);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                // Configurar headers
                var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

                // Realizar la solicitud usando _httpHelper
                var response = await _httpHelper.DoPostRequest<ResponseCootregua>(url, content, headers);


                // Procesar la respuesta
                switch (response.ResponseCode)
                {
                    case EResponseCode.Error:
                        EventLogger.SaveLog(EventType.Error, response.ResponseMessage + "Cootregua");
                        break;
                    case EResponseCode.Ok:
                        EventLogger.SaveLog(EventType.Error, response.ResponseData + "DATA RECIBIDA ENCRYPTADA");
                        string dataApi = EncryptorEcity.Decrypt(response.ResponseData.ToString(), Key);
                        EventLogger.SaveLog(EventType.Error, dataApi + "DATA RECIBIDA DESENCRYPTADA ");
                        return dataApi;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error,"EROOR EN EL METODO:CallApiCootregua ");

            }

            return string.Empty;
        }*/
    }
}
