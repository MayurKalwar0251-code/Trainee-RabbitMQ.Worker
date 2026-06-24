namespace TrainineeAPI.Models;

public enum ProcessingJobEnumValues {Queued, Processing, Completed, Failed};
public class ProcessingJob
{
    public long Id { get; set; }
    public int Attempts { get; set; }
    public required string CorrelationId { get; set; }
    public required long SubmissionFileId {get; set;}
    public required string MessageId { get; set; }
    public string? ErrorSummary { get; set; }
    public required string Status { get; set; }
    public DateTime StartedTime { get; set; }
    public DateTime CompletedTime { get; set; }
}