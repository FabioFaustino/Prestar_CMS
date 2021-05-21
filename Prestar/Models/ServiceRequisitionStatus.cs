using System.ComponentModel.DataAnnotations;

namespace Prestar.Models
{
    /// <summary>
    /// This class is an enumeration of the state of a service requesition. 
    /// The status can be pending (Pending), accepted (Accepted), 
    /// completed (Concluded), rejected (Rejected) and canceled (Canceled).
    /// </summary>
    public enum ServiceRequisitionStatus
    {
        [Display(Name = "Pedido Pendente")] Pending,
        [Display(Name = "Pedido Aceite")] Accepted,
        [Display(Name = "Pedido Concluido")] Concluded,
        [Display(Name = "Pedido Rejeitado")] Rejected,
        [Display(Name = "Pedido Cancelado")] Cancelled
    }
}
