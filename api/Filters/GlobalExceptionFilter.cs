using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Volo.Abp;

namespace GuguEveryday.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "服务器内部错误";

        switch (context.Exception)
        {
            case StatusCodeException scEx:
                statusCode = (HttpStatusCode)scEx.HttpStatusCode;
                message = scEx.Message;
                break;
            case UserFriendlyException ufEx:
                statusCode = HttpStatusCode.BadRequest;
                message = ufEx.Message;
                break;
            case Volo.Abp.Authorization.AbpAuthorizationException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "未授权";
                break;
            case Volo.Abp.Domain.Entities.EntityNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = context.Exception.Message;
                break;
            default:
                _logger.LogError(context.Exception, "未处理的异常");
                break;
        }

        context.Result = new ObjectResult(new
        {
            error = new { message }
        })
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
    }
}
