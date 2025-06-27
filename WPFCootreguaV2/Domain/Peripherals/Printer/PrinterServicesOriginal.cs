using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFCootreguaV2.Domain.Variables;
using WPFCootreguaV2.Domain.UIServices;
using System.IO;

namespace WPFCootreguaV2.Domain.Peripherals.Printer
{
    #region PrintForLines

    public class PrintService
    {
        public static int numberOfSecondsToPrint = 5;
        private static PrintController _printController;
        private static PrintDocument _document;
        private static Graphics _graphics;
        private static PrintProperties _properties;
        public static List<PrintObj> _printData;
        public static bool? recentImpressionSuccess;
        public static Transaction _ts = Transaction.Instance;


        static PrintService()
        {
            _properties = new PrintProperties("", 9600);
            _printController = new StandardPrintController();
            _document = new PrintDocument();
            _document.PrintController = _printController;
            _document.PrintPage += new PrintPageEventHandler(Print);


        }


        public async static Task Start()
        {

            await Task.Run(() =>
            {
                try
                {
                    _document.Print();
                    var wasSucess = MonitorPrintJobs();
                    if (!wasSucess) CleanPrintQueue();
                    recentImpressionSuccess = wasSucess;
                }
                catch (Exception ex)
                {
                    EventLogger.SaveLog(EventType.Error, $"Error en la tarea Start de Impresión: {ex.Message}", ex);
                }

            });

        }

        public static void BuildPrint(Dictionary<string, string> header, Dictionary<string, string> body, Dictionary<string, string> footer)
        {

            SolidBrush color = new SolidBrush(Color.Black);
            Font fontKeys = new Font("Arial Narrow", 10, FontStyle.Bold);
            Font fontValues = new Font("Agency FB", 8, FontStyle.Regular);
            Font fontBrand = new Font("Agency FB", 12, FontStyle.Regular);
            Font fontFooter = new Font("Agency FB", 7, FontStyle.Regular);
            int y = 90;
            int yGap = 25;
            int xValues = 150;
            int xKeys = 15;


            string imgVoucher = Path.Combine(AppConfig.Get("imgVoucher").Replace('/', '\\'));

            var dataToPrint = new List<PrintObj>();

            dataToPrint.Add(new PrintObj { Image = imgVoucher, X = 20, Y = 0 });

            dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = "Recaudo:", X = xKeys + 90, Y = y += 15 });

           dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = $"{_ts.TipoRecaudo}", X = xKeys + 90, Y = y += 20 });

            // Body
            foreach (var key in body.Keys)
            {
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = key, X = xKeys, Y = y += yGap });
                dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = body[key] ?? string.Empty, X = xValues + 30, Y = y });

            }

            //foreach(var key in footer.Keys)
            //{
            //    dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = key, X = xKeys, Y = y += yGap });
            //    dataToPrint.Add(new PrintObj { Brush = color, Font = fontKeys, Text = footer[key] ?? string.Empty, X = xValues + 30, Y = y });

            //}

            _printData = dataToPrint;
        }

        private static void Print(object sender, PrintPageEventArgs e)
        {
            try
            {
                if (_printData.Count <= 0) return;
                if (e.Graphics == null) throw new Exception("La propiedad Graphics del evento Print es nula");

                foreach (var printObj in _printData)
                {
                    _graphics = e.Graphics;

                    if (printObj.QR != null)
                    {
                        _graphics.DrawImage(printObj.QR, printObj.X, printObj.Y);
                    }
                    else if (!string.IsNullOrEmpty(printObj.Image))
                    {
                        _graphics.DrawImage(Image.FromFile(printObj.Image), printObj.X, printObj.Y);
                    }
                    else if (printObj.Point.X != 0 && printObj.Point.Y != 0)
                    {
                        _graphics.DrawString(printObj.Text, printObj.Font, printObj.Brush, printObj.Point, printObj.Direction);
                    }
                    else
                    {
                        _graphics.DrawString(printObj.Text, printObj.Font, printObj.Brush, printObj.X, printObj.Y);
                    }
                }

            }
            catch (Exception ex)
            {
                EventLogger.SaveLog(EventType.Error, $"Ocurrió un error en tiempo de ejecución: {ex.Message}", ex);
            }
        }

        private static bool MonitorPrintJobs()
        {
            PrintServer printServer = new PrintServer();
            PrintQueue printQueue = printServer.GetPrintQueue("w80");
            printQueue.Refresh();
            int printingTime = 0;
            var jobCollections = printQueue.GetPrintJobInfoCollection();
            EventLogger.SaveLog(EventType.Info, $"Se encontraron estos trabajos a imprimir {jobCollections.Count()}");
            if (jobCollections.Count() == 0) return true;

            foreach (PrintSystemJobInfo job in jobCollections)
            {
                printingTime = 0;
                EventLogger.SaveLog(EventType.Info, $"Imprimiendo {job.Name}");
                while (!job.IsPrinted && printingTime < numberOfSecondsToPrint)
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
                if (printingTime == numberOfSecondsToPrint) EventLogger.SaveLog(EventType.Info, $"La impresion tardo mas de {numberOfSecondsToPrint} segundos");

            }
            if (printingTime == numberOfSecondsToPrint) return false;
            return true;

        }
        public static void CleanPrintQueue()
        {
            LocalPrintServer printServer = new LocalPrintServer();
            PrintQueue queue = printServer.GetPrintQueue("w80");
            queue.Refresh();
            var jobCollections = queue.GetPrintJobInfoCollection();
            EventLogger.SaveLog(EventType.Error, $"Se cancelaran {jobCollections.Count()} impresiones en cola.");

            // Retrieve and cancel all print jobs
            foreach (PrintSystemJobInfo printJob in queue.GetPrintJobInfoCollection())
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
            EventLogger.SaveLog(EventType.Error, $"Impresionas en cola canceladas");

        }



    }

    #endregion


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

        int m_iInit = -1;
        int m_iStatus = -1;
        int m_lcLanguage = 0;

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
                StringBuilder sPort = new StringBuilder(portName, portName.Length);
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
            catch (Exception ex)
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
