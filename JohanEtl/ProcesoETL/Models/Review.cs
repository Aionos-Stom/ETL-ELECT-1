namespace ProcesoETL.Models;

/// <summary>
/// Review model from relational database
/// </summary>
public class Review
{
    public int ReviewID { get; set; }
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public int ProductID { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime ReviewDate { get; set; }
}

