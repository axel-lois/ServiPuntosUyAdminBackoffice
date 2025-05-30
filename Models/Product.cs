namespace ServiPuntosUyAdmin.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public bool AgeRestricted { get; set; }
    }
}
