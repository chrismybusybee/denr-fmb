namespace FMB_CIS.Models
{
    public class PermitProgressStatusModel
    {
        //public int Id { get; set; }
        public int Progress { get; set; }
        public string ApplicationStatusCode { get; set; }
        public bool isHappyPath { get; set; }
    }
}
