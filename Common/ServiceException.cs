namespace Common
{
    public class ServiceException : Exception
    {
        public string Reason { get; }

        public ServiceException(string message, string reason) : base(message)
        {
            Reason = reason;
        }
    }
} 