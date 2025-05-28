// Models/TenantListResponse.cs
using System.Collections.Generic;

namespace ServiPuntosUyAdmin.Models
{
    public class TenantListResponse
    {
        public bool Error { get; set; }
        public List<Tenant> Data { get; set; }
        public string Message { get; set; }
    }
}
