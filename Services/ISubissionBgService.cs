public interface ISubissionBgService
{
    Task GetFileMetaData(long docId, CancellationToken cancellationToken);
}