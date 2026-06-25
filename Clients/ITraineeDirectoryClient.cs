using TrainineeAPI.Models;

public interface ITraineeDirectoryClient
{
    Task<Trainee?> GetTraineeAsync(int id);

    Task GetAssignmentAsync(int id);
}