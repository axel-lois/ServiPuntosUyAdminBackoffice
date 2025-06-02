using System.Collections.Generic;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Models
{
    public class ProductListResponse
    {
        public bool Error { get; set; }
        public List<Product> Data { get; set; }
        public string Message { get; set; }
    }
}