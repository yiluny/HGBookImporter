using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HG.Coprorate.Firebrand.Helpers
{
    public class CryptoHelper
    {
        public static string GenerateMd5Hash(string s)
        {
            string hash;

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    .Replace("-", String.Empty);
            }

            return hash;
        }
    }
}
