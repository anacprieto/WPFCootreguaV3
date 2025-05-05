using Newtonsoft.Json;
using System;

namespace WPFCootreguaV2.ApiService.Models
{
    /// <summary>
    /// Http Generic Response
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        #region Properties
        public int statusCode { get; set; }
        public string message { get; set; }
        public T response { get; set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <param name="response"></param>
        public ApiResponse(int statusCode, string message, T response)
        {
            this.statusCode = statusCode;
            this.message = message;
            this.response = response;
        }

        #endregion Constructor

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        //public class ResponseCootregua
        //{
        //    public EResponseCode ResponseCode { get; set; }
        //    public string ResponseMessage { get; set; }
        //    public object ResponseData { get; set; }
        //}

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
}

