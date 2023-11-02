using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace OneOf.Serialization
{
    internal enum OneOfArgToken
    {
        None = 0,
        Object,
        //String,
        StringOrEnum,
        Integer,
        Float,
        Boolean,
        Date,
    }

    internal static class OneOfJsonConverterCommon
    {
        public static readonly List<Type> EmptyTypeList = new List<Type> (0);

        public static OneOfArgToken MapJTokenTypeToOneOfArgToken(JTokenType jTokenType)
        {
            switch (jTokenType)
            {
            case JTokenType.Object:     return OneOfArgToken.Object;
            case JTokenType.Integer:    return OneOfArgToken.Integer;
            case JTokenType.Float:      return OneOfArgToken.Float;
            case JTokenType.String:     return OneOfArgToken.StringOrEnum;
            case JTokenType.Boolean:    return OneOfArgToken.Boolean;
            case JTokenType.Date:       return OneOfArgToken.Date;
            //case JTokenType.Guid:       return OneOfArgToken.String;
            //case JTokenType.Uri:        return OneOfArgToken.String;
            default:                    return OneOfArgToken.None;
            }
        }

        public static OneOfArgToken GetOneOfArgToken(Type type)
        {
            if (type == typeof(string) || type.IsEnum)
                return OneOfArgToken.StringOrEnum;
            if (type == typeof(long)  || type == typeof(ulong)  ||
                type == typeof(int)   || type == typeof(uint)   ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(sbyte) || type == typeof(byte))
                return OneOfArgToken.Integer;
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return OneOfArgToken.Float;
            if (type == typeof(bool))
                return OneOfArgToken.Boolean;
            if (type == typeof(DateTime))
                return OneOfArgToken.Date;
            if (type.IsClass)
                return OneOfArgToken.Object;
            return OneOfArgToken.None;
        }

        public static bool CanConvertDirectly(Type objectType, Type oneOfType) => objectType == oneOfType;

        public static bool CanConvertByEnumerating(Type objectType, Type oneOfType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(objectType))
            {
                var enTypes = objectType?.GenericTypeArguments ?? Array.Empty<Type>();
                return enTypes.Length == 1 && enTypes[0] == oneOfType;
            }
            return false;
        }

    }
}