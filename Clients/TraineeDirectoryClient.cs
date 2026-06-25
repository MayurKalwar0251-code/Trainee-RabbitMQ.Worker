
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
        // catch(HttpRequestException)
        // {
        //     Console.WriteLine("Could not able to connect with TraineeDirectory.Api service apis");   
        //     throw new Exception("Could not able to connect with TraineeDirectory.Api service apis");
        // }
        catch (System.Exception)
        {
            Console.WriteLine("Something went wrong while calling TraineeDirectory.Api service apis");   
            throw new Exception("Something went wrong while calling TraineeDirectory.Api service apis");
        }
    }
}