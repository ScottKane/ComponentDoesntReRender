using JsonFlatten;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ComponentDoesntReRender
{
    public static class Utilities
    {
        public static object SetPropertyValueFromKey(string key, object item, object value, string[] properties = null)
        {
            var propertyNames = properties is null ? key.Split('.').ToList() : properties.ToList();
            var type = item.GetType();
            if (propertyNames.Count == 1)
            {
                type.GetProperty(propertyNames.FirstOrDefault()).SetValue(item, value);
                return item;
            }

            var names = key.Split('.')[1..];
            item = type.GetProperty(propertyNames.FirstOrDefault()).GetValue(item);

            return SetPropertyValueFromKey(string.Join('.', names), item, value, names);
        }

        public static object GetPropertyValueFromKey(string key, object item, string[] properties = null)
        {
            var propertyNames = properties is null ? key.Split('.').ToList() : properties.ToList();
            var type = item.GetType();
            if (propertyNames.Count == 1) return type.GetProperty(propertyNames.FirstOrDefault()).GetValue(item);

            var names = key.Split('.')[1..];
            item = type.GetProperty(propertyNames.FirstOrDefault()).GetValue(item);

            return GetPropertyValueFromKey(string.Join('.', names), item, names);
        }

        public static PropertyInfo GetPropertyFromKey(string key, object item, string[] properties = null)
        {
            var propertyNames = properties is null ? key.Split('.').ToList() : properties.ToList();
            var type = item.GetType();
            if (propertyNames.Count == 1) return type.GetProperty(propertyNames.FirstOrDefault());

            var names = key.Split('.')[1..];
            item = type.GetProperty(propertyNames.FirstOrDefault()).GetValue(item);

            return GetPropertyFromKey(string.Join('.', names), item, names);
        }

        public static Dictionary<string, FormResponse> GetFormItemProperties(Dictionary<string, object> properties) => properties.ToDictionary(x => x.Key, x => x.Value is string ? new FormResponse(x.Value.ToString(), x.Value.GetType()) : new FormResponse(JsonSerializer.Serialize(x.Value), x.Value.GetType()));
        public static Dictionary<string, object> GetItemProperties(object item) => (Dictionary<string, object>)JObject.Parse(JsonSerializer.Serialize(item)).Flatten();
        public static TItem CreateFormItem<TItem>(Dictionary<string, FormResponse> formProperties) => (TItem)CreateFormItem(typeof(TItem), formProperties);
        public static TItem CreateDefaultItem<TItem>() => (TItem)CreateDefaultItem(typeof(TItem));

        public static object CreateFormItem(Type type, Dictionary<string, FormResponse> formProperties, object result = null)
        {
            static object FormResponseToObject(string value, Type type)
            {
                if (type == typeof(string)) return value;
                if (type.Name.Contains("EmptyPartition")) type = typeof(List<object>);
                return JsonSerializer.Deserialize(value, type);
            }

            var properties = formProperties.ToDictionary(x => x.Key, x => FormResponseToObject(x.Value.Value, x.Value.Type));

            return JsonSerializer.Deserialize(properties.Unflatten().ToString(), type);
        }

        public static object CreateDefaultItem(Type type, object item = null)
        {
            void SetProperty(PropertyInfo property, object instance)
            {
                if (property.PropertyType.IsPrimitive || property.PropertyType.IsEnum || property.PropertyType == typeof(string))
                {
                    if (!type.IsAssignableTo(typeof(IList)) && !type.IsInterface)
                    {
                        if (property.PropertyType.IsPrimitive) property.SetValue(instance, Activator.CreateInstance(property.PropertyType));
                        if (property.PropertyType == typeof(string)) property.SetValue(instance, string.Empty);
                    }
                }
                else
                {
                    property.SetValue(item, Activator.CreateInstance(property.PropertyType));
                    CreateDefaultItem(property.PropertyType, property.GetValue(item));
                }
            }

            if (item is null) item = Activator.CreateInstance(type);

            foreach (var property in type.GetProperties())
                if (property.GetSetMethod() is not null)
                    SetProperty(property, item);

            return item;
        }
    }
}
