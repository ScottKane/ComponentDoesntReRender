using System;
using System.Text.Json.Serialization;

namespace ComponentDoesntReRender
{
    public class FormResponse
    {
        public FormResponse(string value, Type type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; set; }
        [JsonIgnore]
        public Type Type { get; set; }
    }
}
