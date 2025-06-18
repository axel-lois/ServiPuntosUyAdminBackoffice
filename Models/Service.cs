using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServiPuntosUyAdmin.Models
{
    public class ServiceListResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        
        [JsonPropertyName("data")]
        public List<Service> Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class Service
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        // <-- PARA CREATE Y EDIT VIEWS
        [JsonIgnore] 
        public int BranchId { get; set; }

        [JsonIgnore] 
        public TimeSpan StartTime { get; set; }

        [JsonIgnore] 
        public TimeSpan EndTime { get; set; }

        // <-- LO QUE YA TENÍAS
        [JsonPropertyName("tenantId")]
        public int TenantId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("ageRestricted")]
        public bool AgeRestricted { get; set; }

        [JsonPropertyName("availabilities")]
        public List<Availability> Availabilities { get; set; }
    }

    public class Availability
    {
        [JsonPropertyName("startTime")]
        public TimeSpan StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public TimeSpan EndTime { get; set; }

        // …otros campos si los necesitas
    }
}
