namespace ProcesoETL.Models;

/// <summary>
/// Comment model from REST API
/// </summary>
public class Comment
{
    public int CommentID { get; set; }
    public int OrderID { get; set; }
    public int CustomerID { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Status { get; set; }
}

