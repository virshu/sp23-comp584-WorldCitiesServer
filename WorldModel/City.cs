using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorldModel;

public partial class City
{
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Lat { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Lon { get; set; }

    public int Population { get; set; }

    public int CountryId { get; set; }

    public virtual Country Country { get; set; } = null!;
}
