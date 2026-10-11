

using Newtonsoft.Json;
using System.Runtime.Serialization;


[JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
public enum Player
{
    [EnumMember(Value = "X")]
    X ,
    [EnumMember(Value = "O")]
    O
}
