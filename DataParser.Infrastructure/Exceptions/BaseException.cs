namespace DataParser.Infrastructure.Exceptions;

public class BaseException : Exception
{
	protected BaseException() : base() { }

	protected BaseException(string message) : base(message) { }

	protected BaseException(string message, Exception innerException) : base(message, innerException) { }
}
