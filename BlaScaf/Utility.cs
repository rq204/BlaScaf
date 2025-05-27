using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace BlaScaf
{
    public class Utility
    {
        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MD5(string str)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            string txt = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", null);
            return txt.ToLower();
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string DESEncrypt(string plainText, string key, string iv)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));

            using (var des = DES.Create())
            {
                des.Key = GetBytes(key, 8);
                des.IV = GetBytes(iv, 8);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string DESDecrypt(string encryptedText, string key, string iv)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));

            using (var des = DES.Create())
            {
                des.Key = GetBytes(key, 8);
                des.IV = GetBytes(iv, 8);
                des.Padding = PaddingMode.PKCS7; // 显式设置

                byte[] cipherBytes = Convert.FromBase64String(encryptedText);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        private static byte[] GetBytes(string input, int requiredLength)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            if (bytes.Length < requiredLength)
                throw new ArgumentException($"输入字符串不足 {requiredLength} 字节", nameof(input));
            return bytes.Length > requiredLength ? SubArray(bytes, requiredLength) : bytes;
        }

        private static byte[] SubArray(byte[] data, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, result, length);
            return result;
        }
    }
}