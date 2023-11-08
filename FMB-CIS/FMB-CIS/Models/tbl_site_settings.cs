namespace FMB_CIS.Models
{
    public class tbl_site_settings
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string value { get; set; } // true, false, list, regex... allows system flexibility / maintainability, minimizes code edit
        public string scope { get; set; } // Page, Module, Functionality
    }
}
