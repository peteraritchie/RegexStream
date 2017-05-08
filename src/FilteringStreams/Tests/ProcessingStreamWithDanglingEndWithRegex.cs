using System;
using System.IO;
using System.Text;
using PRI.FilteringStreams.RegexStream;
using Xunit;

namespace Tests
{
	public sealed class ProcessingStreamWithDanglingEndWithRegex : IDisposable
	{
		private readonly Stream _sourceStream;

		public ProcessingStreamWithDanglingEndWithRegex()
		{
			var builder = new StringBuilder();
			builder.AppendLine("line 1");
			builder.AppendLine("line 2");
			builder.Append("line 3");
			_sourceStream = new MemoryStream(Encoding.ASCII.GetBytes(builder.ToString()));
		}

		[Fact]
		public void ReplacementsSucceed()
		{
			using (var regexStream = new RegexStream(_sourceStream, "[0-9]+$", match =>
			{
				int number;
				return int.TryParse(match.Value, out number) ? ToEnglishString(number) : match.Value;
			}))
			{
				var reader = new StreamReader(regexStream);
				var text = reader.ReadToEnd();
				var expected = @"line one
line two
line three";
				Assert.Equal(expected, text);
			}
		}

		public void Dispose()
		{
			_sourceStream?.Dispose();
		}

		// TODO: [ExcludeFromCodeCoverage]
		public static string ToEnglishString(int number)
		{
			switch (number)
			{
				case 1: return "one";
				case 2: return "two";
				case 3: return "three";
				case 4: return "four";
				default: throw new NotImplementedException();
			}
		}
	}
}