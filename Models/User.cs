namespace UserManagementAPI;

public record User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}