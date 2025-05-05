using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFCootreguaV2.Domain;
using WPFCootreguaV2.Domain.Variables;

namespace HantleDispenserAPI
{
    public static class Dispenser
    {
        private static CDMS_Handler _handler { get; set; }
        private static int[]? _quantities { get; set; }
        private static int _valueToDispense { get; set; } = 0;
        public static int DispensedValue { get; set; } = 0;
        public static int CoinsValue { get; set; } = 0;
        public static Dictionary<int, int> RejectData { get; set; } = new();
        public static Dictionary<int, int> DispensedData { get; set; } = new();


        private static bool IsConnected()
        {
            try
            {
                _handler = new CDMS_Handler(AppConfig.Get("dispenserPort"), AppConfig.Get("dispenserDenominations"));
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.P_Dispenser, "El puerto o las denominaciones en App.config no están en el formato correcto", ex);
                return false;
            }

            _handler.Disconnect();
            if (_handler.Connect())
            {
                return true;
            }
            return false;
        }

        public static async Task< bool> Start()
        {
            bool res = false;
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {

                EventLogger.SaveLog(EventType.P_Dispenser, $"Inicializando dispensador...");
                if (!IsConnected())
                {
                    EventLogger.SaveLog(EventType.P_Dispenser, $"Error al connectarse al puerto del dispensador {AppConfig.Get("arduinoPort")}");
                    res = false;
                    return;
                }

                CleanVariable();

                //Verificar que el numero de denominaciones del App.config corresponda al número de baules conectados
                var sensor = _handler.GetSensor();
                if (sensor == null || sensor.ErrorCode == ErrorCDMS.RuntimeError)
                {
                    EventLogger.SaveLog(EventType.P_Dispenser, $"Error al consultar los sensores del dispensador", sensor);
                    res = false;
                    return;
                }

                for ( int i = 0; i<_handler.cassetesValues.Count; i++ )
                {
                    if (!sensor.SensorInfo.CassetteConnected[i])
                    {
                        EventLogger.SaveLog(EventType.P_Dispenser, $"El número de baules conectados es menor a los especificados en el App.config", sensor);
                        res = false;
                        return;
                    }
                }


                var handlerResponse = _handler.Initialize();

                if (!handlerResponse.isSuccess)
                {
                    EventLogger.SaveLog(EventType.P_Dispenser, $"Error, dispensador inicia incorrectamente", handlerResponse);
                    res = false;
                    return;
                }
                EventLogger.SaveLog(EventType.P_Dispenser, $"Dispensador inicia correctamente", handlerResponse);
                res = true;
            });
            return res;
        }
  
        private static void CleanVariable()
        {
            _valueToDispense = 0;
            DispensedValue = 0;
            DispensedData = new();
            RejectData = new();
            foreach (var denom in _handler.cassetesValues)
            {
                DispensedData.Add(denom, 0);
                RejectData.Add(denom, 0);
            }

        }

        public static async Task DispenseAmount(int dispendAmount)
        {
            _valueToDispense = dispendAmount;
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                GoDispend(dispendAmount);
            });
        }

        /* Cuidado, cuando escribi esta función solo había dos personas que sabían como funcionaba, Dios y yo. Ahora solo sabe Dios
         * Esta función es recursiva
         */
        private static void GoDispend( int dispendValue, List<int>? cassetteToIgnore = null)
        {
            if (cassetteToIgnore == null) cassetteToIgnore = new List<int>();
            
            _quantities = CalcReturn(dispendValue, cassetteToIgnore);
            
            EventLogger.SaveLog(EventType.P_Dispenser, $"Enviando dispensación... Cantidades", _quantities);

            var response = _handler.Dispense(_quantities);

            EventLogger.SaveLog(EventType.P_Dispenser, $"Respuesta dispensación", response);
            if (response.ErrorCode == ErrorCDMS.RuntimeError)
            {
                CoinsValue = _valueToDispense - DispensedValue;
                return;
            }

            List<CDMS_DenomInfo> denomsWithMissing = new();
            int MissingValue = 0;

            foreach(var denomInfo in response.DispenseData)
            {
                DispensedValue += denomInfo.Denomination * (denomInfo.DispensedQuantity);

                DispensedData[denomInfo.Denomination] += denomInfo.DispensedQuantity;
                RejectData[denomInfo.Denomination] += denomInfo.OutOfCassetteQuantity - denomInfo.DispensedQuantity;

                if ((denomInfo.RequestedQuantity - denomInfo.DispensedQuantity) != 0)
                {
                    denomsWithMissing.Add(denomInfo);
                    
                }
                MissingValue += denomInfo.Denomination * (denomInfo.RequestedQuantity - denomInfo.DispensedQuantity);

            }

            if (response.isSuccess && MissingValue==0)
            {
                CoinsValue = _valueToDispense - DispensedValue;
                EventLogger.SaveLog(EventType.P_Dispenser, $"Devuelta correcta, valor a devolver en monedas: {CoinsValue}");
                return;
            }


            //Si se confirma que hay un baúl vacio
            if (response.ErrorDescription.EndsWith("JamOrEmpty") && !IsJammed())
            {
                EventLogger.SaveLog(EventType.P_Dispenser, $"Un baúl se quedó sin billetes {response.ErrorDescription}");
                var indexCassetteEmpty = ((int)response.ErrorCode) - 61;
                if (indexCassetteEmpty >= (_handler.cassetesValues.Count() - 1))
                {
                    CoinsValue = _valueToDispense - DispensedValue;
                    EventLogger.SaveLog(EventType.P_Dispenser, $"El último baúl se quedó sin billetes, valor a devolver en monedas: {CoinsValue}");
                    return;
                }
                cassetteToIgnore.Add(indexCassetteEmpty);


                GoDispend(MissingValue, cassetteToIgnore);
                return;

            }

            //Si se presenta un atasco
            if ((response.ErrorDescription.EndsWith("Sensor") || response.ErrorDescription.EndsWith("Jam") || response.ErrorDescription.EndsWith("JamOrEmpty")) && IsJammed())
            {
                EventLogger.SaveLog(EventType.P_Dispenser, $"Se detectó un atasco {response.ErrorDescription}");
                if (!TryEject()) //Intenta Ejectar 3 veces
                {

                    var sensors = _handler.GetSensor();
                    CoinsValue = _valueToDispense - DispensedValue;
                    EventLogger.SaveLog(EventType.P_Dispenser, $"No se pudo desatascar, llamar a Julio. Valor a devolver en monedas: {CoinsValue}. Estado de sensores del dispensador",sensors);
                    return;
                }


                GoDispend(MissingValue, cassetteToIgnore);
                CoinsValue = _valueToDispense - DispensedValue;
                return;
            };

            CoinsValue = _valueToDispense - DispensedValue;
            EventLogger.SaveLog(EventType.P_Dispenser, $"No se logró determinar el estado del dispensador, ocurrió un error no determinado, valor a devolver en monedas: {CoinsValue}", response);

        }
        private static int[] CalcReturn(int valueToDispend, List<int >_cassetteToIgnore)
        {
            
            int [] _quantities = new int[_handler.cassetesValues.Count];
            for (int i = 0; i < _handler.cassetesValues.Count; i++)
            {
                var denominacion = _handler.cassetesValues[i];
                // Si la denominación es -1 es por que se debe ignorar
                if ((valueToDispend < denominacion) || (_cassetteToIgnore.Contains(i)))
                {
                    _quantities[i] = 0;
                    continue;
                }
                _quantities[i] = (int)(valueToDispend / denominacion);
                valueToDispend -= (_quantities[i] * denominacion);
            }

            return _quantities;
        }
        private static bool TryEject()
        {
            bool ejectIsSuccess = false;
            EventLogger.SaveLog(EventType.P_Dispenser, $"Se va a intentar ejectar");
            int tries = 3;
            do
            {
                Thread.Sleep(800);
                EventLogger.SaveLog(EventType.P_Dispenser, $"Intento de Ejección {(3-tries)+1}");

                ejectIsSuccess = _handler.Eject().isSuccess && !IsJammed();
                tries--;
            }
            while (!ejectIsSuccess && tries >= 0);

            return ejectIsSuccess;
        }
        private static bool IsJammed()
        {
            var sensors = _handler.GetSensor();
            
            if (sensors == null || sensors.ErrorCode == ErrorCDMS.RuntimeError) return false;
            if (sensors.SensorInfo == null) return false;

            bool skewJam = false;
            for(int i = 0; i<8; i++)
            {
                skewJam = skewJam || sensors.SensorInfo.CassetteSkew1[i];
                skewJam = skewJam || sensors.SensorInfo.CassetteSkew2[i];
            }

            if (skewJam) return true;

            return (
                sensors.SensorInfo.ScanStart ||
                sensors.SensorInfo.Gate1 || sensors.SensorInfo.Gate2 ||
                sensors.SensorInfo.Exit ||
                sensors.SensorInfo.RejectIn
            );

            
        }

        public static string GetLoadMessage()
        {
            if (!IsConnected())
            {
                return DispenserMessages.COM_NOT_AVAILABLE;
            }
            CDMS_Response? sensorResponse = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                sensorResponse = _handler.GetSensor();
            });

            if (sensorResponse == null ||
                sensorResponse.ErrorCode == ErrorCDMS.RuntimeError ||
                sensorResponse.SensorInfo == null)
            {
                return DispenserMessages.PHYSICAL_CONN_LOST;
            }

            if (sensorResponse.SensorInfo.RejectBoxOpen) return DispenserMessages.REJECT_BOX_OPEN;
            
            for(int i = 0; i < _handler.cassetesValues.Count; i++)
            {
                if (!sensorResponse.SensorInfo.CassetteConnected[i]) return string.Format(DispenserMessages.CASSETTE_DISCONNECTED, i + 1);
                if (sensorResponse.SensorInfo.CassetteDismounted[i]) return string.Format(DispenserMessages.CASSETTE_DISMOUNTED, i + 1);
                if (sensorResponse.SensorInfo.CassetteSkew1[i] ||
                    sensorResponse.SensorInfo.CassetteSkew2[i]) 
                    return string.Format(DispenserMessages.CASSETTE_BAD_LOAD, i + 1);
            }

            if (sensorResponse.SensorInfo.CisOpen) return DispenserMessages.CIS_OPEN;

            if (sensorResponse.SensorInfo.ScanStart ||
                sensorResponse.SensorInfo.Gate1 || sensorResponse.SensorInfo.Gate2 ||
                sensorResponse.SensorInfo.Exit ||
                sensorResponse.SensorInfo.RejectIn )
                return DispenserMessages.JAM;

            return string.Empty;
        }

    }
    

}
public  class ErrorPrint
{
    public  string Code { get; set; } = string.Empty;
    public  string Name { get; set; } = string.Empty;
    public  string Description { get; set; } = string.Empty;
}