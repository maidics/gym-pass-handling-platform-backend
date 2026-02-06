using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitPass.Application.Common.Settings;

public static class JsonDefaults
{
    public static JsonSerializerOptions SerializerOptions =>
        new JsonSerializerOptions(
            JsonSerializerDefaults.Web /* sets: naming policy: camel case, name case-insensitive: true*/
        )
        {
            Converters = { new JsonStringEnumConverter() },
        };
}
