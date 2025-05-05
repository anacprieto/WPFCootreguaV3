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
using WPFCootreguaV2.Domain.UIServices.Integrations;

namespace WPFCootreguaV2.Domain.ApiService
{
    public static class ApiIntegration
    {
        private static GenericAPI _httpHelper;
        private static string _baseAddress;
        private static string _apiKey;
        private static string _clientId;
        private static string _token;
        private static HttpClient _client;
        private static Dictionary<string, string> _defaultHeaders;
        private static HttpClient client;
        private static string BasseAddress;
        private static string Token;
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
                // Configurar el handler para manejar problemas de certificados SSL
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        // Verificar si el error es RemoteCertificateNameMismatch
                        if (policyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
                        {
                            return true; // Ignorar el error de nombre incorrecto
                        }
                        // Retornar true si no hay errores, false si hay otros errores
                        return policyErrors == SslPolicyErrors.None;
                    }
                };

                // Preparar la URL completa
                string baseUrl = AppConfig.Get("basseAddressCootregua");
                //ControllerCootreguaGetStateProduct
                string endpoint = AppConfig.Get(controller);
                string url = string.Concat(baseUrl, endpoint);


                // Registrar los datos enviados antes de encriptar
                EventLogger.SaveLog(EventType.Info, "DATA ENVIADA "+ " "+endpoint + JsonConvert.SerializeObject(data));

                var encryptedData = Encryptor.Encrypt(JsonConvert.SerializeObject(data), Key);



                // Encriptar los datos
                EventLogger.SaveLog(EventType.Info, "DATA ENVIADA ENCRYPTADA {endpoint}: " + encryptedData, "");


                // Crear el objeto RequestGlobal con los datos encriptados
                RequestGlobal requestGlobal = new RequestGlobal
                {
                    Data = encryptedData,
                    CallDate = DateTime.Now
                };

                // Serializar el objeto a JSON
                string jsonPayload = JsonConvert.SerializeObject(requestGlobal);

                // Crear el contenido HTTP
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Crear cliente HTTP y configurar headers
                using (var httpClient = new HttpClient(handler))
                {
                    // Añadir headers necesarios
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Realizar la petición POST
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    // Verificar si la respuesta fue exitosa
                    if (!response.IsSuccessStatusCode)
                    {
                        //AdminPayPlus.SaveErrorControl($"ERROR {endpoint}: " + response.ReasonPhrase, "", EError.Api, ELevelError.Medium);
                        return string.Empty;
                    }

                    // Leer el contenido de la respuesta
                    string responseContent = await response.Content.ReadAsStringAsync();

                    // Deserializar la respuesta
                    //var responseApi = JsonConvert.DeserializeObject<ResponseCootregua>(responseContent);

                    //// Manejar la respuesta según el código de respuesta
                    //switch (responseApi.ResponseCode)
                    //{
                    //    case EResponseCode.Error:
                    //        AdminPayPlus.SaveErrorControl(responseApi.ResponseMessage + " Cootregua", "", EError.Api, ELevelError.Medium);
                    //        break;
                    //    case EResponseCode.Ok:
                    //        AdminPayPlus.SaveErrorControl($"DATA RECIBIDA ENCRYPTADA {endpoint}: " + responseApi.ResponseData.ToString(), "", EError.Api, ELevelError.Medium);
                    //        string dataApi = EncryptorEcity.Decrypt(responseApi.ResponseData.ToString(), Key);
                    //        AdminPayPlus.SaveErrorControl($"DATA RECIBIDA DESENCRYPTADA {endpoint}: " + dataApi, "", EError.Api, ELevelError.Medium);
                    //        return dataApi;
                    //    default:
                    //        break;
                    //}
                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(System.Reflection.MethodBase.GetCurrentMethod().Name, "ApiIntegration", ex, ex.ToString());
            }

            return string.Empty;
        }
    }
}
