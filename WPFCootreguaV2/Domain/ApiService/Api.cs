using ControlzEx.Standard;
using DB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService.Models;
using WPFCootreguaV2.ApiService.QueueModels;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.UIServices;

namespace WPFCootreguaV2.ApiService
{
    public static class Api
    {
        private static string _baseAddress;
        private static string _keyId;
        private static HttpClient _client;
        private static string? _token;
        private static RequestQueue _requestsQueue;

        static Api()
        {
            _baseAddress = AppConfig.Get("apiBaseAddress");
            _keyId = AppConfig.Get("apiKeyId");
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_baseAddress);
            _client.DefaultRequestHeaders.Add("DashboardKeyId", _keyId);
            _client.Timeout = TimeSpan.FromMilliseconds(10000);
            _requestsQueue = new RequestQueue();
        }

        public static async Task<bool> Login()
        {
            
            var oCredentials = new LoginDto
            {
                userName = AppConfig.Get("username"),
                password = AppConfig.Get("pwd")
            };

            string credentials = JsonConvert.SerializeObject(oCredentials);

            var content = new StringContent(credentials, Encoding.UTF8, "Application/json");
            var endpoint = AppConfig.Get("Login");

            var response = await _client.PostAsync(endpoint, content);

            var result = await response.Content.ReadAsStringAsync();
            if (result == null)
            {
                EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                return false;
                
            }

            var requestresponse = JsonConvert.DeserializeObject<ApiResponse<string>>(result);
            if (requestresponse == null)
            {
                EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                return false;
            }

            if (requestresponse.statusCode == 200)
            {
                _token = requestresponse.response;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                return true;
            }

            EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
            return false;

        }

