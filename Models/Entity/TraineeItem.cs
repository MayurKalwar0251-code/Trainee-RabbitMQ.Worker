namespace TrainineeAPI.Models;

public enum TraineeStatusEnumValues {Active, Inactive, Completed}

public class Trainee
{
    public long Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string TechStack { get; set; }
    public required string Status { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateOnly UpdatedDate { get; set; }
    
}