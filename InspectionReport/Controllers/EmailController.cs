using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InspectionReport.Models;
using InspectionReport.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InspectionReport.Controllers
{
    [Route("api/emailTest")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _mailer;

        public EmailController(IEmailService eservice)
        {
            _mailer = eservice;
        }

        [HttpGet]
        public IActionResult sendEmail()
        {
            EmailAddress clientEmail = new EmailAddress("William", "chao_mix@hotmail.com");
            EmailAddress hitchEmail = new EmailAddress("Hitch Building Inspections", "hitchinspectionz@gmail.com");
            EmailMessage emessage = new EmailMessage();
            emessage.to = clientEmail;
            emessage.from = hitchEmail;
            emessage.subject = "Test - Inspection Report";
            emessage.content = "Your inspection has been completed";
            _mailer.Send(emessage);
            return Ok();
        }
    }
}