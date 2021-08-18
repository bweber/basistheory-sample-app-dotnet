using System.Collections.Generic;
using BasisTheory.net.Encryption.Entities;

namespace BasisTheory.SampleApp.Entities
{
    public class TokenContext
    {
        public DataType DataType { get; }
        public string Data { get; set; }
        public Dictionary<string, string> Metadata { get; }

        public ProviderEncryptionKey ProviderKey { get; set; }

        public TokenContext(DataType dataType, string data, Dictionary<string, string> metadata = null)
        {
            DataType = dataType;
            Data = data;
            Metadata = metadata;
        }
    }

    public enum DataType
    {
        TEXT,
        FILE
    }
}
