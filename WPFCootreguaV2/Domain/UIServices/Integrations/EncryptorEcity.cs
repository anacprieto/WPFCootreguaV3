/*using System;
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
*/
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

namespace WPFCootreguaV2.Domain.UIServices.Integrations
{
    public static class EncryptorEcity
    {
        // Constante para usar exactamente la misma clave que el original
        private const string DEFAULT_KEY = "Cootregua";
        private static bool _enableDebug = true;

        public static string Encrypt(string plainText, string key = null)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                LogDebug("Encrypt: Input plainText is null or empty");
                return null;
            }

            try
            {
                // Obtenemos y validamos el RijndaelManaged con la clave correcta
                RijndaelManaged rijndael = GetRijndaelManaged(key);
                if (rijndael == null)
                {
                    LogDebug("Encrypt: Failed to create RijndaelManaged instance");
                    return null;
                }

                LogDebug($"Encrypt: Using key: {key ?? DEFAULT_KEY}");

                // Convertimos el texto a bytes
                byte[] bytes = Encoding.UTF8.GetBytes(plainText);

                // Realizamos la encriptación exactamente como el original
                byte[] inArray = rijndael.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length);

                // Convertimos a Base64
                string result = Convert.ToBase64String(inArray);

                return result;
            }
            catch (Exception ex)
            {
                LogDebug($"Encrypt: Exception occurred: {ex.GetType().Name} - {ex.Message}");
                return null;
            }
        }

        public static string Decrypt(string encryptedText, string key = null)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                LogDebug("Decrypt: Input encryptedText is null or empty");
                return null;
            }

            try
            {
                // Obtenemos y validamos el RijndaelManaged con la clave correcta
                RijndaelManaged rijndael = GetRijndaelManaged(key);
                if (rijndael == null)
                {
                    LogDebug("Decrypt: Failed to create RijndaelManaged instance");
                    return null;
                }

                // Convertimos de Base64 a bytes
                byte[] array = Convert.FromBase64String(encryptedText);

                // Realizamos la desencriptación exactamente como el original
                byte[] bytes = rijndael.CreateDecryptor().TransformFinalBlock(array, 0, array.Length);

                // Convertimos de bytes a texto
                string result = Encoding.UTF8.GetString(bytes);

                return result;
            }
            catch (Exception ex)
            {
                LogDebug($"Decrypt: Exception occurred: {ex.GetType().Name} - {ex.Message}");
                return null;
            }
        }

        private static RijndaelManaged GetRijndaelManaged(string secretKey)
        {
            try
            {
                // Usamos la clave proporcionada o la clave por defecto
                string effectiveKey = secretKey ?? DEFAULT_KEY;
                LogDebug($"GetRijndaelManaged: Using key: {effectiveKey}");

                // Creamos el array de bytes para la clave
                byte[] keyBytes = new byte[16];
                byte[] sourceBytes = Encoding.UTF8.GetBytes(effectiveKey);

                // Copiamos los bytes exactamente como el original
                Array.Copy(sourceBytes, keyBytes, Math.Min(keyBytes.Length, sourceBytes.Length));
                LogDebug($"GetRijndaelManaged: Key bytes: {BitConverter.ToString(keyBytes)}");

                // Creamos y configuramos RijndaelManaged exactamente como el original
                RijndaelManaged rijndael = new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128,
                    BlockSize = 128,
                    Key = keyBytes,
                    IV = keyBytes // Usando la misma key como IV
                };

                return rijndael;
            }
            catch (Exception ex)
            {
                LogDebug($"GetRijndaelManaged: Exception occurred: {ex.GetType().Name} - {ex.Message}");
                return null;
            }
        }

        private static void LogDebug(string message)
        {
            if (_enableDebug)
            {
                Debug.WriteLine($"[EncryptorEcity] {message}");
                Console.WriteLine($"[EncryptorEcity] {message}");
            }
        }

        // Métodos para probar compatibilidad con el encriptador original

        public static bool TestDecryptOriginal(string originalEncrypted, string expectedPlainText, string key = null)
        {
            try
            {
                string decrypted = Decrypt(originalEncrypted, key);
                bool success = decrypted == expectedPlainText;

                LogDebug($"TestDecryptOriginal: '{originalEncrypted}' -> '{decrypted}'");
                LogDebug($"Expected: '{expectedPlainText}', Success: {success}");

                return success;
            }
            catch (Exception ex)
            {
                LogDebug($"TestDecryptOriginal Exception: {ex.Message}");
                return false;
            }
        }

        public static bool TestEncryptForOriginal(string plainText, string originalEncrypted, string key = null)
        {
            try
            {
                string encrypted = Encrypt(plainText, key);
                bool success = encrypted == originalEncrypted;

                LogDebug($"TestEncryptForOriginal: '{plainText}' -> '{encrypted}'");
                LogDebug($"Original: '{originalEncrypted}', Match: {success}");

                return success;
            }
            catch (Exception ex)
            {
                LogDebug($"TestEncryptForOriginal Exception: {ex.Message}");
                return false;
            }
        }

        public static string ForceFixedKey()
        {
            return DEFAULT_KEY;
        }
    }
}