﻿namespace FMB_CIS.Models
{
    public class tbl_user_types
    {
        public int id { get; set; }
        public int tbl_user_id { get; set; }
        public string name { get; set; }
        public bool? is_active { get; set; }
        public int created_by { get; set; }
        public int modified_by { get; set; }
        public DateTime? date_created { get; set; }
        public DateTime? date_modified { get; set; }
    }
}
