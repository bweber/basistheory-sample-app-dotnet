using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BasisTheory.net.Encryption;
using BasisTheory.SampleApp.Services;

namespace BasisTheory.SampleApp.Encryption
{
    public class RSAFileEncryptionFactory : IEncryptionFactory
    {
        public string Provider => "FILE";
        public string Algorithm => "RSA";

        private readonly IFileService _fileService;

        public RSAFileEncryptionFactory(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<string> EncryptAsync(string providerKeyId, string plaintext,
            CancellationToken cancellationToken = default)
        {
            var publicKeyName = $"{providerKeyId}_pub.pem";
            Console.Write($"Encrypting on your machine with {publicKeyName}...");

            var publicKeyData = await _fileService.GetFileData($"keys/{publicKeyName}");

            using var rsa = RSA.Create();
            rsa.ImportFromPem(Encoding.UTF8.GetString(publicKeyData.Contents));

            var encryptedData = Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(plaintext), RSAEncryptionPadding.Pkcs1));

            Console.WriteLine("complete");
            Console.WriteLine();

            return encryptedData;
        }

        public async Task<string> DecryptAsync(string providerKeyId, string ciphertext,
            CancellationToken cancellationToken = default)
        {
            var privateKeyName = $"{providerKeyId}.pem";
            Console.Write($"Decrypting on your machine with {privateKeyName}...");

            var privateKeyData = await _fileService.GetFileData($"keys/{privateKeyName}");

            using var rsa = RSA.Create();
            rsa.ImportFromPem(Encoding.UTF8.GetString(privateKeyData.Contents));

            var decryptedData = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(ciphertext), RSAEncryptionPadding.Pkcs1));

            Console.WriteLine("complete");
            Console.WriteLine();

            return decryptedData;
        }
    }
}
