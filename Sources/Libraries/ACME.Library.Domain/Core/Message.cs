using System;
using ACME.Library.Domain.Enums;

namespace ACME.Library.Domain.Core
{
    public class Message
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public Guid? UserId { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public bool IsRead { get; set; }
        public Guid? FromId { get; set; }
        public MessageStateType State {get;set;}

    }
}
