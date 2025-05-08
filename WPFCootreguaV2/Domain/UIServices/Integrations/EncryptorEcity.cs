using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace WPFCootreguaV2.Domain.UIServices.Integrations
{
    public static class EncryptorEcity
    {

        public static string Encrypt(string plainText, string key = null)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainText);
                using (var aes = CreateAesProvider(key))
                {
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] inArray = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
                        return Convert.ToBase64String(inArray);
                    }
                }
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
                using (var aes = CreateAesProvider(key))
                {
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        byte[] bytes = decryptor.TransformFinalBlock(array, 0, array.Length);
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Aes CreateAesProvider(string secretKey)
        {
            try
            {
                if (secretKey == null)
                {
                    // En .NET 8, obtenemos el namespace de manera similar pero compatible
                    secretKey = Assembly.GetEntryAssembly()?.EntryPoint?.DeclaringType?.Namespace
                        ?? "Encryptor.Ecity"; // Valor por defecto si no se puede obtener
                }

                byte[] array = new byte[16];
                byte[] bytes = Encoding.UTF8.GetBytes(secretKey);
                Array.Copy(bytes, array, Math.Min(array.Length, bytes.Length));

                Aes aes = Aes.Create();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Key = array;
                aes.IV = array;

                return aes;
            }
            catch (Exception)
            {
                return null;
            }
        }
    
     }
}
