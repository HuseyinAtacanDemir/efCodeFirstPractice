using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class Category
    {

        [Key]

        public int Id { get; set; }

        //make this a required field, using built in validation of ef
        [Required]
        public string Name { get; set; }

        //name to be displayed about the field in a view when asp-for is equals to the internal field name (asp-for="DisplayOrder")
        [DisplayName("Display Order")]
        [Required]
        [Range(1,int.MaxValue, ErrorMessage = "Display Order for Category must be greater than zero!")]
        public int DisplayOrder { get; set; }

        
    }
}
