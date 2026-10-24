namespace Szkola2.Models
{
    /// <summary>
    /// Encja, która może należeć do konta testowego (demo).
    /// null = prawdziwa treść szkoły, wartość = Id konta demo, które ją utworzyło.
    /// </summary>
    public interface IDemoOwned
    {
        string? DemoOwnerId { get; set; }
    }
}
