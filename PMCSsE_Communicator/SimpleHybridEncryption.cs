using System.Security.Cryptography;

namespace PMCSsE_Communicator
{
    internal static class SimpleHybridEncryption
    {
        internal static (string publicKey, string privateKey) GenerateRSAKey()
        {
            using RSA rsa = RSA.Create(2048);

            byte[] publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
            string publicKey = Convert.ToBase64String(publicKeyBytes);

            byte[] privateKeyBytes = rsa.ExportPkcs8PrivateKey();
            string privateKey = Convert.ToBase64String(privateKeyBytes);

            return (publicKey, privateKey);
        }
        internal static Aes GenerateAes()
        {
            byte[] aesKey = new byte[32]; RandomNumberGenerator.Fill(aesKey);
            byte[] aesIv = new byte[16]; RandomNumberGenerator.Fill(aesIv);
            Aes aes = Aes.Create(); aes.Key = aesKey; aes.IV = aesIv;
            return aes;
        }
        internal static byte[] EncryptWithRSA(byte[] Content, string publicKey)
        {
            try
            {
                using RSA rsa = RSA.Create();
                byte[] publicKeyBytes = Convert.FromBase64String(publicKey);
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
                return rsa.Encrypt(Content, RSAEncryptionPadding.OaepSHA256);
            }
            catch
            {
                Console.WriteLine($"使用RSA加密时发生异常");
                return [];
            }
        }
        internal static byte[] DecryptWithRSA(byte[] encryptedData, string privateKey)
        {
            try
            {
                using RSA rsa = RSA.Create();
                byte[] privateKeyBytes = Convert.FromBase64String(privateKey);
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                return rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="aes"></param>
        /// <param name="headerLength">添加长度头</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CryptographicException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        internal static byte[] EncryptWithAES(byte[] content, Aes aes, int headerLength = 4)
        {
            int finalLength = headerLength + ((content.Length / 16) + 1) * 16;
            try
            {
                byte[] result = new byte[finalLength];
                using MemoryStream ms = new(result);
                ms.Position = headerLength;
                using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(content, 0, content.Length);
                    cs.FlushFinalBlock();
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        internal static byte[] DecryptWithAES(byte[] encryptedData, Aes aes)
        {
            try
            {
                using MemoryStream ms = new(encryptedData);
                using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using MemoryStream result = new();

                cs.CopyTo(result);
                return result.ToArray();

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
