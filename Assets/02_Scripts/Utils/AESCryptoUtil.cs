using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class AESCryptoUtil
{
    // 32바이트(256비트) 암호화 키 & 16바이트 IV (프로젝트 고유 32자/16자 문자열 설정)
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("OzProjectDemonKingHeroSecretKey32");
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("OzProjectInitIV16");

    /// <summary>
    /// 평문 JSON 문자열을 AES로 암호화하여 Base64 문자열로 반환
    /// </summary>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AESCryptoUtil] 암호화 실패: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 암호화된 Base64 문자열을 복호화하여 평문 JSON으로 반환
    /// </summary>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs, Encoding.UTF8))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AESCryptoUtil] 복호화 실패 (데이터 위변조 의심): {ex.Message}");
            return null;
        }
    }
}
