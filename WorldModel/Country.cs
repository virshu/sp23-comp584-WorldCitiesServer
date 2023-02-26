using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WorldModel;

public partial class Country
{
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(2)]
    [Unicode(false)]
    public string Iso2 { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string Iso3 { get; set; } = null!;

    public virtual ICollection<City> Cities { get; } = new List<City>();
}
