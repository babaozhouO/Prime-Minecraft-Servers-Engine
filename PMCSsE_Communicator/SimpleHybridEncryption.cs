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
        /// 使用 AES-256-GCM 加密。输出格式：[headerLength 保留字节][12B nonce][密文(与明文等长)][16B tag]。
        /// headerLength 由调用方写入传输层长度头，本方法只保留空间。
        /// </summary>
        /// <param name="content">明文</param>
        /// <param name="aes">已协商的 AES 对象（仅使用其 Key）</param>
        /// <param name="headerLength">在开头保留的字节数，留给传输层长度头</param>
        /// <exception cref="CryptographicException">底层加密失败</exception>
        internal static byte[] EncryptWithAES(byte[] content, Aes aes, int headerLength = 4)
        {
            // 单次分配：[头部保留][12B nonce][密文][16B tag]
            int totalLength = headerLength + 12 + content.Length + 16;
            byte[] result = new byte[totalLength];

            // nonce 直接写入 result，零额外分配
            RandomNumberGenerator.Fill(result.AsSpan(headerLength, 12));

            Span<byte> ciphertext = result.AsSpan(headerLength + 12, content.Length);
            Span<byte> tag = result.AsSpan(headerLength + 12 + content.Length, 16);

            using var aesGcm = new AesGcm(aes.Key, tagSizeInBytes: 16);
            aesGcm.Encrypt(
                nonce: result.AsSpan(headerLength, 12),
                plaintext: content,
                ciphertext: ciphertext,
                tag: tag);

            return result;
        }
        /// <summary>
        /// 使用 AES-256-GCM 解密。输入格式：[headerLength 保留字节][12B nonce][密文][16B tag]。
        /// tag 不匹配时抛出 AuthenticationTagMismatchException。
        /// </summary>
        /// <param name="encryptedData">密文（含 nonce + tag）</param>
        /// <param name="aes">已协商的 AES 对象（仅使用其 Key）</param>
        /// <param name="headerLength">输入开头的保留字节数（传输层长度头，传输层已剥离则传 0）</param>
        /// <exception cref="AuthenticationTagMismatchException">认证标签不匹配——密文被篡改</exception>
        /// <exception cref="CryptographicException">数据长度不合法或解密失败</exception>
        internal static byte[] DecryptWithAES(byte[] encryptedData, Aes aes, int headerLength = 0)
        {
            int ciphertextLength = encryptedData.Length - headerLength - 12 - 16;
            if (ciphertextLength < 0)
                throw new CryptographicException("数据包太短，不可能有效");

            ReadOnlySpan<byte> nonce = encryptedData.AsSpan(headerLength, 12);
            ReadOnlySpan<byte> ciphertext = encryptedData.AsSpan(headerLength + 12, ciphertextLength);
            ReadOnlySpan<byte> tag = encryptedData.AsSpan(headerLength + 12 + ciphertextLength, 16);

            // 单次分配
            byte[] plaintext = new byte[ciphertextLength];

            using var aesGcm = new AesGcm(aes.Key, tagSizeInBytes: 16);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            // tag 不匹配时 Decrypt 抛出 AuthenticationTagMismatchException

            return plaintext;
        }

    }
}
