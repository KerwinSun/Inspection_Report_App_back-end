using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InspectionReport.Models
{
    public class EmailMessage
    {
        public EmailMessage()
        {
 
        }
        
        public EmailAddress to { get; set; }
        public EmailAddress from { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
    }
}
