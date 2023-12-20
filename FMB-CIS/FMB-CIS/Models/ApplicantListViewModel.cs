using Elfie.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace FMB_CIS.Models
{
    public class ApplicantListViewModel
    {
        public int? id { get; set; }
        public tbl_application application { get; set; }
        public string application_type { get; set; }
        public string permit_status { get; set; }
        public string permit_type { get; set; }
        public int status { get; set; }
        public string full_name { get; set; }
        public string first_name { get; set; }
        public string middle_name { get; set; }
        public string last_name { get; set; }
        public string suffix { get; set; }
        public string full_address { get; set; }
        public string? email { get; set; }
        public string contact { get; set; }
        public string address { get; set; }


        public int tbl_user_id { get; set; }
        public string user_type { get; set; }
        public string valid_id { get; set; }
        public string valid_id_no { get; set; }
        public string birth_date { get; set; }
        public int tbl_region_id { get; set; }
        public int tbl_province_id { get; set; }
        public int tbl_city_id { get; set; }
        public int tbl_brgy_id { get; set; }
        //public string street_address { get; set; }
        public string region { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string brgy { get; set; }
        public string comment { get; set; }
        public int tbl_user_types_id { get; set; }
        public string user_classification { get; set; }
        public string company_name { get; set; }

        public int? qty { get; set; }
        public string specification { get; set; }
        
        public string purpose { get; set; }
        public DateTime? initial_date_of_inspection { get; set; } //Initial Date of Inspection
        public DateTime? inspectionDate { get; set; } //Actual Date of Inspecton
        public DateTime? expectedTimeArrived { get; set; }
        public DateTime? expectedTimeRelease { get; set; }
        public FileUpload? filesUpload { get; set; }

        public DateTime? applicationDate { get; set; }
        public DateTime? date_of_registration { get; set; }
        public DateTime? date_of_expiration { get; set; }
        public DateTime? date_due_for_officers { get; set; }
        public bool? coordinatedWithEnforcementDivision { get; set; }
        public int? renew_from { get;set; }

        //FOR CHAINSAW REGISTRATION APPROVAL
        public string? chainsawBrand { get; set; }
        public string? chainsawModel { get; set; }
        public string? Engine { get; set; }
        public string? powerSource { get; set; }
        public int? Watt { get; set; }
        public int? hp { get; set; }
        public string? gb { get; set; }
        public string? chainsaw_serial_number { get; set; }
        public string? chainsawSupplier { get; set; }
        public DateTime? date_purchase { get; set; }
        //FOR ApplicantList
        public bool? isRead { get; set; }

    }
}
