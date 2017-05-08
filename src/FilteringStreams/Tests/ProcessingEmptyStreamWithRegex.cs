using System;
using System.IO;
using PRI.FilteringStreams.RegexStream;
using Xunit;

namespace Tests
{
	public sealed class ProcessingEmptyStreamWithRegex : IDisposable
	{
		private readonly Stream _sourceStream;

		public ProcessingEmptyStreamWithRegex()
		{
			_sourceStream = new MemoryStream();
		}

		[Fact]
		public void ReplacementsDoesNotThrow()
		{
			using (var regexStream = new RegexStream(_sourceStream, "[0-9]+$", match =>
			{
				int number;
				return int.TryParse(match.Value, out number) ? number.ToString() : match.Value;
			}))
			{
				var reader = new StreamReader(regexStream);
				var text = reader.ReadToEnd();
				Assert.Equal(string.Empty, text);
			}
		}
		public void Dispose()
		{
			_sourceStream?.Dispose();
		}
	}
}