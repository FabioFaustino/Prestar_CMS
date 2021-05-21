using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Models
{
    /// <summary>
    /// This class is an enumeration of the user's order types. The types are the 
    /// account change order (ProvideServices) and the order to add a category 
    /// (AddCategory).
    /// </summary>
    public enum RequestType
    {
        [Display(Name = "Prestar Serviços")]
        ProvideServices ,
        [Display(Name = "Adicionar Categoria")]
        AddCategory
    }
}
