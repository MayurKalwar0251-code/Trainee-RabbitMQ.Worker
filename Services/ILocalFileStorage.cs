public interface ILocalFileStorage
{
    Stream OpenReadStream(string filePath);

    bool Exists(string path);
}