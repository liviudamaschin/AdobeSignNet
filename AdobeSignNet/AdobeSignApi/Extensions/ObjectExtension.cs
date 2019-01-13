using Newtonsoft.Json;

namespace AdobeSignApi.Extensions
{
    public static class ObjectExtension
    {
        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {

            if (obj.GetType().GetProperty(propertyName) != null)
                return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
            return null;
        }
    }
}