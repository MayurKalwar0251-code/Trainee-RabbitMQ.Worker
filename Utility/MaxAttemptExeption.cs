public class MaxAttemptExeption(int count, string message) : Exception(message)
{
    public int AttemptCount {get;} = count;
}