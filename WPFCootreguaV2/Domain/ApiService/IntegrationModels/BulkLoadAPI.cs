using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.ApiService.IntegrationModels
{

    public class ResponseGeneric
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object ResponseData { get; set; }
    }

    #region SIGAM

    #region ConsultData

    public class RequestConsultData
    {
        public string idComercio { get; set; }
        public string password { get; set; }
        public string idCliente { get; set; }
        public string claveConsulta { get; set; }
    }

    public class ResponseConsultData
    {
        public int CodRespuesta { get; set; }
        public string Descripcion { get; set; }
        public List<Factura> Facturas { get; set; }
    }

    public class Factura
    {
        public int idFactura { get; set; }
        public string concepto { get; set; }
        public float totalFactura { get; set; }
        public float totalIva { get; set; }
        public float saldo { get; set; }
        public DateTime fechaVencimiento { get; set; }
        public string idCatastro { get; set; }
        public string idCliente { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string email { get; set; }
        public object telefono { get; set; }
        public string direccion { get; set; }
        public string urlFactura { get; set; }
        public string matriculaInmobiliaria { get; set; }
        public string tipo { get; set; }
    }

    #endregion

    #region NotifyPay

    public class RequestNotifyPaySigam
    {
        public string idComercio { get; set; }
        public string password { get; set; }
        public string idCliente { get; set; }
        public int idFactura { get; set; }
        public string estadoPago { get; set; }
        public string formaPago { get; set; }
        public int valorTotalPagado { get; set; }
        public DateTime fechaPago { get; set; }
        public int idPago { get; set; }
        public string firma { get; set; }
        public string tipo { get; set; }
    }

    public class ResponseNotifySigam
    {
        public int CodEstado { get; set; }
        public string DescripcionEstado { get; set; }
    }

    #endregion

    #endregion

    public class TokenData
    {
        public string Token { get; set; }
    }


    public class RequestDataPay
    {
        public int ClienteId { get; set; }
        public int NumeroProceso { get; set; }
        public int TipoPago { get; set; }
        public string Token { get; set; }
    }

    public class InvoicePay
    {

        [JsonProperty(PropertyName = "$id")]
        public object id { get; set; }
        public long lngClienteId { get; set; }
        public int lngNumeroProceso { get; set; }
        public long lngCupon { get; set; }
        public int intTipoPago { get; set; }
        public int lngValorPago { get; set; }
        public long lngCodigoIac { get; set; }
        public int intCiclo { get; set; }
        public string strNombre { get; set; }
        public string strFechaFacturacion { get; set; }
        public string strFechaVencimiento { get; set; }
        public string strNumeroFactura { get; set; }
        public int intCodAceptacion { get; set; }
        public string strMensjAceptacion { get; set; }
    }


    public class RequestNotifyPay
    {
        public int lngCupon { get; set; }
        public int intTipoPago { get; set; }
        public int intValor { get; set; }
        public string Token { get; set; }
    }


    public class ResponseNotifyPay
    {
        public string strHora { get; set; }
        public int intCodError { get; set; }
        public string strMensjError { get; set; }
    }



    public class InvoiceCore
    {
        public string BillPayerName { get; set; }
        public string BillPayerDocument { get; set; }
        public string InvoiceId { get; set; }
        public long ValueToPay { get; set; }
        public string PeriodBill { get; set; }
        public string IssueDate { get; set; }
        public string SuspensionDate { get; set; }
    }
    public class Invoice : InvoiceCore
    {
        public long? Id { get; set; }
        public bool? IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
    public class NotifyPay
    {
        public string InvoiceId { get; set; }
        public int PaypadId { get; set; }
    }
}
