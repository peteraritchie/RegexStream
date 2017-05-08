using System;
using System.IO;
using PRI.FilteringStreams.RegexStream;
using Xunit;

namespace Tests
{
	public sealed class ProcessingNullStreamWithRegex : IDisposable
	{
		private readonly Stream _sourceStream;

		[Fact]
		public void ReplacementsThrows()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				using (var regexStream = new RegexStream(_sourceStream, "[0-9]+$", match =>
				{
					int number;
					return int.TryParse(match.Value, out number) ? number.ToString() : match.Value;
				}))
				{
				}
			});
		}

		public void Dispose()
		{
			_sourceStream?.Dispose();
		}
	}
}