using System;
using System.Runtime.Serialization;

namespace CoreWiki.Application.Articles.Managing.Exceptions;

[Serializable]
internal class CreateArticleException : Exception
{
	public CreateArticleException()
	{
	}

	public CreateArticleException(string message) : base(message)
	{
		//Log error
	}

	public CreateArticleException(string message, Exception innerException) : base(message, innerException)
	{
	}

	// Removed due to the following warning:
	// SYSLIB0051: Constructor 'System.Exception.Exception(SerializationInfo, StreamingContext)' is obsolete:
	// 'This API supports obsolete formatter-based serialization. It should not be called or extended by application code.'
	//protected CreateArticleException(SerializationInfo info, StreamingContext context) : base(info, context)
	//{
	//}
}
