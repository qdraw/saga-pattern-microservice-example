using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ACME.Library.Domain.Enums.Registration;
using ACME.Library.Domain.Interfaces.Saga;

namespace ACME.Library.Domain.Registration
{
    public class RegistrationData : ISagaData
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Functional link of the registration
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// State of the saga
        /// </summary>
        [JsonIgnore]
        public string State { get; set; }

        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public RegistrationStateType RegistrationState { get; set; }
        
        
        [NotMapped]
        public RegistrationActionType Action { get; set; }
    }
}