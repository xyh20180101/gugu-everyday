using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuguEveryday.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GuguEveryday.Tests;

[TestClass]
public class GlobalExceptionFilterTests
{
    private readonly ILogger<GlobalExceptionFilter> _logger = Substitute.For<ILogger<GlobalExceptionFilter>>();
    private readonly GlobalExceptionFilter _sut;

    public GlobalExceptionFilterTests()
    {
        _sut = new GlobalExceptionFilter(_logger);
    }

    private ExceptionContext CreateContext(Exception exception)
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    [TestMethod]
    public void OnException_StatusCodeException_ReturnsCustomStatusCode()
    {
        var context = CreateContext(new StatusCodeException("业务错误", 422));

        _sut.OnException(context);

        Assert.IsTrue(context.ExceptionHandled);
        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(422, result!.StatusCode);
    }

    [TestMethod]
    public void OnException_StatusCodeException_ReturnsErrorMessage()
    {
        var context = CreateContext(new StatusCodeException("登录名已存在", 409));

        _sut.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result!.Value, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        Assert.IsTrue(json.Contains("登录名已存在"), $"JSON was: {json}");
    }

    [TestMethod]
    public void OnException_GenericException_Returns500()
    {
        var context = CreateContext(new InvalidOperationException("something broke"));

        _sut.OnException(context);

        Assert.IsTrue(context.ExceptionHandled);
        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result!.StatusCode);
    }

    [TestMethod]
    public void OnException_GenericException_DoesNotLeakMessage()
    {
        var context = CreateContext(new InvalidOperationException("internal secret details"));

        _sut.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        var json = System.Text.Json.JsonSerializer.Serialize(result!.Value, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = null, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        Assert.IsFalse(json.Contains("internal secret details"));
        Assert.IsTrue(json.Contains("服务器内部错误"), $"JSON was: {json}");
    }

    [TestMethod]
    public void OnException_EntityNotFoundException_Returns404()
    {
        var context = CreateContext(new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Models.User), 42));

        _sut.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result!.StatusCode);
    }

    [TestMethod]
    public void OnException_AuthorizationException_Returns401()
    {
        var context = CreateContext(new Volo.Abp.Authorization.AbpAuthorizationException());

        _sut.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(401, result!.StatusCode);
    }

    [TestMethod]
    public void OnException_StatusCodeException_DoesNotLogError()
    {
        var context = CreateContext(new StatusCodeException("user error"));

        _sut.OnException(context);

        _logger.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [TestMethod]
    public void OnException_UnknownException_LogsError()
    {
        var context = CreateContext(new Exception("unexpected"));

        _sut.OnException(context);

        _logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Is<Exception>(e => e.Message == "unexpected"),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
