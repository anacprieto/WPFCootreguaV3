using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain;

namespace WPFCootreguaV2.ApiService.QueueModels
{
    public delegate Task<object?> RequestDelegate();
    public class RequestQueue
    {
        private Queue<RequestDelegate> _requests = new Queue<RequestDelegate>();
        private Task? _backgroundQueueTask;

        public void Enqueue(RequestDelegate requestFunc) 
        { 
            lock (_requests)
            {
                _requests.Enqueue(requestFunc);
            }

            if (_backgroundQueueTask == null || _backgroundQueueTask.IsCompleted)
                _backgroundQueueTask = DoQueue();
        }

        private async Task DoQueue()
        {
            
            while (_requests.Count > 0)
            {
                if (!await InternetConnectionManager.IsConnected())
                {
                    await Task.Delay(2000);
                    continue;
                }

                var currentDelegate = _requests.Dequeue();
                object? result = null;
                bool enqueueAgain = false;
                try
                {
                    result = await currentDelegate.Invoke();
                }
                catch(TaskCanceledException ex)
                {
                    if (ex.InnerException == null) 
                    {
                        EventLogger.SaveLog(EventType.Warning, "Una petición en cola a superado el tiempo máximo de expera, se volverá a encolar", ex);
                        enqueueAgain = true;
                    }
                    EventLogger.SaveLog(EventType.Error, $"Ocurrió un error desconocido en una de las peticiones en cola. {ex.Message}", ex);
                }
                catch (HttpRequestException ex) 
                {
                    if (ex.InnerException is SocketException)
                    {
                        EventLogger.SaveLog(EventType.Warning, "Una petición en cola a fallado por fallas de internet, se volverá a encolar", ex);
                        enqueueAgain = true;
                    }
                    EventLogger.SaveLog(EventType.Error, $"Ocurrió un error desconocido en una de las peticiones en cola. {ex.Message}", ex);
                }
                catch(Exception ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"Ocurrió un error desconocido en una de las peticiones en cola. {ex.Message}", ex);
                }
                if (enqueueAgain) _requests.Enqueue(currentDelegate);
            }
        }
    }
}
