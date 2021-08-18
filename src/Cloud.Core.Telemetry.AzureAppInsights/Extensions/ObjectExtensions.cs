using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cloud.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace Cloud.Core.Telemetry.AzureAppInsights.Extensions
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Builds a dictionary of all reflected properties of an object, using a delimiter to denoate sub-type properties
        /// i.e. a class could be reflected as:
        /// "Prop1"    "Value1"
        /// "Prop2:A"  "Value2"
        /// "Prop2:B"  "Value3"
        /// "Prop2:C"  "Value4"
        /// "Prop3"    "true"
        /// "Prop4"    "500"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keyCasing">The string casing for outputted keys.</param>
        /// <param name="keyDelimiter">The delimiter.</param>
        /// <param name="maskPiiData">If set to <c>true</c> mask pii data (properties marked with PersonalData attribute).</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        internal static Dictionary<string, string> AsFlatStringDictionary<T>(this T source, StringCasing keyCasing = StringCasing.Unchanged, bool maskPiiData = false,
            string keyDelimiter = ":", BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                where T : class, new()
        {
            // If the source is not set, return empty.
            if (source == null)
                return new Dictionary<string, string>();

            if (source is JToken token)
            {
                return token.AsFlatStringDictionary(keyCasing, maskPiiData, keyDelimiter, bindingAttr);
            }

            // Return final resulting flat dictionary.
            return source.GetFlatDictionary(keyCasing, keyDelimiter, String.Empty, maskPiiData, bindingAttr);
        }

        /// <summary>
        /// Builds a dictionary of all reflected properties of a JObject, using a delimiter to denoate sub-type properties
        /// i.e. a class could be reflected as:
        /// "Prop1"    "Value1"
        /// "Prop2:A"  "Value2"
        /// "Prop2:B"  "Value3"
        /// "Prop2:C"  "Value4"
        /// "Prop3"    "true"
        /// "Prop4"    "500"
        /// </summary>
        /// <param name="source">The JToken source.</param>
        /// <param name="keyCasing">The string casing for outputted keys.</param>
        /// <param name="keyDelimiter">The delimiter.</param>
        /// <param name="maskPiiData">If set to <c>true</c> mask pii data (properties marked with PersonalData attribute).</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        internal static Dictionary<string, string> AsFlatStringDictionary(this JToken source, StringCasing keyCasing = StringCasing.Unchanged, bool maskPiiData = false,
            string keyDelimiter = ":", BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {

            JObject inner = source.Root.Value<JObject>();
            var tokenDict = inner.ToDictionary();
            return tokenDict.GetFlatDictionary(keyCasing, keyDelimiter, String.Empty, maskPiiData, bindingAttr);
        }

        private static Dictionary<string, string> GetFlatDictionary<T>(this T source, StringCasing keyCasing = StringCasing.Unchanged, string keyDelimiter = ":",
            string prefix = "", bool maskPiiData = false, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                where T : class, new()
        {
            var returnDict = new Dictionary<string, string>();

            // Return empty dictionary if the source is null.
            if (source == null)
                return returnDict;

            // If this is a dictionary, parse each element one by one.
            if (source.IsDictionary())
            {
                foreach (DictionaryEntry item in source as IDictionary)
                {
                    var itemKey = $"{prefix}{(prefix.Length > 0 ? keyDelimiter : "")}{item.Key}".WithCasing(keyCasing);
                    if (item.Value.IsDictionary())
                    {
                        returnDict.AddRange(GetFlatDictionary(item.Value, keyCasing, keyDelimiter, itemKey, maskPiiData, bindingAttr));
                    }
                    else if (item.Value.GetType().IsEnumerableType())
                    {
                        returnDict.AddRange(GetProperty(item.Key.ToString(), item.Value, prefix, keyCasing, keyDelimiter, maskPiiData, bindingAttr));
                    }
                    else
                    {
                        returnDict.Add(itemKey, item.Value.ToString());
                    }
                }
            }
            else
            {
                // Otherwise, if this is an object, parse each property.
                var rootItems = source.GetType().GetProperties(bindingAttr);

                // If the reflected values length is zero, just add now to the dictionary.
                if (rootItems.Length == 0)
                {
                    returnDict.Add(prefix, source == null ? String.Empty : source.ToString());
                }
                else
                {
                    if (!prefix.IsNullOrEmpty())
                        prefix += keyDelimiter;

                    // Loop through each reflected property in order to build up the returned dictionary key/values.
                    foreach (var item in rootItems)
                    {
                        returnDict.AddRange(GetProperty(item.Name, item.GetValue(source, null), prefix, keyCasing, keyDelimiter, maskPiiData, bindingAttr));
                    }
                }
            }

            // Return final resulting flat dictionary.
            return returnDict;
        }

        private static Dictionary<string, string> GetProperty(string name, object value, string prefix, StringCasing keyCasing, string keyDelimiter, bool maskPiiData, BindingFlags bindingAttr)
        {
            var returnDict = new Dictionary<string, string>();
            var key = $"{prefix}{name}".WithCasing(keyCasing);
            Type valueType = value != null ? value.GetType() : null;
            bool isEnumerable = valueType != null && valueType.IsEnumerableType();

            if (value == null || value.Equals(valueType.GetDefault()))
            {
                // If we dont have a value, add as empty string.
                returnDict.Add(key, "");
            }
            else if (value.IsDictionary() && !isEnumerable)
            {
                foreach (DictionaryEntry item in value as IDictionary)
                {
                    var itemKey = $"{prefix}{(prefix.Length > 0 ? keyDelimiter : "")}{item.Key}".WithCasing(keyCasing);
                    if (item.Value.IsDictionary())
                    {
                        returnDict.AddRange(GetFlatDictionary(item.Value, keyCasing, keyDelimiter, itemKey, maskPiiData, bindingAttr));
                    }
                    else
                    {
                        returnDict.Add(itemKey, item.Value.ToString());
                    }
                }
            }
            else if (isEnumerable)
            {
                // If this is an enumerable, then loop through each item and get its value for the dictionary.
                IEnumerable vals = value as IEnumerable;
                var index = 0;

                foreach (var val in vals)
                {
                    returnDict.AddRange(val.GetFlatDictionary(keyCasing, keyDelimiter, $"{key}[{index}]", maskPiiData, bindingAttr));
                    index++;
                }
            }
            else if (valueType.IsSystemType())
            {
                // If this is a plain old system type, then just add straight into the dictionary.
                returnDict.Add($"{key}", maskPiiData && (valueType.GetPiiDataProperties().Any() || valueType.GetSensitiveInfoProperties().Any()) ? "*****" : value.ToString());
            }
            else
            {
                // Otherwise, reflect all properties of this complex type in the next level of the dictionary.
                returnDict.AddRange(value.GetFlatDictionary(keyCasing, keyDelimiter, key, maskPiiData, bindingAttr));
            }

            return returnDict;
        }

        /// <summary>Converts JObject to array.</summary>
        /// <param name="array">The array.</param>
        /// <returns>System.Object[].</returns>
        internal static object[] ToArray(this JArray array)
        {
            return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
        }

        /// <summary>Converts JObject to dictionary.</summary>
        /// <param name="json">The json.</param>
        /// <returns>Dictionary&lt;System.String, System.Object&gt;.</returns>
        internal static Dictionary<string, object> ToDictionary(this JObject json)
        {
            var propertyValuePairs = json.ToObject<Dictionary<string, object>>();
            ProcessJObjectProperties(propertyValuePairs);
            ProcessJArrayProperties(propertyValuePairs);
            return propertyValuePairs;
        }

        private static void ProcessJObjectProperties(Dictionary<string, object> propertyValuePairs)
        {
            var objectPropertyNames = (from property in propertyValuePairs
                                       let propertyName = property.Key
                                       let value = property.Value
                                       where value is JObject
                                       select propertyName).ToList();

            objectPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToDictionary((JObject)propertyValuePairs[propertyName]));
        }

        private static void ProcessJArrayProperties(Dictionary<string, object> propertyValuePairs)
        {
            var arrayPropertyNames = (from property in propertyValuePairs
                                      let propertyName = property.Key
                                      let value = property.Value
                                      where value is JArray
                                      select propertyName).ToList();

            arrayPropertyNames.ForEach(propertyName => propertyValuePairs[propertyName] = ToArray((JArray)propertyValuePairs[propertyName]));
        }

        private static object ProcessArrayEntry(object value)
        {
            if (value is JObject)
            {
                return ToDictionary((JObject)value);
            }
            if (value is JArray)
            {
                return ToArray((JArray)value);
            }
            return value;
        }

        /// <summary>
        ///Gets a default value for the passed in property type.
        /// </summary>
        /// <param name="prop">The type to check.</param>
        /// <returns>Dictionary of PiiData properties.</returns>
        internal static object GetDefault(this Type prop)
        {
            return prop.IsValueType ? Activator.CreateInstance(prop) : null;
        }
    }
}

