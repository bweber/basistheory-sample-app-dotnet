using CommandLine;

namespace BasisTheory.SampleApp.Entities
{
    public class Options
    {
        [Option("api-key", HelpText = "API Key with token:* permissions")]
        public string ApiKey { get; set; }

        [Option("text", HelpText = "Text to be tokenized")]
        public string Text { get; set; }

        [Option("file", HelpText = "Full path to file to be tokenized")]
        public string FilePath { get; set; }

        [Option("encrypt-key", HelpText = "The name of the encryption key used to encrypt the data")]
        public string EncryptKey { get; set; }
    }
}
