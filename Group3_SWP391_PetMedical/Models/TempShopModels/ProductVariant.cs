using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Group3_SWP391_PetMedical.Models.TempShopModels;

public partial class ProductVariant
{
    [Key]
    [Column("variant_id")]
    public int VariantId { get; set; }

    [Column("product_id")]
    public int ProductId { get; set; }

    [Column("variant_name")]
    [StringLength(255)]
    public string VariantName { get; set; } = null!;

    [Column("color")]
    [StringLength(50)]
    public string? Color { get; set; }

    [Column("size")]
    [StringLength(50)]
    public string? Size { get; set; }

    [Column("material")]
    [StringLength(100)]
    public string? Material { get; set; }

    [Column("sku")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Sku { get; set; }

    [Column("price_override", TypeName = "decimal(18, 2)")]
    public decimal? PriceOverride { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("status")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("image_url")]
    [StringLength(255)]
    [Unicode(false)]
    public string? ImageUrl { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductVariants")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("Variant")]
    public virtual ICollection<ProductOrderItem> ProductOrderItems { get; set; } = new List<ProductOrderItem>();
}
