using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Models.TempShopModels;

[Index("OrderCode", Name = "UQ_ProductOrders_OrderCode", IsUnique = true)]
public partial class ProductOrder
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("order_code")]
    [StringLength(50)]
    [Unicode(false)]
    public string OrderCode { get; set; } = null!;

    [Column("total_amount", TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [Column("payment_method")]
    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column("payment_status")]
    [StringLength(30)]
    public string PaymentStatus { get; set; } = null!;

    [Column("order_status")]
    [StringLength(30)]
    public string OrderStatus { get; set; } = null!;

    [Column("pickup_method")]
    [StringLength(50)]
    public string PickupMethod { get; set; } = null!;

    [Column("pickup_note")]
    [StringLength(500)]
    public string? PickupNote { get; set; }

    [Column("pickup_date", TypeName = "datetime")]
    public DateTime? PickupDate { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<ProductOrderItem> ProductOrderItems { get; set; } = new List<ProductOrderItem>();
}
