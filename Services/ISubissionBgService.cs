public interface ISubissionBgService
{
    Task GetFileMetaData(long docId, string messageId, CancellationToken cancellationToken);
}