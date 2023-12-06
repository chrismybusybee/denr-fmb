using System.ComponentModel.DataAnnotations;

namespace FMB_CIS.Models
{
    public class WorkflowListViewModel
    {
        public IEnumerable<Workflow> workflows { get; set; }
    }
}
