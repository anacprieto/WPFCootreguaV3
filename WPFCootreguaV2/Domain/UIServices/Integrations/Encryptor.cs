using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.UIServices.Integrations
{
    public static class Encryptor
    {
        public static string Encrypt(string plainText, string key = null)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainText);
                byte[] inArray = GetRijndaelManaged(key).CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);
                return Convert.ToBase64String(inArray);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Decrypt(string encryptedText, string key = null)
        {
            try
            {
                byte[] array = Convert.FromBase64String(encryptedText);
                byte[] bytes = GetRijndaelManaged(key).CreateDecryptor().TransformFinalBlock(array, 0, array.Length);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static RijndaelManaged GetRijndaelManaged(string secretKey)
        {
            try
            {
                if (secretKey == null)
                {
                    // Si no se proporciona una clave, usa el namespace de la clase que contiene el punto de entrada
                    secretKey = Assembly.GetExecutingAssembly().EntryPoint.DeclaringType.Namespace;
                }

                byte[] array = new byte[16];
                byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
                Array.Copy(bytes, array, Math.Min(array.Length, bytes.Length));

                return new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128,
                    BlockSize = 128,
                    Key = array,
                    IV = array
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
