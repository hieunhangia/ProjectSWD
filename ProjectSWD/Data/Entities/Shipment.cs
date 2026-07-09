using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ProjectSWD.Data.Enums;

namespace ProjectSWD.Data.Entities
{
    public class Shipment
    {
        [Key, ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int DeliveryPartnerId { get; set; }
        public DeliveryPartner DeliveryPartner { get; set; }

        public ShipmentStatus Status { get; set; }
    }
}
