namespace ACME.Library.RabbitMq.Serializer
{
    using Attributes;
    using EasyNetQ;
    using EasyNetQ.SystemMessages;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CustomEasyNetQTypeNameSerializer : ITypeNameSerializer
    {
        public readonly Dictionary<string, Type> TypesByName = new Dictionary<string, Type>();
        public readonly Dictionary<Type, string> NamesByType = new Dictionary<Type, string>();

        public CustomEasyNetQTypeNameSerializer()
        {
            RegisterEasyNetQtypes();
            RegisterTypes(typeof(CustomEasyNetQTypeNameSerializer).Assembly);
        }

        private void RegisterTypes(Assembly assembly)
        {
            var serializableTypes = GetSerializableTypes(assembly);

            foreach (var kvp in serializableTypes)
            {
                if (!string.IsNullOrEmpty(kvp.SimpleName))
                {
                    RegisterType(kvp.SimpleName, kvp.Type);
                }
            }
        }

        private void RegisterEasyNetQtypes()
        {
            RegisterType(typeof(Error).FullName, typeof(Error));
        }

        private void RegisterType(string simpleName, Type type)
        {
            TypesByName.Add(simpleName, type);
            NamesByType.Add(type, simpleName);
        }

        private static IEnumerable<(Type Type, string SimpleName)> GetSerializableTypes(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(BusTypeNameAttribute), true).FirstOrDefault() is BusTypeNameAttribute attribute)
                {
                    yield return (type, attribute.TypeName);
                }
            }
        }

        public string Serialize(Type type)
        {
            var typeName = NamesByType[type];
            return typeName;
        }

        public Type DeSerialize(string typeName)
        {
            var type = TypesByName[typeName];
            return type;
        }
    }
}
