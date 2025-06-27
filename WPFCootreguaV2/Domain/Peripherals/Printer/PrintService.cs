using DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.UIServices;
using WPFCootreguaV2.Domain.Variables;

namespace WPFCootreguaV2.Domain.Peripherals
{
    public static class PrintService
    {
        public static int NumberOfSecondsToPrint { get; set; } = 5;
        private static readonly PrintController PrintController = new StandardPrintController();
        private static readonly PrintDocument PrintDocument = new PrintDocument();
        private static Graphics Graphics;
        private static PrintProperties PrintProperties;
        private static List<PrintObj> PrintData;
        public static bool? RecentImpressionSuccess;

        static PrintService()
        {
            PrintProperties = new PrintProperties("", 9600);
            PrintDocument.PrintController = PrintController;
            PrintDocument.PrintPage += PrintPage;
        }

        public static void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    PrintDocument.Print();
                    var wasSuccess = MonitorPrintJobs();
                    if (!wasSuccess) CleanPrintQueue();
                    RecentImpressionSuccess = wasSuccess;
                }
                catch (Exception ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"Error en la tarea Start de Impresión: {ex.Message}", ex);
                }
            });
        }

        public static void BuildPrint(Dictionary<string, string?> header, Dictionary<string, string?> body, Dictionary<string, string?> footer)
        {
            var color = new SolidBrush(Color.Black);
            var fontKeys = new Font("Arial", 8, FontStyle.Bold);
            var fontValues = new Font("Arial", 8, FontStyle.Regular);
            var fontBrand = new Font("Arial", 12, FontStyle.Bold);
            int y = 60;
            const int yGap = 15;
            const int xValues = 150;
            const int xKeys = 15;
            const int xCentered90 = 90;
            const int xCentered70 = 70;
            const int xCentered60 = 60;
            const int xCentered50 = 50;




            string imgVoucher = Path.Combine(AppInfo.APP_DIR, AppConfig.Get("imgVoucher").Replace('/', '\\'));

            var dataToPrint = new List<PrintObj>
            {
                new PrintObj { Image = imgVoucher, X = 0, Y = 0 },
                //new PrintObj { Brush = color, Font = fontKeys, Text = "========================================", X = xKeys, Y = y += yGap }

            };
            // Información de encabezado fija
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "Comprobante de transacción", X = xCentered70, Y = y += yGap });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "SEDE ADMINISTRATIVA", X = xCentered70, Y = y += yGap });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "Calle 10 20-54, Barrio La Esperanza", X = xCentered50, Y = y += yGap });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "San josé del Guaviare, Guaviare", X = xCentered60, Y = y += yGap });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "CEL. 315 8404476", X = xCentered90, Y = y += yGap });

            // Línea separadora después del encabezado fijo
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = "========================================", X = xKeys, Y = y += yGap });



            // Header
            foreach (var key in header.Keys)
            {
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = key, X = xKeys, Y = y += yGap });
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = header[key] ?? string.Empty, X = xValues, Y = y });
            }

            dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = "========================================", X = xKeys, Y = y += yGap });

            // Body
            foreach (var key in body.Keys)
            {
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = key, X = xKeys, Y = y += yGap });
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = body[key] ?? string.Empty, X = xValues, Y = y });
            }

            dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = "========================================", X = xKeys, Y = y += yGap });

            // Footer
            foreach (var key in footer.Keys)
            {
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = key, X = xKeys, Y = y += yGap });
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = footer[key] ?? string.Empty, X = xValues, Y = y });
            }

            dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = "========================================", X = xKeys, Y = y += yGap });


            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "Recuerde siempre esperar la tirilla de soporte de su", X = xKeys, Y = y += yGap + 10 });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontValues, Text = "pago, es el único documento que lo respalda.", X = xKeys, Y = y += 20 });
            dataToPrint.Add(new PrintObj { Brush = color, Font = fontBrand, Text = "E-city Software", X = 80, Y = y += yGap + 10 });

            PrintData = dataToPrint;
        }

        private static void PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (PrintData.Count <= 0) return;
                if (e.Graphics == null) throw new Exception("La propiedad Graphics del evento Print es nula");

                Graphics = e.Graphics;

                foreach (var printObj in PrintData)
                {
                    if (printObj.QR != null)
                    {
                        Graphics.DrawImage(printObj.QR, printObj.X, printObj.Y);
                    }
                    else if (!string.IsNullOrEmpty(printObj.Image))
                    {
                        Graphics.DrawImage(Image.FromFile(printObj.Image), printObj.X, printObj.Y);
                    }
                    else if (printObj.Point.X != 0 && printObj.Point.Y != 0)
                    {
                        Graphics.DrawString(printObj.Text, printObj.Font, printObj.Brush, printObj.Point, printObj.Direction);
                    }
                    else
                    {
                        Graphics.DrawString(printObj.Text, printObj.Font, printObj.Brush, printObj.X, printObj.Y);
                    }
                }

                // Cut the paper after printing
                PrintProperties.PrintCutpaper(1);
            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }

        private static bool MonitorPrintJobs()
        {
            var printServer = new PrintServer();
            var printQueue = printServer.GetPrintQueue("w80");
            printQueue.Refresh();
            int printingTime = 0;
            var jobCollections = printQueue.GetPrintJobInfoCollection();
            EventLogger.SaveLog(EventType.Info, $"Se encontraron estos trabajos a imprimir {jobCollections.Count()}");
            if (jobCollections.Count() == 0) return true;

            foreach (PrintSystemJobInfo job in jobCollections)
            {
                printingTime = 0;
                EventLogger.SaveLog(EventType.Info, $"Imprimiendo {job.Name}");
                while (!job.IsPrinted && printingTime < NumberOfSecondsToPrint)
                {
                    Thread.Sleep(1000);
                    job.Refresh();
                    if (job.IsPrinted || job.IsCompleted || job.IsDeleted)
                    {
                        EventLogger.SaveLog(EventType.Info, $"La impresion se realizo con exito");
                        break;
                    }

                    printingTime += 1;
                }
                if (printingTime == NumberOfSecondsToPrint) EventLogger.SaveLog(EventType.Info, $"La impresion tardo mas de {NumberOfSecondsToPrint} segundos");
            }
            return printingTime != NumberOfSecondsToPrint;
        }

        public static void CleanPrintQueue()
        {
            var printServer = new LocalPrintServer();
            var queue = printServer.GetPrintQueue("w80");
            queue.Refresh();
            var jobCollections = queue.GetPrintJobInfoCollection();
            EventLogger.SaveLog(EventType.Error, $"Se cancelaran {jobCollections.Count()} impresiones en cola.");

            // Retrieve and cancel all print jobs
            foreach (PrintSystemJobInfo printJob in jobCollections)
            {
                try
                {
                    EventLogger.SaveLog(EventType.Error, $" {printJob.Name} cancelada");
                    printJob.Cancel();
                }
                catch (Exception ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
                }
            }
            EventLogger.SaveLog(EventType.Error, $"Impresiones en cola canceladas");
        }
    }

    public class PrintObj
    {
        public string Text { get; set; }
        public string Image { get; set; }
        public Image QR { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Font Font { get; set; }
        public SolidBrush Brush { get; set; }
        public PointF Point { get; set; }
        public StringFormat Direction { get; set; }
    }

    internal class PrintProperties
    {
        [DllImport("kernel32.dll", EntryPoint = "GetSystemDefaultLCID", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSystemDefaultLCID();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetInit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetInit();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetClean", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetClean();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetClose", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetClose();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetAlignment", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetAlignment(int iAlignment);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetBold", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetBold(int iBold);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetCommmandmode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetCommmandmode(int iMode);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetLinespace", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetLinespace(int iLinespace);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetPrintport", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetPrintport(StringBuilder strPort, int iBaudrate);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintString", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintString(StringBuilder strData, int iImme);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintSelfcheck", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintSelfcheck();

        [DllImport("Msprintsdk.dll", EntryPoint = "GetStatus", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetStatus();

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintFeedline", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintFeedline(int iLine);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintCutpaper", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintCutpaper(int iMode);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetSizetext", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetSizetext(int iHeight, int iWidth);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetSizechinese", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetSizechinese(int iHeight, int iWidth, int iUnderline, int iChinesetype);

        [DllImport("Msprintsdk.dll", EntryPoint = "SetItalic", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetItalic(int iItalic);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintDiskbmpfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintDiskbmpfile(StringBuilder strData);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintDiskimgfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintDiskimgfile(StringBuilder strData);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintQrcode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintQrcode(StringBuilder strData, int iLmargin, int iMside, int iRound);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintRemainQR", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintRemainQR();

        [DllImport("Msprintsdk.dll", EntryPoint = "SetLeftmargin", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int SetLeftmargin(int iLmargin);

        [DllImport("Msprintsdk.dll", EntryPoint = "GetProductinformation", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetProductinformation(int Fstype, StringBuilder FIDdata);

        [DllImport("Msprintsdk.dll", EntryPoint = "PrintTransmit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int PrintTransmit(byte[] strCmd, int iLength);

        [DllImport("Msprintsdk.dll", EntryPoint = "GetTransmit", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe int GetTransmit(string strCmd, int iLength, StringBuilder strRecv, int iRelen);

        private int m_iInit = -1;
        private int m_iStatus = -1;
        private int m_lcLanguage = 0;

        public PrintProperties(string portName, int baudrate)
        {
            ConfigurationPrinter(portName, baudrate);
        }

        private bool ConfigurationPrinter(string portName, int baudrate)
        {
            try
            {
                int countIntent = 0;
                m_lcLanguage = GetSystemDefaultLCID();
                var sPort = new StringBuilder(portName, portName.Length);
                int iBaudrate = baudrate;
                SetPrintport(sPort, iBaudrate);
                while (countIntent < 3)
                {
                    m_iInit = SetInit();
                    if (m_iInit == 0)
                    {
                        return true;
                    }
                    else
                    {
                        countIntent++;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ClosePrint()
        {
            SetClose();
        }
    }
}
