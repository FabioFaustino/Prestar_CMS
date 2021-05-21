using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class is an enumeration of the status of the account change request 
    /// and the request to add a new subcategory. States are waiting for approval 
    /// (WaitingApproval), under analysis (Analyzing), approved (Approved) and 
    /// rejected (Rejected).
    /// </summary>
    public enum RequestStatus
    {
        [Display(Name = "Espera Aprovação")]
        WaitingApproval,
        [Display(Name = "Em análise")]
        Analyzing,
        [Display(Name = "Aprovado")]
        Aproved,
        [Display(Name = "Rejeitado")]
        Rejected
    }
}
