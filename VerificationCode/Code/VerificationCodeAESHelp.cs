using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VerificationCode.Code
{
    public class VerificationCodeAESHelp
    {
        /// <summary>
        /// Key123Ace#321Key
        /// </summary>
        private static readonly string _AESKEY = "qwertyuiopasdfghjklzxcvbnm123456";

        /// <summary>
        /// slide
        /// </summary>
        public const string _SlideCode = "slidecode.";

        /// <summary>
        ///验证码cookie
        /// </summary>
        public const string _YZM = "_YZM.";


        private IHttpContextAccessor _httpContextAccessor;

        public VerificationCodeAESHelp(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// AES加密返回base64字符串
        /// </summary>
        public string AES_Encrypt_Return_Base64String(string str)
        {
            string base64Str = AESEncrypt(str, _AESKEY);

            return base64Str;
        }


        /// <summary>
        /// AES解密返回string
        /// </summary>
        public string AES_Decrypt_Return_String(string str)
        {
            return AESDecrypt(str, _AESKEY);
        }



        #region AES

        private static string SubString(string sourceStr, int startIndex, int length)
        {
            string str;
            if (string.IsNullOrEmpty(sourceStr))
            {
                str = "";
            }
            else
            {
                str = (sourceStr.Length < startIndex + length ? sourceStr.Substring(startIndex) : sourceStr.Substring(startIndex, length));
            }
            return str;
        }


        private static byte[] _aeskeys = new byte[] { 18, 52, 86, 120, 144, 171, 205, 239, 18, 52, 86, 120, 144, 171, 205, 239 };
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string AESEncrypt(string encryptStr, string encryptKey)
        {

            string base64String;
            if (!string.IsNullOrWhiteSpace(encryptStr))
            {
                encryptKey = SubString(encryptKey, 0, 32);
                encryptKey = encryptKey.PadRight(32, ' ');
                SymmetricAlgorithm bytes = Rijndael.Create();
                byte[] numArray = Encoding.UTF8.GetBytes(encryptStr);
                bytes.Key = Encoding.UTF8.GetBytes(encryptKey);
                bytes.IV = _aeskeys;
                byte[] array = null;
                MemoryStream memoryStream = new MemoryStream();
                try
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, bytes.CreateEncryptor(), CryptoStreamMode.Write);
                    try
                    {
                        cryptoStream.Write(numArray, 0, numArray.Length);
                        cryptoStream.FlushFinalBlock();
                        array = memoryStream.ToArray();
                        cryptoStream.Close();
                        memoryStream.Close();
                    }
                    finally
                    {
                        if (cryptoStream != null)
                        {
                            ((IDisposable)cryptoStream).Dispose();
                        }
                    }
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        ((IDisposable)memoryStream).Dispose();
                    }
                }
                base64String = Convert.ToBase64String(array);
            }
            else
            {
                base64String = string.Empty;
            }
            return base64String;

        }



        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string AESDecrypt(string decryptStr, string decryptKey)
        {

            string empty;
            if (!string.IsNullOrWhiteSpace(decryptStr))
            {
                decryptKey = SubString(decryptKey, 0, 32);
                decryptKey = decryptKey.PadRight(32, ' ');
                byte[] numArray = Convert.FromBase64String(decryptStr);
                SymmetricAlgorithm bytes = Rijndael.Create();
                bytes.Key = Encoding.UTF8.GetBytes(decryptKey);
                bytes.IV = _aeskeys;
                byte[] numArray1 = new byte[numArray.Length];
                MemoryStream memoryStream = new MemoryStream(numArray);
                try
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, bytes.CreateDecryptor(), CryptoStreamMode.Read);
                    try
                    {
                        cryptoStream.Read(numArray1, 0, numArray1.Length);
                        cryptoStream.Close();
                        memoryStream.Close();
                    }
                    finally
                    {
                        if (cryptoStream != null)
                        {
                            ((IDisposable)cryptoStream).Dispose();
                        }
                    }
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        ((IDisposable)memoryStream).Dispose();
                    }
                }
                empty = Encoding.UTF8.GetString(numArray1).Replace("\0", "");
            }
            else
            {
                empty = string.Empty;
            }
            return empty;

        }

        #endregion


        #region  Cookie

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="minute"></param>
        public void SetCookie(string key, string value, int minute)
        {
            Microsoft.AspNetCore.Http.CookieOptions cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions();
            cookieOptions.Path = "/";
            cookieOptions.Expires = DateTimeOffset.Now.AddMinutes(minute);
            this._httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string</returns>
        public string GetCookie(string key)
        {
            string _cookie = this._httpContextAccessor.HttpContext.Request.Cookies[key];

            return _cookie;
        }



        #endregion
    }
}
