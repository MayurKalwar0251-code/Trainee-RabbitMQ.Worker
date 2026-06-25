public class MaxAttemptExeption(int count, string message, bool retryReque = false) : Exception(message)
{
    public int AttemptCount {get;} = count;
    public bool RetryReque {get;set;} = retryReque;
}