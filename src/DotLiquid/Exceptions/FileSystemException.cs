using System;

namespace DotLiquid.Exceptions
{
#if !NETCore
    [Serializable]
#endif
    public class FileSystemException : LiquidException
	{
		public FileSystemException(string message, params string[] args)
			: base(string.Format(message, args))
		{
		}
	}
}