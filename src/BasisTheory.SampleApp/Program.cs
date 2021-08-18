using System;
using System.Drawing;
using System.Threading.Tasks;
using BasisTheory.net.AspNetCore;
using BasisTheory.net.Encryption;
using BasisTheory.net.Tokens;
using BasisTheory.SampleApp.Encryption;
using BasisTheory.SampleApp.Entities;
using BasisTheory.SampleApp.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Pastel;

namespace BasisTheory.SampleApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Process);
        }

        private static async Task Process(Options options)
        {
            try
            {
                PrintLogo();

                Console.WriteLine();
                Console.WriteLine("Welcome to our sample app! Basis Theory makes it easy to build secure");
                Console.WriteLine("applications. This app walks you through creating your first encrypted token,");
                Console.WriteLine("retrieving it, and decrypting it. You can even choose if you want to encrypt");
                Console.WriteLine("using encryption keys on your machine or leveraging our server. But first, we");
                Console.WriteLine("need your api key. You can generate one here:");
                Console.WriteLine("https://portal.basistheory.com/applications".Pastel(Color.Cyan));

                Console.WriteLine();

                var apiKey = options.ApiKey;
                while (string.IsNullOrEmpty(apiKey))
                {
                    Console.Write("Enter API Key: ");
                    apiKey = Console.ReadLine();

                    Console.WriteLine();
                }

                var serviceProvider = new ServiceCollection()
                    .AddLazyCache()
                    .AddSingleton<IFileService, FileService>()
                    .AddSingleton<IOptionsService, OptionsService>()
                    .AddSingleton<ITokenService, TokenService>()
                    .AddSingleton<ITokenClient>(_ => new TokenClient(apiKey))
                    .AddSingleton<IProviderKeyFactory, RSAFileProviderKeyFactory>()
                    .AddSingleton<IEncryptionFactory, RSAFileEncryptionFactory>()
                    .AddBasisTheoryEncryption()
                    .BuildServiceProvider();

                var tokenService = serviceProvider.GetRequiredService<ITokenService>();
                await tokenService.Process(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void PrintLogo()
        {
            Console.WriteLine("                                     ".PastelBg(Color.DarkBlue));
            Console.WriteLine("                                     ".PastelBg(Color.DarkBlue));

            Console.Write("        ".PastelBg(Color.DarkBlue));
            Console.Write("                     ".PastelBg(Color.Cyan));
            Console.WriteLine("        ".PastelBg(Color.DarkBlue));

            for (var i = 0; i < 10; i++)
            {
                Console.Write("                          ".PastelBg(Color.DarkBlue));
                Console.Write("   ".PastelBg(Color.Cyan));
                Console.WriteLine("        ".PastelBg(Color.DarkBlue));
            }

            Console.WriteLine("                                     ".PastelBg(Color.DarkBlue));
            Console.WriteLine("                                     ".PastelBg(Color.DarkBlue));
        }
    }
}
