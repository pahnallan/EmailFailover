using System;
using System.Collections.Generic;
using System.Text;

namespace EmailFailOverLambda.Models
{
    public class EmailApiRequest : IEmailApiRequest
    {
        public string To { get; set; }
        public string ToName { get; set; }
        public string From { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
