using Encryptor.Ecity.Dll;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using WPFCootreguaV2.ApiService;
using WPFCootreguaV2.Domain.ApiService.Models;
using WPFCootreguaV2.Domain.Enumerables;

namespace WPFCootreguaV2.Domain.UIServices
{
    public class Utilities
    {
        private static SpeechSynthesizer speechSynthesizer;
        private static Transaction _ts;



        public static string GetConfiguration(string key, bool decodeString = false)
        {
            try
            {
                string value = "";
                AppSettingsReader reader = new AppSettingsReader();
                value = reader.GetValue(key, typeof(String)).ToString();
                if (decodeString)
                {
                    value = EncryptorEcity.Decrypt(value, "WPFCootregua");
                }
                return value;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex, ex.ToString());
                return string.Empty;
            }
        }
        public static void Speak(string text)
        {
            try
            {
                if (GetConfiguration("ActivateSpeak").ToUpper() == "SI")
                {
                    if (speechSynthesizer == null)
                    {
                        speechSynthesizer = new SpeechSynthesizer();
                    }

                    speechSynthesizer.SpeakAsyncCancelAll();
                    speechSynthesizer.SelectVoice("Microsoft Sabina Desktop");
                    speechSynthesizer.SpeakAsync(text);
                }
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex, ex.ToString());
            }
        }
        public static decimal RoundValue(decimal Total, bool arriba)
        {
            try
            {
                decimal roundTotal = 0;

                if (arriba)
                {
                    roundTotal = Math.Ceiling(Total / 100) * 100;
                }
                else
                {
                    roundTotal = Math.Floor(Total / 100) * 100;
                }

                return roundTotal;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex);
                return Total;
            }
        }

        public static bool ValidateModule(decimal module, decimal amount)
        {
            try
            {
                var result = (amount % module);
                return result == 0 ? true : false;
            }
            catch (Exception ex)
            {
                //Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "Utilities", ex);
                return false;
            }
        }


        public static async Task SaveTransaction()
        {
            try
            {
                if (_ts != null)
                {
                    _ts.TotalDevuelta = await ValidateMoney();

                    if (_ts.payer == null)
                    {
                        _ts.payer = new PAYER
                        {
                            IDENTIFICATION = _dataConfiguration.ID_PAYPAD.ToString(),
                            NAME = Utilities.GetConfiguration("NAME_PAYPAD"),
                            LAST_NAME = Utilities.GetConfiguration("LAST_NAME_PAYPAD")
                        };
                    }

                    _ts.payer.PAYER_ID = await SavePayer(_ts.payer);

                    if (_ts.payer.PAYER_ID > 0)
                    {
                        var data = new TRANSACTION
                        {
                            TYPE_TRANSACTION_ID = Convert.ToInt32(_ts.Type),
                            PAYER_ID = _ts.payer.PAYER_ID,
                            STATE_TRANSACTION_ID = Convert.ToInt32(_ts.State),
                            TOTAL_AMOUNT = _ts.Total,
                            DATE_END = DateTime.Now,
                            TRANSACTION_ID = 0,
                            RETURN_AMOUNT = 0,
                            INCOME_AMOUNT = 0,
                            PAYPAD_ID = 0,
                            DATE_BEGIN = DateTime.Now,
                            STATE_NOTIFICATION = 0,
                            STATE = 0,
                            DESCRIPTION = "Transaccion iniciada",
                            TRANSACTION_REFERENCE = ""
                        };

                        if (Utilities.TransactionType != ETransactionType.Registros)
                        {
                            data.TRANSACTION_DESCRIPTION.Add(new TRANSACTION_DESCRIPTION
                            {
                                AMOUNT = transaction.Amount,
                                TRANSACTION_ID = data.ID,
                                TRANSACTION_PRODUCT_ID = (int)ETypeProduct.Existence,
                                DESCRIPTION = string.Concat(transaction.ProductSelect.NameLine, "-", transaction.ProductSelect.ValorPagar),
                                EXTRA_DATA = "",
                                TRANSACTION_DESCRIPTION_ID = 0,
                                STATE = true
                            });
                        }

                        if (data != null)
                        {
                            var responseTransaction = await api.CallApi("SaveTransaction", data);
                            if (responseTransaction != null)
                            {
                                transaction.IdTransactionAPi = JsonConvert.DeserializeObject<int>(responseTransaction.ToString());

                                if (transaction.IdTransactionAPi > 0)
                                {
                                    data.TRANSACTION_ID = transaction.IdTransactionAPi;
                                    transaction.TransactionId = SqliteDataAccess.SaveTransaction(data);
                                }
                            }
                            else
                            {
                                SaveLog(new RequestLog
                                {
                                    Reference = transaction.reference,
                                    Description = string.Concat(MessageResource.NoInsertTransaction, " en su primer intente "),
                                    State = 1,
                                    Date = DateTime.Now
                                }, ELogType.General);

                                responseTransaction = await api.CallApi("SaveTransaction", data);
                                if (responseTransaction != null)
                                {
                                    transaction.IdTransactionAPi = JsonConvert.DeserializeObject<int>(responseTransaction.ToString());

                                    if (transaction.IdTransactionAPi > 0)
                                    {
                                        data.TRANSACTION_ID = transaction.IdTransactionAPi;
                                        transaction.TransactionId = SqliteDataAccess.SaveTransaction(data);
                                    }
                                }
                                else
                                {
                                    SaveLog(new RequestLog
                                    {
                                        Reference = transaction.reference,
                                        Description = string.Concat(MessageResource.NoInsertTransaction, " en su segundo intente "),
                                        State = 1,
                                        Date = DateTime.Now
                                    }, ELogType.General);
                                }
                            }
                        }
                    }
                    else
                    {
                        SaveLog(new RequestLog
                        {
                            Reference = transaction.reference,
                            Description = MessageResource.NoInsertPayment + transaction.payer.IDENTIFICATION,
                            State = 1,
                            Date = DateTime.Now
                        }, ELogType.General);
                    }
                }
            }
            catch (Exception ex)
            {
                Error.SaveLogError(MethodBase.GetCurrentMethod().Name, "InitPaypad", ex, ex.ToString());
            }
        }
    }
}
