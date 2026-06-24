namespace TrainineeAPI.Models;
public class SubmissionProcessingRequestModel
{
    public required string MessageId { get; set; }
    public required string CorrelationId { get; set; }
    public required long SubmissionId { get; set; }
    public required long SubmissionFileId { get; set; }
    public SubmissionFile? SubmissionFile { get; set; }
    public DateTime RequestedAt {get; set;}
    public string? ContractVersion {get; set;}
}