using Volo.Abp;
using Volo.Abp.ExceptionHandling;

namespace GuguEveryday;

public class StatusCodeException : UserFriendlyException, IHasHttpStatusCode
{
    public int HttpStatusCode { get; }

    public StatusCodeException(string message, int statusCode = 400) : base(message)
    {
        HttpStatusCode = statusCode;
    }
}