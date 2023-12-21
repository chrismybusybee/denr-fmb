namespace FMB_CIS.Models
{
    //public class tbl_application_group
    //{
    //    public int ID { get; set; }
    //    public int? ApplicationID { get; set; }
    //    public string SupplierName { get; set; }
    //    public string SupplierAddress { get; set; }
    //    public DateTime? ExpectedTimeArrival { get; set; }
    //    public string PowerSource { get; set; }
    //    public string UnitOfMeasure { get; set; }
    //    public string Brand { get; set; }
    //    public string Model { get; set; }
    //    public string EngineSerialNo { get; set; }
    //    public int? Quantity { get; set; }
    //    public int? CreatedBy { get; set; }
    //    public int? ModifiedBy { get; set; }
    //    public DateTime? DateCreated { get; set; }
    //    public DateTime? DateModified { get; set; }

    //}


    public class tbl_application_group
    {
        public int id { get; set; }
        public int? tbl_application_id { get; set; }
        public string supplier_name { get; set; }
        public string supplier_address { get; set; }
        public DateTime? expected_time_arrival { get; set; }
        public string power_source { get; set; }
        public string unit_of_measure { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public string engine_serialNo { get; set; }
        public int? quantity { get; set; }
        public int? created_by { get; set; }
        public int? modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
