using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rocky.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; }

        public IEnumerable<SelectListItem> CategorySelectlist { get; set; }

        public IEnumerable<SelectListItem> ApplicationTypeSelectlist { get; set; }
    }
}
