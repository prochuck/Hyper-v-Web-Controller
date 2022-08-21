using Hyper_v_Web_Controller.Interfaces;
using Hyper_v_Web_Controller.Models;
using System.Security.Cryptography;
using System.Text;

namespace Hyper_v_Web_Controller.Services
{
    public class HashService : IHashService
    {

        public string GetHash(string key)
        {
            StringBuilder stringBuilder=new StringBuilder();
            foreach (var item in SHA256.HashData(Encoding.UTF8.GetBytes(key)))
            {
                stringBuilder.Append(item.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
