using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecuritySystem.Business.Encryption
{
    public class Encryptor
    {
        public string Encrypt(string text)
        {
            var encryptedText="";
            byte[] data = UTF8Encoding.UTF8.GetBytes(text);
            using (var sha512 = SHA512.Create())
            {
                var result = sha512.ComputeHash(data);
                encryptedText = Convert.ToBase64String(result, 0, result.Length);
            }
            return encryptedText;
        }

    }
}
