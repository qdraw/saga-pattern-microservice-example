using System;
using System.ComponentModel;
using System.Linq;

namespace ACME.Library.Common.Extensions
{
    public static class StringExtensions
    {
        public static Guid ToGuid(this string guid)
        {
            Guid.TryParse(guid, out var result);
            return result;
        }
        public static Guid ToGuidFromString(string guid)
        {
            return guid.ToGuid();
        }
        public static bool TryConvertCollection<T>(this string[] values, out T[] output)
        {
            output = Array.Empty<T>();
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    output = values.Select(converter.ConvertFromString).Cast<T>().ToArray();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}