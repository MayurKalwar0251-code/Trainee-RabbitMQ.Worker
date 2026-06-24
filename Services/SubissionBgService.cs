
using System.Security.Cryptography;
using System.Text.Json;

public class SubissionBgService : ISubissionBgService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILocalFileStorage _fileStorageService;
    public SubissionBgService(AppDbContext context, IConfiguration configuration, ILocalFileStorage localFileStorage)
    {
        _context = context;
        _configuration = configuration;
        _fileStorageService = localFileStorage;
    }
    public async Task GetFileMetaData(long docId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Param got : " + docId);

        // finding submission file id document
        var submissionFile = _context.SubmissionFiles.FirstOrDefault(s => s.Id == docId);

        var serialize = JsonSerializer.Serialize(submissionFile);

        Console.WriteLine("Founded doc : " + serialize);

        if (submissionFile == null)
        {
            Console.WriteLine("Document not found");
            throw new Exception();
        }

        var filePath = Path.Combine(_configuration["StoredFilesPath"]!, submissionFile.GeneratedStorageName);

        Console.WriteLine("FIlepath : " + filePath);

        if (!_fileStorageService.Exists(filePath))
        {  
            Console.WriteLine("File Not Exists");
            throw new Exception();
        }

        // 3. Compute SHA256 Hash using streams (memory efficient)
        string checksum = "";
        using (var sha256 = SHA256.Create())
        {
            // Open the file as a stream via our service
            await using (var fileStream = _fileStorageService.OpenReadStream(filePath))
            {
                // Computes hash smoothly over chunks of data without loading whole file to RAM
                byte[] hashBytes = await sha256.ComputeHashAsync(fileStream, cancellationToken);
                checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        Console.WriteLine("CHECKSUM : " + checksum);

        submissionFile.Checksum = checksum;

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine("Service function completed successfully");
    }
}