        public static async Task<bool> Validate()
        {
            var endpoint = AppConfig.Get("Validate");

            var response = await _client.GetAsync(endpoint);

            var result = await response.Content.ReadAsStringAsync();
            if (result == null)
            {
                EventLogger.SaveLog(EventType.Error, "No se obtuvo respuesta del servicio");
                return false;
                
            }

            var requestresponse = JsonConvert.DeserializeObject<ApiResponse<bool>>(result);
            if (requestresponse == null)
            {
                EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                return false;
            }

            if (requestresponse.statusCode == 200)
            {
                return requestresponse.response;
            }

            EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
            return false;
            
        }
        public static async Task<Payer?> CreatePayer()
        {
            var ts = Transaction.Instance;

            var payerToCreate = new Payer
            {
                Document = ts.Documento,
                DocumentType = ts.payer.DocumentType,
                Name = ts.payer.Name,
                LastName = ts.payer.LastName,
                Phone = ts.payer.Phone,
                Email = ts.payer.Email,
                IdTransaction = ts.payer.IdTransaction,
                IdClient = ts.payer.IdClient,
                IdPayPad = ts.payer.IdPayPad
            };

            int tries = 2;
            while (tries > 0)
            {
                tries--;
                try
                {

                    string payload = JsonConvert.SerializeObject(payerToCreate);

                    var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                    var url = AppConfig.Get("Payer");

                    EventLogger.SaveLog(EventType.Info, "Petición: Creación de pagador Dashboard", payerToCreate);

                    var response = await _client.PostAsync(url, content);

                    var result = await response.Content.ReadAsStringAsync();
                    if (result == null)
                    {
                        EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                        continue;
                    }

                    var requestresponse = JsonConvert.DeserializeObject<ApiResponse<Payer>>(result);
                    if (requestresponse == null)
                    {
                        EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                        continue;
                    }

                    if (requestresponse.statusCode == 200)
                    {
                        EventLogger.SaveLog(EventType.Info, "Respuesta: Creación de Payer Dashboard", requestresponse);
                        return requestresponse.response;
                    }

                    EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    EventLogger.SaveLog(EventType.Error, $"Timeout al crear payer (intento {3 - tries}): {ex.Message}");
                    if (tries == 0)
                    {
                        // Último intento fallido
                        throw new Exception("La operación excedió el tiempo límite después de varios intentos");
                    }
                    // Esperar un poco antes del siguiente intento
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"Error al crear pager: {ex.Message}");
                    throw;
                }
            }
            return null;
        }
        //public static async Task<Payer?> CreatePayer()
        //{
        //    var ts = Transaction.Instance;

        //    var payerToCreate = new Payer
        //        {
        //            DOCUMENT = ts.payer.DOCUMENT, // This will be set by the API
        //            DOCUMENTTYPE = ts.payer.DOCUMENTTYPE,
        //            NAME = ts.payer.NAME,
        //            LASTNAME = ts.payer.LASTNAME,
        //            NACIONALITY= ts.payer.NACIONALITY,
        //            PHONE = ts.payer.PHONE,
        //            EMAIL = ts.payer.EMAIL,
        //            DEPARTAMENT = ts.payer.DEPARTAMENT,
        //            MUNICIPALITY = ts.payer.MUNICIPALITY,
        //            IDTRANSACTION = ts.payer.IDTRANSACTION,
        //            IDCLIENT = ts.payer.IDCLIENT,
        //            IDPAYPAD = ts.payer.IDPAYPAD
        //    };
        //    int tries = 2;
        //    while (tries > 0)
        //    {
        //        tries--;

        //        string payload = JsonConvert.SerializeObject(payerToCreate);

        //        var content = new StringContent(payload, Encoding.UTF8, "Application/json");
        //        var url = AppConfig.Get("Payer");

        //        EventLogger.SaveLog(EventType.Info, "Petición: Creación de payer Dashboard", payerToCreate);
        //        var response = await _client.PostAsync(url, content);

        //        var result = await response.Content.ReadAsStringAsync();
        //        if (result == null)
        //        {
        //            EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
        //            continue;
        //        }

        //        var requestresponse = JsonConvert.DeserializeObject<ApiResponse<Payer>>(result);
        //        if (requestresponse == null)
        //        {
        //            EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
        //            continue;
        //        }

        //        if (requestresponse.statusCode == 200)
        //        {
        //            var transactionCreated = requestresponse.response;

        //            // Se guarda la transaccion en la base de datos local
        //            var mapper = ObjMapper.Instance;

        //            EventLogger.SaveLog(EventType.Info, "Respuesta: Creación de Payer Dashboard", requestresponse);
        //            return requestresponse.response;
        //        }

        //        EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
        //    }

        //    return null;
        //}
        public static async Task<TransactionDto?> CreateTransaction()
        {
            var ts = Transaction.Instance;


            ts.EstadoTransaccion = StateTransaction.Iniciada;

            var transactionToCreate = new TransactionDto();


            if (ts.Type== ETransactionType.Registros)
            {
                 transactionToCreate = new TransactionDto
                {
                    Document = ts.Documento,
                    Reference = "100",//
                    Product = "Certificado de Registro mercantil,Registro mercantíl del establecimiento",
                    TotalAmount =0,
                    RealAmount = 0,
                    IncomeAmount = 0,
                    ReturnAmount = 0,
                    Description = ts.Descripcion ?? string.Empty,
                    IdStateTransaction = (int)ts.EstadoTransaccion,
                    StateTransaction = ts.EstadoTransaccion.ToString(),
                    IdTypeTransaction = (int)ts.TipoTransaccion,
                    IdTypePayment = (int)ts.TipoPago,//
                };
            }else
            {
                transactionToCreate = new TransactionDto
                {
                    Document = ts.Documento,
                    Reference = ts.ProductSelect.NumberProduct,//
                    Product = ts.ProductSelect.NameLine.ToString(),
                    TotalAmount = Convert.ToDouble(ts.Total),
                    RealAmount = Convert.ToDouble(ts.TotalSinRedondear),
                    IncomeAmount = 0,
                    ReturnAmount = 0,
                    Description = ts.Descripcion ?? string.Empty,
                    IdStateTransaction = (int)ts.EstadoTransaccion,
                    StateTransaction = ts.EstadoTransaccion.ToString(),
                    IdTypeTransaction = (int)ts.TipoTransaccion,
                    IdTypePayment = (int)ts.TipoPago,//
                };
            }

            

            int tries = 2;
            while (tries > 0)
            {
                tries--;

                string payload = JsonConvert.SerializeObject(transactionToCreate);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("Transaction");

                EventLogger.SaveLog(EventType.Info, "Petición: Creación de transacción Dashboard", transactionToCreate);
                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    continue;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<TransactionDto>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    continue;
                }

                if (requestresponse.statusCode == 200)
                {
                    var transactionCreated = requestresponse.response;
                    ts.ApiDto = transactionCreated;
                    ts.IdTransaccionApi = transactionCreated.Id;

                    // Se guarda la transaccion en la base de datos local
                    var mapper = ObjMapper.Instance;
                    if (!await DB_TransactionService.Create(mapper.Map<DB_Transaction>(transactionCreated)))
                    {
                        EventLogger.SaveLog(EventType.Error, "No se pudo crear la transacción de manera local");
                    }
                    EventLogger.SaveLog(EventType.Info, "Respuesta: Creación de transacción Dashboard", requestresponse);
                    return requestresponse.response;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
            }

            return null;

        }

        //public static void UpdateTransaction()
        //{
        //    var ts = Transaction.Instance;


        //    var transactionToUpdate = ts.ApiDto;

        //    if((int)ts.EstadoTransaccion == null)
        //    {

        //        EventLogger.SaveLog(EventType.Error, "No se pudo obtener el estado");
        //        return;
        //    }
        //    transactionToUpdate.IdStateTransaction = (int)ts.EstadoTransaccion;
        //    transactionToUpdate.Description = ts.Descripcion;
        //    transactionToUpdate.IncomeAmount = (double)ts.TotalIngresado;
        //    transactionToUpdate.ReturnAmount = (double)ts.TotalDevuelta;
        //    transactionToUpdate.Document = ts.Documento;


           

        //    _requestsQueue.Enqueue(async () =>
        //    {
        //        string payload = JsonConvert.SerializeObject(transactionToUpdate);

        //        var content = new StringContent(payload, Encoding.UTF8, "Application/json");
        //        var url = AppConfig.Get("Transaction");

        //        EventLogger.SaveLog(EventType.Info, "Petición: Actualización de transacción Dashboard", transactionToUpdate);
        //        var response = await _client.PutAsync(url, content);

        //        var result = await response.Content.ReadAsStringAsync();
        //        if (result == null)
        //        {
        //            EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
        //            return null;
        //        }

        //        var requestresponse = JsonConvert.DeserializeObject<ApiResponse<TransactionDto>>(result);
        //        if (requestresponse == null)
        //        {
        //            EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
        //            return null;
        //        }

        //        if (requestresponse.statusCode == 200)
        //        {
        //            var transactionUpdated = requestresponse.response;
        //            ts.ApiDto = transactionUpdated;
        //            ts.IdTransaccionApi = transactionUpdated.Id;
        //            // Se guarda la transaccion en la base de datos local
        //            var mapper = ObjMapper.Instance;
        //            if (!await DB_TransactionService.Update(mapper.Map<DB_Transaction>(transactionUpdated)))
        //            {
        //                EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local");
        //            }
        //            EventLogger.SaveLog(EventType.Info, "Respuesta: Actualización de transacción Dashboard", requestresponse);
        //            return requestresponse.response;
        //        }

        //        EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
        //        return null;
        //    });

        //}
        public static void UpdateTransaction()
        {
            try
            {
                // Validación 1: Verificar que Transaction.Instance no sea null
                var ts = Transaction.Instance;
                if (ts == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Transaction.Instance es null");
                    return;
                }

                // Validación 2: Verificar que ApiDto no sea null
                var transactionToUpdate = ts.ApiDto;
                if (transactionToUpdate == null)
                {
                    EventLogger.SaveLog(EventType.Error, "ts.ApiDto es null - no se puede actualizar la transacción");
                    return;
                }

                // Validación 3: Verificar EstadoTransaccion correctamente
                // EstadoTransaccion es un enum, no puede ser null directamente
                // Pero si es nullable enum, usar HasValue
                if (!Enum.IsDefined(typeof(StateTransaction), ts.EstadoTransaccion))
                {
                    EventLogger.SaveLog(EventType.Error, $"Estado de transacción inválido: {ts.EstadoTransaccion}");
                    return;
                }

                // Asignar valores con validaciones adicionales
                transactionToUpdate.IdStateTransaction = (int)ts.EstadoTransaccion;
                transactionToUpdate.Description = ts.Descripcion ?? string.Empty;
                transactionToUpdate.IncomeAmount = (double)ts.TotalIngresado;
                transactionToUpdate.ReturnAmount = (double)ts.TotalDevuelta;
                transactionToUpdate.Document = ts.Documento;

                // Validación 4: Verificar que _requestsQueue no sea null
                if (_requestsQueue == null)
                {
                    EventLogger.SaveLog(EventType.Error, "_requestsQueue es null");
                    return;
                }

                _requestsQueue.Enqueue(async () =>
                {
                    try
                    {
                        string payload = JsonConvert.SerializeObject(transactionToUpdate);
                        var content = new StringContent(payload, Encoding.UTF8, "Application/json");

                        var url = AppConfig.Get("Transaction");
                        if (string.IsNullOrEmpty(url))
                        {
                            EventLogger.SaveLog(EventType.Error, "URL de Transaction no configurada");
                            return null;
                        }

                        EventLogger.SaveLog(EventType.Info, "Petición: Actualización de transacción Dashboard", transactionToUpdate);

                        // Validación 5: Verificar que _client no sea null
                        if (_client == null)
                        {
                            EventLogger.SaveLog(EventType.Error, "HttpClient (_client) es null");
                            return null;
                        }

                        var response = await _client.PutAsync(url, content);

                        if (response == null)
                        {
                            EventLogger.SaveLog(EventType.Error, "Response de la API es null");
                            return null;
                        }

                        var result = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(result))
                        {
                            EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                            return null;
                        }

                        var requestresponse = JsonConvert.DeserializeObject<ApiResponse<TransactionDto>>(result);

                        if (requestresponse == null)
                        {
                            EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                            return null;
                        }
                        if (requestresponse.statusCode == 200)
                        {
                            var transactionUpdated = requestresponse.response;
                            ts.ApiDto = transactionUpdated;
                            ts.IdTransaccionApi = transactionUpdated.Id;
                            // Se guarda la transaccion en la base de datos local
                            var mapper = ObjMapper.Instance;
                            if (!await DB_TransactionService.Update(mapper.Map<DB_Transaction>(transactionUpdated)))
                            {
                                EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local");
                            }
                            EventLogger.SaveLog(EventType.Info, "Respuesta: Actualización de transacción Dashboard", requestresponse);
                            return requestresponse.response;
                        }
                        else
                        {
                            EventLogger.SaveLog(EventType.Error, $"Api respondió con código {requestresponse.statusCode}", requestresponse);
                            return null;
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        EventLogger.SaveLog(EventType.Error, $"Error de red en UpdateTransaction: {httpEx.Message}");
                        return null;
                    }
                    catch (JsonException jsonEx)
                    {
                        EventLogger.SaveLog(EventType.Error, $"Error de JSON en UpdateTransaction: {jsonEx.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.SaveLog(EventType.Error, $"Error inesperado en UpdateTransaction (async): {ex.Message}");
                        return null;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Error inesperado en UpdateTransaction: {ex.Message} - StackTrace: {ex.StackTrace}");
            }
        }

        /*
        public static void CreateTransactionDetail(TypeOperation op, int value)
        {
            int quantity = 1;
            var detail = new TransactionDetailDto
            {
                IdTransaction = Transaction.Instance.IdTransaccionApi,
                CurrencyDenomination = value,
                IdTypeOperation = (int)op,
                Quantity = quantity

            };



            _requestsQueue.Enqueue(async () =>
            {
                string payload = JsonConvert.SerializeObject(detail);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("TransactionDetails");

                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    return null;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<TransactionDetailDto>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    return null;
                }

                if (requestresponse.statusCode == 200)
                {
                    var transactionDetailCreated = requestresponse.response;
                    // Se guarda el detalle en la base de datos local
                    var mapper = ObjMapper.Instance;
                    if (!await DB_TransactionService.CreateDetail(mapper.Map<DB_TransactionDetail>(transactionDetailCreated)))
                    {
                        EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local");
                    }
                    return transactionDetailCreated;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                return null;
            });
        }*/
        public static void CreateTransactionDetail1(int idTransactionApi, TypeOperation op, int value, int quantity)
        {
            var detail = new TransactionDetailDto
            {
                IdTransaction = idTransactionApi,
                CurrencyDenomination = value,
                IdTypeOperation = (int)op,
                Quantity = quantity
            };



            _requestsQueue.Enqueue(async () =>
            {
                string payload = JsonConvert.SerializeObject(detail);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("TransactionDetails");

                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    return null;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<List<TransactionDetailDto>>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    return null;
                }

                if (requestresponse.statusCode == 200)
                {
                    var listTransactionsCreated = requestresponse.response;
                    // Se guarda el detalle en la base de datos local
                    var mapper = ObjMapper.Instance;
                    foreach (var transactionDetailCreated in listTransactionsCreated)
                    {
                        if (!await DB_TransactionService.CreateDetail(mapper.Map<DB_TransactionDetail>(transactionDetailCreated)))
                        {
                            EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local", transactionDetailCreated);
                        }
                    }
                    return listTransactionsCreated;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                return null;
            });
        }


        public static void CreateTransactionDetail(TypeOperation op, int value)
        {
            var detail = new TransactionDetailDto
            {
                IdTransaction = Transaction.Instance.IdTransaccionApi,
                CurrencyDenomination = value,
                IdTypeOperation = (int)op
            };



            _requestsQueue.Enqueue(async () =>
            {
                string payload = JsonConvert.SerializeObject(detail);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("TransactionDetails");

                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    return null;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<TransactionDetailDto>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    return null;
                }

                if (requestresponse.statusCode == 200)
                {
                    var transactionDetailCreated = requestresponse.response;
                    // Se guarda el detalle en la base de datos local
                    var mapper = ObjMapper.Instance;
                    if (!await DB_TransactionService.CreateDetail(mapper.Map<DB_TransactionDetail>(transactionDetailCreated)))
                    {
                        EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local");
                    }
                    return transactionDetailCreated;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                return null;
            });
        }

        public static void CreateTransactionDetail1(TypeOperation op, int value, int quantity)
        {
            var detail = new TransactionDetailDto
            {
                CurrencyDenomination = value,
                IdTypeOperation = (int)op,
                Quantity = quantity
            };



            _requestsQueue.Enqueue(async () =>
            {
                string payload = JsonConvert.SerializeObject(detail);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("TransactionDetails");

                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    return null;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<List<TransactionDetailDto>>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    return null;
                }

                if (requestresponse.statusCode == 200)
                {
                    var listTransactionsCreated = requestresponse.response;
                    // Se guarda el detalle en la base de datos local
                    var mapper = ObjMapper.Instance;
                    foreach (var transactionDetailCreated in listTransactionsCreated)
                    {
                        if (!await DB_TransactionService.CreateDetail(mapper.Map<DB_TransactionDetail>(transactionDetailCreated)))
                        {
                            EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local", transactionDetailCreated);
                        }
                    }
                    return listTransactionsCreated;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                return null;
            });
        }
        public static void CreateTransactionDetail2(TypeOperation op, int value)
        {
            var detail = new TransactionDetailDto
            {
                CurrencyDenomination = value,
                IdTypeOperation = (int)op
            };



            _requestsQueue.Enqueue(async () =>
            {
                string payload = JsonConvert.SerializeObject(detail);

                var content = new StringContent(payload, Encoding.UTF8, "Application/json");
                var url = AppConfig.Get("TransactionDetails");

                var response = await _client.PostAsync(url, content);

                var result = await response.Content.ReadAsStringAsync();
                if (result == null)
                {
                    EventLogger.SaveLog(EventType.Error, "No se obtuvo contenido de la api");
                    return null;
                }

                var requestresponse = JsonConvert.DeserializeObject<ApiResponse<List<TransactionDetailDto>>>(result);
                if (requestresponse == null)
                {
                    EventLogger.SaveLog(EventType.Error, "Error deserializando la respuesta");
                    return null;
                }

                if (requestresponse.statusCode == 200)
                {
                    var listTransactionsCreated = requestresponse.response;
                    // Se guarda el detalle en la base de datos local
                    var mapper = ObjMapper.Instance;
                    foreach (var transactionDetailCreated in listTransactionsCreated)
                    {
                        if (!await DB_TransactionService.CreateDetail(mapper.Map<DB_TransactionDetail>(transactionDetailCreated)))
                        {
                            EventLogger.SaveLog(EventType.Error, "No se pudo actualizar la transacción de manera local", transactionDetailCreated);
                        }
                    }
                    return listTransactionsCreated;
                }

                EventLogger.SaveLog(EventType.Error, "Api no respondió satisfactoriamente", requestresponse);
                return null;
            });
        }
    }
}
