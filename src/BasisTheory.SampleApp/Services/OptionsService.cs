using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BasisTheory.net.Encryption;
using BasisTheory.net.Encryption.Entities;
using BasisTheory.SampleApp.Entities;

namespace BasisTheory.SampleApp.Services
{
    public interface IOptionsService
    {
        Task<TokenContext> ProcessOptions(Options options);
    }

    public class OptionsService : IOptionsService
    {
        private readonly IFileService _fileService;
        private readonly IProviderKeyService _providerKeyService;

        public OptionsService(IFileService fileService, IProviderKeyService providerKeyService)
        {
            _fileService = fileService;
            _providerKeyService = providerKeyService;
        }

        public async Task<TokenContext> ProcessOptions(Options options)
        {
            var context = await ProcessDataInput(options);

            Console.WriteLine();

            context.ProviderKey = await ProcessEncryptionOptions(options);

            return context;
        }

        private async Task<TokenContext> ProcessDataInput(Options options)
        {
            if (!string.IsNullOrEmpty(options.Text))
                return new TokenContext(DataType.TEXT, options.Text);

            if (!string.IsNullOrEmpty(options.FilePath))
                return await ReadFile(options.FilePath);

            Console.WriteLine("What type of information would you like to encrypt and tokenize?");
            Console.WriteLine("1. Text");
            Console.WriteLine("2. File");
            ConsoleKey inputResponse;
            do
            {
                Console.Write("Choose option (1 or 2): ");
                inputResponse = Console.ReadKey(false).Key;
                if (inputResponse != ConsoleKey.Enter)
                    Console.WriteLine();

            } while (inputResponse != ConsoleKey.D1 && inputResponse != ConsoleKey.D2);

            Console.WriteLine();

            switch (inputResponse)
            {
                case ConsoleKey.D1:
                    Console.Write("Text to be tokenized: ");
                    return new TokenContext(DataType.TEXT, Console.ReadLine());

                case ConsoleKey.D2:
                    Console.Write("Full path to file to be tokenized: ");

                    TokenContext fileData = null;
                    do
                    {
                        try
                        {
                            var filePath = Console.ReadLine();
                            fileData = await ReadFile(filePath);
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("Invalid file path provided");
                            Console.Write("Full path to file to tokenized: ");
                        }
                    } while (fileData == null);

                    return fileData;
            }

            throw new ArgumentException("Invalid input provided");
        }

        private async Task<ProviderEncryptionKey> ProcessEncryptionOptions(Options options)
        {
            Console.WriteLine("Would you like to encrypt your own data or let Basis Theory encrypt your data?");
            Console.WriteLine("1. Encrypt using my keys (only you can decrypt and get it back)");
            Console.WriteLine("2. Let Basis Theory encrypt my data (BT can decrypt for you)");

            ConsoleKey inputResponse;
            do
            {
                Console.Write("Choose option (1 or 2): ");
                inputResponse = Console.ReadKey(false).Key;
                if (inputResponse != ConsoleKey.Enter)
                    Console.WriteLine();

            } while (inputResponse != ConsoleKey.D1 && inputResponse != ConsoleKey.D2);

            Console.WriteLine();

            if (inputResponse == ConsoleKey.D1)
            {
                Console.Write("What should we name your encryption key? ");
                options.EncryptKey = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(options.EncryptKey)) return null;

            Console.WriteLine();
            var providerKey = await _providerKeyService.GetOrCreateKeyAsync(options.EncryptKey, "FILE", "RSA");
            Console.WriteLine();
            return providerKey;
        }

        private async Task<TokenContext> ReadFile(string filePath)
        {
            var (fileName, contents) = await _fileService.GetFileData(filePath);
            return new TokenContext(DataType.FILE, Convert.ToBase64String(contents), new Dictionary<string, string>
            {
                { "file", Path.GetFileName(fileName) }
            });
        }
    }
}
