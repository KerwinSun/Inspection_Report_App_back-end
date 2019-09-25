using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class EmailMessage
    {
        public EmailMessage(EmailAddress _to)
        {
            to = _to;
        }
        
        public EmailAddress to { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public byte[] file { get; set; }
        public string fname { get; set; }
    }
}
