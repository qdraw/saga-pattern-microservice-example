namespace ACME.Library.RabbitMq.Attributes
{
    using System;

    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BusTypeNameAttribute : Attribute
    {
        public BusTypeNameAttribute(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; set; }
    }
}
