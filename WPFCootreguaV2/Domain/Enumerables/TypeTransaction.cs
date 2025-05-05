using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.Enumerables
{
    public enum TypeTransaction
    {
        Retiro = 2,
        Consulta = 1,
        Pago,
        Registro,
        Abono = 8,
        Recarga = 9,
        PagoFactura = 10
    }
}
