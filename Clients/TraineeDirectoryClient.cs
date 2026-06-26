
using System.Net.Http.Json;
using TrainineeAPI.Models;

public class TraineeDirectoryClient : ITraineeDirectoryClient
{
    private readonly HttpClient _httpClient;

    public TraineeDirectoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public Task GetAssignmentAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Trainee?> GetTraineeAsync(int id)
    {
        try
        {
            var res = await _httpClient.GetAsync($"/trainee/{id}");

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Trainee : {id} not found");
                return null;
            }

            res.EnsureSuccessStatusCode();
          
            return await res.Content.ReadFromJsonAsync<Trainee>();
        }
        catch(HttpRequestException e)
        {
            Console.WriteLine("TraineeDirectory.Api service apis unreachable : " + e.Message);   
            throw new RetryableException("TraineeDirectory.Api service apis unreachable : " + e.Message);
        }
        catch (System.Exception e)
        {
            Console.WriteLine("Unexpected Error : " + e.Message);   
            throw new RetryableException("Unexpected Error : " + e.Message);
        }
    }
}