using SIS.MvcFramework.Loggers;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SIS.MvcFramework.Services
{
    public class HashService : IHashService
    {
        private readonly ILogger logger;

        public HashService(ILogger logger)
        {
            this.logger = logger;
        }

        public string Hash(string stringToHash)
        {

            stringToHash = stringToHash + "myAppSalt#"; //Тук слагам солта!
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                string hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                this.logger.Log(hash);
                return hash;
            }
        }
    }
}