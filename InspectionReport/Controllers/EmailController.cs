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
    [Route("api/email/[action]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        [ActionName("testMsg")]
        public async Task<IActionResult> sendTextMessage()
        {
            EmailAddress toEmail = new EmailAddress("Client", "hitchinspectionz@gmail.com");
            EmailMessage msg = new EmailMessage(toEmail);
            msg.subject = "Test - Inspection Report";
            msg.body = "Hello world your inspection has been completed";
            await _emailService.SendAsync(msg);
            return new OkResult();
        }

    }
}
