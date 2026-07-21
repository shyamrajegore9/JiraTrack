namespace JiraTrack.BusinessLogic;

public class BusinessException : Exception
{
    public int StatusCode { get; }

    public BusinessException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public class UnauthorizedBusinessException : BusinessException
{
    public UnauthorizedBusinessException(string message) : base(message, 401) { }
}

public class ForbiddenBusinessException : BusinessException
{
    public ForbiddenBusinessException(string message) : base(message, 403) { }
}
