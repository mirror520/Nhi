using System.Linq;
using System.Text.Json.Serialization;

namespace Nhi
{
    public class User
    {

        public User(byte[] adpuData)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var UTF8 = System.Text.Encoding.UTF8;
            var BIG5 = System.Text.Encoding.GetEncoding("big5");

            CardNumber  = UTF8.GetString(adpuData.Take(12).ToArray());
            Name        = BIG5.GetString(adpuData.Skip(12).Take(20).ToArray()).TrimEnd('\0');
            IdCard      = UTF8.GetString(adpuData.Skip(32).Take(10).ToArray());
            Birthday    = UTF8.GetString(adpuData.Skip(42).Take(7).ToArray());
            Gender      = UTF8.GetString(adpuData.Skip(49).Take(1).ToArray()) == "M" ? "Male" : "Female";
            CardPublish = UTF8.GetString(adpuData.Skip(50).Take(7).ToArray());
        }

        [JsonPropertyName("card_number")]
        public string CardNumber { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("id_card")]
        public string IdCard { get; init; }

        [JsonPropertyName("birthday")]
        public string Birthday { get; init; }

        [JsonPropertyName("gender")]
        public string Gender { get; init; }

        [JsonPropertyName("card_publish")]
        public string CardPublish { get; init; }
    }
}
