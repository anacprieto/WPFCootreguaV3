using System.Collections.Generic;
using WPFCootreguaV2.ApiService.IntegrationModels;
using WPFCootreguaV2.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;
using WPFCootreguaV2.Domain.Integrations;
using WPFCootreguaV2.UserControls;
using WPFCootreguaV2.Domain.ApiService.Models;
using static WPFCootreguaV2.Presentation.UserControls.ListOfObligationsViewModel;
using System;

namespace WPFCootreguaV2.Domain.UIServices
{
    public class Transaction
    {
        // Patron de Diseño Singleton
        public static ETransactionType TransactionType { get; set; }

        private static Transaction? _instance;
        public static Transaction Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Transaction();
                return _instance;
            }
        }

        public static void Reset()
        {
            _instance = null;
        }

        private Transaction() { }

        public TransactionDto ApiDto { get; set; }
        public int IdTransaccionApi { get; set; }

        public Person DataPerson { get; set; }

        public List<ProductsState> DataProducts { get; set; }
        public ProductsState ProductSelect { get; set; }

        public Payer payer { get; set; }

        public int IdPaypad { get; set; } = 3;
        public string? TipoRecaudo { get; set; }
        public TypeTransaction TipoTransaccion { get; set; }
        public TypePayment TipoPago { get; set; }
        public StateTransaction EstadoTransaccion { get; set; }
        public string EstadoTransaccionVerb { get; set; }
        public string? Referencia { get; set; }
        public string? Documento { get; set; }
        public string? Descripcion { get; set; }
        public string? FechaVencimiento { get; set; }
        public decimal TotalSinRedondear { get; set; }
        public decimal Total { get; set; }
        public decimal TotalDevuelta { get; set; }
        public decimal TotalIngresado { get; set; }

        public bool DevueltaCorrecta { get; set; }
        public PaymentViewModel DatosPago { get; set; }
        public string Calificacion { get; set; }
        public IProcedureManager ProcedureManager { get; set; }

        //Integración

        public string TipoConsulta { get; set; }
        public MunicipalDetails DetailsPago { get; set; }
        public List<MunicipalDetails> ListFacture { get; set; }

        public string Token { get; set; }

        public decimal ValueScan { get; set; }

        public string IdTipoPago { get; set; }

        //Datafono

        public RequestDatafonoInfo Datafono { get; set; }


    }

    public class RequestDatafonoInfo
    {

        public string Inicial { get; set; }

        public string Value { get; set; }

        public string PaypadID { get; set; }    

        public string IdTransaccion { get; set; }

    }
    public class Person
    {
        public long CodPerson { get; set; }
        public string Identification { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string FirstLastName { get; set; }
        public string SecondLastName { get; set; }
        public string Adress { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public int CodOficine { get; set; }
    }

    public class AuthenticationBiomety
    {
        public int IdBiometry { get; set; }
        public int TypeReader { get; set; }
        public string Identification { get; set; }
        public long CodSession { get; set; }
        public string Template { get; set; }
        public int Validate { get; set; }
    }

    public class Authentication
    {
        public long CodUsuario { get; set; }
        public string Identification { get; set; }
        public string Password { get; set; }
        public int Validate { get; set; }
    }

    public class PymentProduct
    {

        public int Coduser { get; set; }
        public string Identification { get; set; }
        public int TypeProduct { get; set; }
        public long NumberProduct { get; set; }
        public string Description { get; set; }
        public long ValueToPay { get; set; }
        public DateTime PayDate { get; set; }
        public int codOpe { get; set; }
        public int TipoMovimiento { get; set; }
    }

    public class ProductsState
    {
        public string Identititfy { get; set; }
        public int CodSession { get; set; }
        public object NameLine { get; set; }
        public string NumberProduct { get; set; }
        public object NameProduct { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ProxDate { get; set; }
        public object PaymentMethod { get; set; }
        public decimal Saldo { get; set; }
        public int RetirarProducto { get; set; }
        public decimal Cuota { get; set; }
        public int TipoProducto { get; set; }
        public string img { get; set; }
        public decimal ValorPagar { get; set; }
        public decimal ValorAPagar { get; set; }
        public string ColorState { get; set; }
    }


}

