using System;

namespace ACME.Library.Common.Models.Message
{
    public class Notification
    {
        public Guid? Id { get; set; }
        public string Message { get; set; }
    }
}
