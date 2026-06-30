using System.Security.Cryptography;
using System.Text;

namespace PMCSsE_Communicator.SharedCodes;

/// <summary>
/// 配置文件加解密工具。使用 AES-256-GCM + PBKDF2 密钥派生。
/// 由 PMCSsE_Backend 和 PMCSsE_Frontend_AvaloniaUI 共用。
/// </summary>
public static class ConfigCrypto
{
    /// <summary>
    /// 从密码 + 盐派生 256 位 AES 密钥。
    /// </summary>
    public static byte[] DeriveKey(byte[] key, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            key,
            salt,
            iterations: 100000,
            HashAlgorithmName.SHA256,
            outputLength: 32);
    }

    /// <summary>
    /// AES-256-GCM 加密。输出格式：[12B nonce][密文][16B tag]。
    /// </summary>
    /// <exception cref="CryptographicException"/>
    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(nonce);

        byte[] result = new byte[12 + plaintext.Length + 16];
        Buffer.BlockCopy(nonce, 0, result, 0, 12);

        using var aesGcm = new AesGcm(key, tagSizeInBytes: 16);
        aesGcm.Encrypt(
            nonce,
            plaintext,
            result.AsSpan(12, plaintext.Length),
            result.AsSpan(12 + plaintext.Length, 16));

        return result;
    }

    /// <summary>
    /// AES-256-GCM 解密。输入格式：[12B nonce][密文][16B tag]。
    /// 认证标签不匹配时抛出 AuthenticationTagMismatchException。
    /// </summary>
    /// <exception cref="AuthenticationTagMismatchException"/>
    /// <exception cref="CryptographicException"/>
    public static byte[] Decrypt(byte[] encrypted, byte[] key)
    {
        int cipherLen = encrypted.Length - 12 - 16;
        if (cipherLen < 0)
            throw new CryptographicException("加密数据太短");

        byte[] plaintext = new byte[cipherLen];

        using var aesGcm = new AesGcm(key, tagSizeInBytes: 16);
        aesGcm.Decrypt(
            encrypted.AsSpan(0, 12),
            encrypted.AsSpan(12, cipherLen),
            encrypted.AsSpan(12 + cipherLen, 16),
            plaintext);

        return plaintext;
    }
}
