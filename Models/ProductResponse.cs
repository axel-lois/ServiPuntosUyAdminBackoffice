using System.Collections.Generic;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Models
{
    public class ProductResponse
    {
        public bool Error { get; set; }
        public Product Data { get; set; }
        public string Message { get; set; }
    }
}