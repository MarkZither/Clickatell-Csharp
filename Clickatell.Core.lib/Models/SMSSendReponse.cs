using System;
using System.Collections.Generic;
using System.Text;

namespace Clickatell.Core.lib.Models
{
    class SMSSendReponse
    {

        public Message[] messages { get; set; }
        public object error { get; set; }
    }

    public class Message
    {
        public string apiMessageId { get; set; }
        public bool accepted { get; set; }
        public string to { get; set; }
        public object error { get; set; }
    }
}
