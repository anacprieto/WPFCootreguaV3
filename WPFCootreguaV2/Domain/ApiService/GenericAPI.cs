using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.ApiService
{
    public class GenericAPI
    {
        private string _baseAddress { get; set; }
        private string _apiName { get; set; }
        public GenericAPI(string baseAddress, string apiName)
        {
            _baseAddress = baseAddress;
            _apiName = apiName;
        }
        public async Task<T?> DoPostRequest<T>(string endpoint, StringContent content, Dictionary<string, string>? customHeaders = null, HttpClientHandler? handler = null)
        {

            HttpClient client = handler != null ? new(handler) : new();
            client.BaseAddress = new Uri(_baseAddress);

            if (customHeaders != null) client.AddHeaders(customHeaders);

            client.Timeout = TimeSpan.FromMilliseconds(30000);
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(endpoint, content);
            }
            catch (HttpRequestException ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error de conexión a la API {_apiName}: {ex.Message}", ex);
                return default;
            }
            catch (TaskCanceledException ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Timeout al conectar con la API {_apiName}: {ex.Message}", ex);
                return default;
            }

            var result = await response.Content.ReadAsStringAsync();
            if (result == null)
            {
                EventLogger.SaveLog(EventType.Error, $"No se obtuvo contenido de la api: {_apiName}");
                return default;
            }

            var requestresponse = JsonConvert.DeserializeObject<T>(result);
            if (requestresponse == null)
            {
                EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                return default;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {

                return requestresponse;
            }

            EventLogger.SaveLog(EventType.Error, $"Api: {_apiName} no respondió satisfactoriamente", requestresponse);
            return default;
        }

        public async Task<T?> DoGetRequest<T>(string endpoint, Dictionary<string, string>? customHeaders = null, HttpClientHandler? handler = null)
        {

            HttpClient client = handler != null ? new(handler) : new();
            client.BaseAddress = new Uri(_baseAddress);
            if (customHeaders != null) client.AddHeaders(customHeaders);

            client.Timeout = TimeSpan.FromMilliseconds(30000);
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(endpoint);
            }
            catch (HttpRequestException ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error de conexión a la API {_apiName}: {ex.Message}", ex);
                return default;
            }
            catch (TaskCanceledException ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Timeout al conectar con la API {_apiName}: {ex.Message}", ex);
                return default;
            }

            var result = await response.Content.ReadAsStringAsync();
            if (result == null)
            {
                EventLogger.SaveLog(EventType.Error, $"No se obtuvo contenido de la api: {_apiName} de archivos planos interna");
                return default;
            }

            var requestresponse = JsonConvert.DeserializeObject<T>(result);
            if (requestresponse == null)
            {
                EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                return default;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {

                return requestresponse;
            }

            EventLogger.SaveLog(EventType.Error, $"Api: {_apiName} no respondió satisfactoriamente", requestresponse);
            return default;
        }

    }

    public static class HttpClientHelpers
    {
        public static HttpClient AddHeaders(this HttpClient client, Dictionary<string, string> customHeaders)
        {
            foreach (KeyValuePair<string, string> item in customHeaders)
            {
                client.DefaultRequestHeaders.Add(item.Key, item.Value);
            }
            return client;
        }
    }
}
