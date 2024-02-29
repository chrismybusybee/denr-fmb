namespace FMB_CIS.Data
{
    public class SMTPCredentials
    {       
        public string Host_Address { get; set; }
        public int Host_Port { get; set; }
        public string Host_Username { get; set; }
        public string Host_Password { get; set; }
        public string Sender_EMail { get; set; }
        public string Sender_Name { get; set; }
    }
}
