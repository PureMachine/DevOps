using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OctopusManager.Utils
{
    public static class ExpandoObjectExtensions
    {
        public static ExpandoObject Clone(this ExpandoObject original)
        {
            var expandoObjectConverter = new ExpandoObjectConverter();
            var originalDoc = JsonConvert.SerializeObject(original, expandoObjectConverter);

            dynamic clone = JsonConvert.DeserializeObject<ExpandoObject>(originalDoc, expandoObjectConverter);

            return clone;
        }
    }
}
