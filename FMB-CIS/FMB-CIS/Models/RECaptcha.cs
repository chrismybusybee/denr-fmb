using FMB_CIS.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;

namespace FMB_CIS.Models
{
    public class RECaptcha
    {

        //FOR fmb-cis.beesuite.ph reCaptcha V2
        public string Key = "6LfQKXkoAAAAABwcL6avWNOL0tUncx8sxrBEcrEP";

        public string Secret = "6LfQKXkoAAAAABzov9v-K8yknc9jF8JsNrEF2Kt-";

        //FOR LOCAL HOST v2
        //public string Key = "6LcA9HwoAAAAAO5aj85XNqV0vhunAerBF--e_v-F";

        //public string Secret = "6LcA9HwoAAAAAIUBqt61CAjOt0HWrqZpRBw3U5Uc";
        public string Response { get; set; }
    }
}
