using System;
using System.Linq;
using System.Reflection;

namespace SIS.MvcFramework
{
    public static class ObjectMapper
    {
        public static T To<T>(this object source) //extension method върху object
            where T : new() //могат да бъдат подавани само неща, които могат да имат инстанция с празен конструктор
        {
            T destination = new T();
            PropertyInfo[] destinationProperties = destination.GetType().GetProperties();

            foreach (PropertyInfo destinationProperty in destinationProperties)
            {
                if(destinationProperty.SetMethod == null) //read-only
                {
                    continue;
                }

                PropertyInfo sourceProperty = source.GetType().GetProperties()
                    .FirstOrDefault(x => x.Name.ToLower() == destinationProperty.Name.ToLower());

                if(sourceProperty?.GetMethod != null)
                {
                    object sourceValue = sourceProperty.GetMethod.Invoke(source, new object[0]);
                    object destinationValue = TryParse(sourceValue.ToString(), destinationProperty.PropertyType);
                    destinationProperty.SetMethod.Invoke(destination, new[] { destinationValue });

                }
            }

            return destination;
        }

        public static object TryParse(string stringValue, Type type) //В какъв тип искам да превърна този стринг
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            object value = null;
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    break;
                case TypeCode.Char:
                    if (char.TryParse(stringValue, out var charValue))
                    {
                        value = charValue;
                    }
                    break;
                case TypeCode.DateTime:
                    if (DateTime.TryParse(stringValue, out var decimalValue))
                    {
                        value = decimalValue;
                    }
                    break;
                case TypeCode.Decimal:
                    if (decimal.TryParse(stringValue, out var dateTimeValue))
                    {
                        value = dateTimeValue;
                    }
                    break;
                case TypeCode.Double:
                    if (double.TryParse(stringValue, out var doubleValue))
                    {
                        value = doubleValue;
                    }
                    break;
                case TypeCode.Int32:
                    if (int.TryParse(stringValue, out var intValue))
                    {
                        value = intValue;
                    }
                    break;
                case TypeCode.Int64:
                    if (long.TryParse(stringValue, out var longValue))
                    {
                        value = longValue;
                    }
                    break;
                case TypeCode.String:
                    value = stringValue;
                    break;
            }

            return value;
        }
    }
}