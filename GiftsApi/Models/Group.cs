namespace GiftsApi.Models;

public class Group
{
    public Guid Id {get; init;}
    public string Name { get; set; }
    public string Password {get; set;}
    public Guid[] Members { get; set; }
}