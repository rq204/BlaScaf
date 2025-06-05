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
using System.Text.RegularExpressions;
using System.Net;

namespace BlaScaf
{
    /// <summary>
    /// 一些常用的工具类
    /// </summary>
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

        /// <summary>
        /// 验证密码是否符合规则：
        /// 2. 至少包含一个数字
        /// 3. 至少包含一个小写字母
        /// 4. 至少包含一个大写字母
        /// </summary>
        /// <param name="password">待验证的密码</param>
        /// <returns>是否有效</returns>
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");

            return hasDigit && hasLower && hasUpper;
        }

        /// <summary>
        /// 更新不同的值，用于给freesql只更新新字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">源变量</param>
        /// <param name="target">目标变量</param>
        public static void UpdateDifferentProperties<T>(T source, T target)
        {
            var type = typeof(T);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;

                var sourceValue = prop.GetValue(source);
                var targetValue = prop.GetValue(target);

                if (!object.Equals(sourceValue, targetValue))
                {
                    prop.SetValue(target, sourceValue);
                }
            }
        }

        /// <summary>
        /// 检查字符串是否包含危险字符（不处理，只检查）
        /// </summary>
        public static bool ContainsDangerousChars(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return input.Contains("<") ||
                   input.Contains(">") ||
                   input.Contains("\"") ||
                   input.Contains("'") ||
                   input.ToLower().Contains("javascript:") ||
                   input.ToLower().Contains("vbscript:");
        }

        /// <summary>
        /// 得到客户端IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRealClientIP(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            ///如果是本地ip的话检查代理头
            if (IsLocalOrPrivateIp(remoteIp))
            {
                var value = context.Request.Headers[BsSecurity.XForwardedFor].FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    // X-Forwarded-For 可能包含多个 IP，取第一个
                    var ip = value.Split(',')[0].Trim();

                    // 清理 IPv4 映射的 IPv6 地址
                    if (ip.StartsWith("::ffff:"))
                    {
                        ip = ip.Substring(7);
                    }

                    // 验证 IP 格式
                    if (IPAddress.TryParse(ip, out var parsedIP))
                    {
                        return ip;
                    }
                }
            }
            // 回退到连接 IP
            if (remoteIp?.IsIPv4MappedToIPv6 == true)
            {
                return remoteIp.MapToIPv4().ToString();
            }

            return remoteIp?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// 判断是否本地或是代理
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static bool IsLocalOrPrivateIp(IPAddress ip)
        {
            if (IPAddress.IsLoopback(ip))
                return true;

            // 转换成 IPv4（如果是 IPv6-mapped）
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            var bytes = ip.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;

            // Docker 默认 bridge 网络: 172.17.0.0/16 也包含在上面范围中
            return false;
        }
    }
}