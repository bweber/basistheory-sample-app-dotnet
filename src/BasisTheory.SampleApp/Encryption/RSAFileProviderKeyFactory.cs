using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BasisTheory.net.Encryption;
using BasisTheory.net.Encryption.Entities;
using BasisTheory.SampleApp.Entities;
using BasisTheory.SampleApp.Services;

namespace BasisTheory.SampleApp.Encryption
{
    public class RSAFileProviderKeyFactory : IProviderKeyFactory
    {
        public string Provider => "FILE";
        public string Algorithm => "RSA";

        private readonly IFileService _fileService;

        public RSAFileProviderKeyFactory(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<ProviderEncryptionKey> GetOrCreateKeyAsync(string name)
        {
            var existingKey = await GetKeyByName(name);
            if (existingKey != null)
            {
                Console.WriteLine($"It looks like a key already exists for {name}, we will use that one");
                return existingKey;
            }

            Console.WriteLine($"We couldn't find any encryption keys for {name}, generating now...");
            Console.WriteLine();

            using var rsa = RSA.Create(4096);

            const string privateKeyHeader = "-----BEGIN RSA PRIVATE KEY-----";
            const string privateKeyFooter = "-----END RSA PRIVATE KEY-----";
            const string publicKeyHeader = "-----BEGIN RSA PUBLIC KEY-----";
            const string publicKeyFooter = "-----END RSA PUBLIC KEY-----";

            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            var privateKeyBytes = Encoding.UTF8.GetBytes($"{privateKeyHeader}\n{privateKey}\n{privateKeyFooter}");

            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            var publicKeyBytes = Encoding.UTF8.GetBytes($"{publicKeyHeader}\n{publicKey}\n{publicKeyFooter}");

            var privateKeyPath = _fileService.SaveFileData(new FileData($"keys/{name}.pem", privateKeyBytes));
            Console.WriteLine($"Your private key was written to: {privateKeyPath}");

            var publicKeyPath = _fileService.SaveFileData(new FileData($"keys/{name}_pub.pem", publicKeyBytes));
            Console.WriteLine($"Your public key was written to: {publicKeyPath}");

            return new ProviderEncryptionKey
            {
                KeyId = name,
                Name = name,
                ProviderKeyId = name,
                Provider = Provider,
                Algorithm = Algorithm
            };
        }

        public async Task<ProviderEncryptionKey> GetKeyByKeyIdAsync(string keyId)
        {
            return await GetKeyByName(keyId);
        }

        private async Task<ProviderEncryptionKey> GetKeyByName(string keyId)
        {
            try
            {
                var key = await _fileService.GetFileData($"keys/{keyId}.pem");
                if (key == null) return null;

                return new ProviderEncryptionKey
                {
                    KeyId = keyId,
                    ProviderKeyId = keyId,
                    Algorithm = "RSA",
                    Provider = "FILE"
                };
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
