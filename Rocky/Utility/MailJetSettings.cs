using System;
namespace Rocky.Utility
{
    public class MailJetSettings
    {
        public string apiKey { get; set; }
        public string secretKey { get; set; }
        public string fromEmail { get; set; }
        public string fromName { get; set; }
    }
}
