using System.Collections.Generic;
using ServiPuntosUyAdmin.Models;

namespace ServiPuntosUyAdmin.Models
{
    public class StationListResponse
    {
        public bool Error { get; set; }
        public List<Station> Data { get; set; }
        public string Message { get; set; }
    }
}