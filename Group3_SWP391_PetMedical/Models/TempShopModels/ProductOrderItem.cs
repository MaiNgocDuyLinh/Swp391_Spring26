using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Models.TempShopModels;

public partial class ProductOrderItem
{
    [Key]
    [Column("order_item_id")]
    public int OrderItemId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("variant_id")]
    public int? VariantId { get; set; }

    [Column("product_name")]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Column("variant_name")]
    [StringLength(255)]
    public string? VariantName { get; set; }

    [Column("unit_price", TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [Column("quantity")]
    public int Quantity { get; set; }

    [Column("line_total", TypeName = "decimal(18, 2)")]
    public decimal LineTotal { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("ProductOrderItems")]
    public virtual ProductOrder Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("ProductOrderItems")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("ProductOrderItems")]
    public virtual ProductVariant? Variant { get; set; }
}
