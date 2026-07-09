using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class DeliveryPartner
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(255)]
        public string Email { get; set; }
        
        [MaxLength(20)]
        public string Phone { get; set; }

        public ICollection<Shipment> Shipments { get; set; }
    }
}
