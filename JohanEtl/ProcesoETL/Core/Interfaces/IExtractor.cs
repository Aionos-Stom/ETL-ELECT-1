namespace ProcesoETL.Core.Interfaces;

/// <summary>
/// Interface for data extraction from different sources
/// </summary>
/// <typeparam name="T">The type of data to extract</typeparam>
public interface IExtractor<T>
{
    /// <summary>
    /// Extracts data from the source asynchronously
    /// </summary>
    /// <returns>A collection of extracted data</returns>
    Task<IEnumerable<T>> ExtractAsync();
    
    /// <summary>
    /// Gets the name of the extractor for logging purposes
    /// </summary>
    string Name { get; }
}
