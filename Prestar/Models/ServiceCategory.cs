using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Prestar.Models
{
    /// <summary>
    /// This class serves as a model for the category of a service, which has a 
    /// name, a list of subcategories, an image and a variable that controls 
    /// whether the category is a subcategory.
    /// </summary>
    public class ServiceCategory
    {
        /// <summary>
        /// Category identification
        /// </summary>
        [Key]
        public int CategoryID { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [Required(ErrorMessage ="Este campo é de preenchimento obrigatório")]
        [Display(Name = "Categoria")]
        [StringLength(100, ErrorMessage = "O {0} deve conter no máximo {1} caracteres.")]
        public string Name { get; set; }

        /// <summary>
        /// List of subcategories
        /// </summary>
        [Display(Name = "Subcategorias")]
        public List<ServiceCategory> ServiceCategories { get; set; }

        /// <summary>
        /// Category image
        /// </summary>
        [Display(Name = "Imagem da Categoria")]
        public byte[] Illustration { get; set; }

        /// <summary>
        /// Checks if category is subcategory
        /// </summary>
        public bool IsSubcategory { get; set; }

        /// <summary>
        /// Returns the subcategories as a string
        /// </summary>
        public string Subcategories
        {
            get
            {
                if (ServiceCategories == null || ServiceCategories.Count == 0) {
                    return "-";
                }
                string subcategories = "";
                for (int i = 0; i < ServiceCategories.Count; i++)
                {
                    subcategories += ServiceCategories[i].Name;
                    if (i < ServiceCategories.Count - 1)
                    {
                        subcategories += ", ";
                    }
                }
                return subcategories;
            }
        }
    }
}
