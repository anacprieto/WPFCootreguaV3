using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.Peripherals.Datafono
{
    public class SignalRManager
    {

        private bool ConexionApi;
        private static readonly Lazy<SignalRManager> instance = new Lazy<SignalRManager>(() => new SignalRManager());

        public static SignalRManager Instance => instance.Value;

        public IHubProxy HubProxy { get; private set; }
        private HubConnection hubConnection;

        private SignalRManager()
        {
          
        }

        public async Task<bool> Initialize()
        {
            try
            {
                hubConnection = new HubConnection(AppConfig.Get("LocalHost")); // Reemplaza con tu URL
                HubProxy = hubConnection.CreateHubProxy("DatafonoHub");
                await hubConnection.Start(); // Iniciar la conexión (puedes manejar esto asincrónicamente si lo prefieres)

                if (hubConnection.State == ConnectionState.Connected)
                {
                    ConexionApi = true;


                    return ConexionApi;



                }
                else
                {
                    ConexionApi = false;
                    return ConexionApi;

                }


            }
            catch (Exception ex)
            {
                ConexionApi = false;
                return ConexionApi;
            }


        }


        public void CerrarConexion()
        {
            if (hubConnection != null)
            {
                hubConnection.Stop();
                hubConnection.Dispose();
                hubConnection = null;
            }
        }


    }
}
