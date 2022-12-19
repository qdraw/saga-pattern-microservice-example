using System;

namespace ACME.Library.Saga
{
    public class SagaConfiguration
    {
        public Type SagaType { get; set; }

        public SagaConfiguration(Type type)
        {
            SagaType = type;
        }
    }
}