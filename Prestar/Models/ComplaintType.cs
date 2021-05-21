using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// Enumerated class of types of complaints. The types can be ReportClient 
    /// (type of complaint addressed to a customer, made by a service provider), 
    /// ReportServiceProvider (type of complaint addressed to a service provider, 
    /// made by a customer) and ReportService (type of complaint directed to a 
    /// service, made by a customer or service provider who at that moment 
    /// assumes the role of customer before another service provider).
    /// </summary>
    public enum ComplaintType
    {
        [Display(Name = "Denunciar Cliente")]
        ReportClient,
        [Display(Name = "Denunciar Prestador de Serviço")]
        ReportServiceProvider,
        [Display(Name = "Denunciar Serviço")]
        ReportService
    }
}
