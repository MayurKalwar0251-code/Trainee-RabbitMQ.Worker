
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
    public async Task GetFileMetaData(long docId, string messageId, CancellationToken cancellationToken)
    {
        Console.WriteLine("Param got : " + docId);

        // finding submission file id document
        var submissionFile = _context.SubmissionFiles.FirstOrDefault(s => s.Id == docId);
        // finding processingJob document
        var processingJob = _context.ProcessingJobs.FirstOrDefault(p => p.MessageId == messageId);

        var attemptCount = processingJob!.Attempts; 

        try
        {
            if (processingJob == null)
            {
                Console.WriteLine("Processing Job data not found");
                throw new MaxAttemptExeption(attemptCount,"Document not found");
            }

            // Detect if process is already completed to ensure idempotent
            if (processingJob.Status == "Completed")
            {
                Console.WriteLine("Already Processed Message");
                return;
            }

            processingJob.Status = "Processing";
            await _context.SaveChangesAsync();

            if (submissionFile == null)
            {
                Console.WriteLine("Document not found");
                throw new MaxAttemptExeption(attemptCount,"Document not found");
            }


            var filePath = Path.Combine(_configuration["StoredFilesPath"]!, submissionFile.GeneratedStorageName);

            Console.WriteLine("FIlepath : " + filePath);

            if (!_fileStorageService.Exists(filePath))
            {
                Console.WriteLine("File Not Exists");
                throw new MaxAttemptExeption(attemptCount,"File MetaData not found");
            }

            // Compute SHA256 Hash using streams (memory efficient)
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

            processingJob.Attempts += 1;
            processingJob.Status = "Completed";

            await _context.SaveChangesAsync(cancellationToken);

            Console.WriteLine("Service function completed successfully");
        }
        catch (MaxAttemptExeption e)
        {
            Console.WriteLine("Error IN CATCH MESSSAGE : " + e.AttemptCount + e.Message);
            processingJob!.ErrorSummary = e.Message;
            processingJob.Attempts = e.AttemptCount + 1;
            processingJob.Status = "Failed";
            await _context.SaveChangesAsync();
            throw new MaxAttemptExeption(e.AttemptCount + 1,e.Message);
        }
    }
}