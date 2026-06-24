using System.Security.Cryptography;
using System.Threading.Tasks;
using TrainineeAPI.Models;

public class LocalFileStorage : ILocalFileStorage
{
    private readonly IConfiguration _configuration;
    public LocalFileStorage(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public Stream OpenReadStream(string filePath)
    {
        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync : true);
    }
}