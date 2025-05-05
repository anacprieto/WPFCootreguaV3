using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.ApiService.IntegrationModels
{
    public class HttpResponse<T>
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
        public HttpResponse(int statusCode, string message, T response)
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
    }
}
