using Volo.Abp;
using Volo.Abp.ExceptionHandling;

namespace GuguEveryday;

public class StatusCodeException : UserFriendlyException, IHasHttpStatusCode
{
    public int HttpStatusCode { get; }
    public int? RetryAfterSeconds { get; }

    public StatusCodeException(string message, int statusCode = 400, int? retryAfterSeconds = null) : base(message)
    {
        HttpStatusCode = statusCode;
        RetryAfterSeconds = retryAfterSeconds;
    }
}