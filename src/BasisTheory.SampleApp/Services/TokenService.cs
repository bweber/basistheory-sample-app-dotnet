using System;
using System.Drawing;
using System.Threading.Tasks;
using BasisTheory.net.Common.Utilities;
using BasisTheory.net.Encryption;
using BasisTheory.net.Encryption.Extensions;
using BasisTheory.net.Tokens;
using BasisTheory.net.Tokens.Entities;
using BasisTheory.net.Tokens.Extensions;
using BasisTheory.net.Tokens.Requests;
using BasisTheory.SampleApp.Entities;
using Newtonsoft.Json;
using Pastel;

namespace BasisTheory.SampleApp.Services
{
    public interface ITokenService
    {
        Task Process(Options options);
    }

    public class TokenService : ITokenService
    {
        private readonly ITokenClient _tokenClient;
        private readonly IOptionsService _optionsService;
        private readonly IFileService _fileService;
        private readonly IEncryptionService _encryptionService;
        private readonly IProviderKeyService _providerKeyService;

        public TokenService(ITokenClient tokenClient, IOptionsService optionsService, IFileService fileService,
            IEncryptionService encryptionService, IProviderKeyService providerKeyService)
        {
            _tokenClient = tokenClient;
            _optionsService = optionsService;
            _fileService = fileService;
            _encryptionService = encryptionService;
            _providerKeyService = providerKeyService;
        }

        public async Task Process(Options options)
        {
            var createToken = true;
            while (createToken)
            {
                var context = await _optionsService.ProcessOptions(options);

                if (!PromptToContinue("Great, ready to create your first token?"))
                    return;

                var createdToken = await CreateToken(context);

                if (!PromptToContinue("Now that you have encrypted the data. Would you like to retrieve it?"))
                    return;

                await RetrieveEncryptedToken(createdToken, context);

                Console.WriteLine("The data that sits behind your token is encrypted and has to be decrypted to be readable.");
                if (!PromptToContinue("Would you like to decrypt it?"))
                    return;

                await RetrieveDecryptedToken(createdToken, context);

                Console.WriteLine("You just created a token and did end to end encryption/decryption.");
                createToken = PromptToContinue("Want to do another one?");

                options = new Options
                {
                    ApiKey = options.ApiKey
                };
            }
        }

        private async Task<Token> CreateToken(TokenContext context)
        {
            var toCreate = new Token
            {
                Type = "token",
                Data = context.Data,
                Metadata = context.Metadata
            };

            if (context.ProviderKey != null)
            {
                var encryptedData = await _encryptionService.EncryptAsync(context.Data, context.ProviderKey);
                toCreate.Data = encryptedData.CipherText;
                toCreate.Encryption = encryptedData.ToEncryptionMetadata();
            }

            Console.WriteLine($"POST to {("https://api.basistheory.com/tokens".Pastel(Color.Cyan))}");
            Console.WriteLine(JsonConvert.SerializeObject(toCreate, Formatting.Indented, new JsonSerializerSettings().InitializeDefaults()));

            var token = await _tokenClient.CreateAsync(toCreate);

            Console.WriteLine($"Done, here is your token: {token.Id.ToString().Pastel(Color.Cyan)}");
            Console.WriteLine();

            return token;
        }

        private async Task RetrieveEncryptedToken(Token createdToken, TokenContext context)
        {
            Console.WriteLine($"GET from {($"https://api.basistheory.com/tokens/{createdToken.Id}".Pastel(Color.Cyan))}");

            var token = await _tokenClient.GetByIdAsync(createdToken.Id);

            Console.WriteLine(context.DataType == DataType.TEXT
                ? $"Encrypted ciphertext is: {((string) token.Data).Pastel(Color.Cyan)}"
                : $"Encrypted file: {token.Metadata["file"].Pastel(Color.Cyan)}");
            Console.WriteLine();
        }

        private async Task RetrieveDecryptedToken(Token createdToken, TokenContext context)
        {
            Console.WriteLine($"GET from {($"https://api.basistheory.com/tokens/{createdToken.Id}/decrypt".Pastel(Color.Cyan))}");

            var token = await _tokenClient.GetByIdAsync(createdToken.Id, new TokenGetByIdRequest
            {
                Decrypt = true
            });

            if (token.Encryption?.KeyEncryptionKey != null)
            {
                var providerKey = await _providerKeyService.GetKeyByKeyIdAsync(token.Encryption.KeyEncryptionKey);
                token.Data = await _encryptionService.DecryptAsync(token.ToEncryptedData(), providerKey);
            }

            if (context.DataType == DataType.TEXT)
                Console.WriteLine($"Your decrypted data is: {((string)token.Data).Pastel(Color.Cyan)}");
            else
            {
                var outputPath = _fileService.SaveFileData(new FileData($"output/{token.Metadata["file"]}", Convert.FromBase64String(token.Data)));
                Console.WriteLine($"Your decrypted file was written to: {outputPath.Pastel(Color.Cyan)}");
            }

            Console.WriteLine();
        }

        private static bool PromptToContinue(string question)
        {
            ConsoleKey response;
            do
            {
                Console.Write($"{question} [y/n] ");
                response = Console.ReadKey(false).Key;
                if (response != ConsoleKey.Enter)
                    Console.WriteLine();
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            Console.WriteLine();

            return response == ConsoleKey.Y;
        }
    }
}
