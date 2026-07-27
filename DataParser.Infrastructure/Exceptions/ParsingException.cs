namespace DataParser.Infrastructure.Exceptions;

public class ParsingException : BaseException
{
	public ParsingException() : base() { }

	public ParsingException(string message) : base(message) { }

	public ParsingException(string message, Exception innerException) : base(message, innerException) { }
}
