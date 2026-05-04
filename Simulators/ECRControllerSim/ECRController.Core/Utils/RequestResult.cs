using System.Net;

namespace ECRController.Core.Utils;

public class RequestResult
{
    public bool IsSuccess { get; }
    public HttpStatusCode Status { get; }
    public string Content { get; }

    internal RequestResult(bool ok, HttpStatusCode status, string content)
    {
        IsSuccess = ok;
        Status = status;
        Content = content;
    }

    internal static RequestResult Success(HttpStatusCode status, string content) =>
        new RequestResult(true, status, content);

    internal static RequestResult Failure(HttpStatusCode status, string content) =>
        new RequestResult(false, status, content);
}

public class RequestResult<T> : RequestResult
{
    public T Data { get; }

    private RequestResult(bool ok, HttpStatusCode status, string content, T data)
        : base(ok, status, content)
    {
        Data = data;
    }

    internal static RequestResult<T> Success(HttpStatusCode status, T data, string content) =>
        new RequestResult<T>(true, status, content, data);

    internal new static RequestResult<T> Failure(HttpStatusCode status, string content) =>
        new RequestResult<T>(false, status, content, default);
}