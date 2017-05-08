using System;
using System.IO;
using System.Text;
using PRI.FilteringStreams.RegexStream;
using Xunit;
// TODO: using PRI.ProductivityExtensions.StreamExtensions;

namespace Tests
{
	public sealed class ProcessingStreamWithRegex : IDisposable // TODO: empty stream, stream with no line feeds,
	{
		private readonly Stream _sourceStream;

		public ProcessingStreamWithRegex()
		{
			var builder = new StringBuilder();
			builder.AppendLine("line 1");
			builder.AppendLine("line 2");
			builder.AppendLine("line 3");
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
line three
";
				Assert.Equal(expected, text);
			}
		}

		[Fact]
		public void ReadOneByteFromOneByteStreamSucceeds()
		{
			using (var regexStream = new RegexStream(new MemoryStream(new byte[]{(int)' '}), "[0-9]+$", match =>
			{
				int number;
				return int.TryParse(match.Value, out number) ? ToEnglishString(number) : match.Value;
			}))
			{
				byte[] buffer = new byte[1];
				// TODO: var bytesRead = regexStream.Read(buffer);
				var bytesRead = regexStream.Read(buffer, 0, buffer.Length);
				Assert.Equal(1, bytesRead);
				Assert.Equal(32, buffer[0]);
			}
		}

		[Fact]
		public void NullRegexThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new RegexStream(_sourceStream, null, match => match.Value));
		}

		[Fact]
		public void NullMatchThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new RegexStream(_sourceStream, "", null));
		}

		[Fact]
		public void NullEncodingThrows()
		{
			Assert.Throws<ArgumentNullException>(() => new RegexStream(_sourceStream, "", match => match.Value, null));
		}

		[Fact]
		public void CanSeekSucceeds()
		{
			using (var stream = new RegexStream(_sourceStream, "", match => match.Value))
			{
				Assert.Equal(true, stream.CanSeek);
			}
		}

		[Fact]
		public void CanWriteSucceeds()
		{
			using (var stream = new RegexStream(_sourceStream, "", match => match.Value))
			{
				Assert.Equal(true, stream.CanWrite);
			}
		}

		[Fact]
		public void LengthSucceeds()
		{
			using (var stream = new RegexStream(_sourceStream, "", match => match.Value))
			{
				Assert.Equal(24, stream.Length);
			}
		}

		[Fact]
		public void LengthOfEmptyStreamSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(), "", match => match.Value))
			{
				Assert.Equal(0, stream.Length);
			}
		}

		[Fact]
		public void SeekSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(new byte[]{1,2}), "", match => match.Value))
			{
				stream.Seek(1, SeekOrigin.Current);
				byte[] buffer = new byte[1];
				// TODO: var bytesRead = stream.Read(buffer);
				var bytesRead = stream.Read(buffer, 0, buffer.Length);
				Assert.Equal(2, buffer[0]);
				Assert.Equal(1, bytesRead);
			}
		}

		[Fact]
		public void WriteToEndOfMemoryStreamThrows()
		{
			using (var stream = new RegexStream(new MemoryStream(new byte[]{1,2}), "", match => match.Value))
			{
				stream.Seek(0, SeekOrigin.End);
				// TODO: Assert.Throws<NotSupportedException>(()=>stream.Write(new byte[]{3}));
				Assert.Throws<NotSupportedException>(()=>stream.Write(new byte[]{3}, 0, 1));
			}
		}

		[Fact]
		public void WriteSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(1), "", match => match.Value))
			{
				// TODO: stream.Write(new byte[] {1,2,3});
				stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
				stream.Seek(0, SeekOrigin.Begin);
				byte[] buffer = new byte[3];
				// TODO: var bytesRead = stream.Read(buffer);
				var bytesRead = stream.Read(buffer, 0, buffer.Length);
				Assert.Equal(3, bytesRead);
				Assert.Equal(new byte[] {1, 2, 3}, buffer);
			}
		}

		[Fact]
		public void PositionGetSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(1), "", match => match.Value))
			{
				Assert.Equal(0, stream.Position);
				// TODO: stream.Write(new byte[] { 1, 2, 3 });
				stream.Write(new byte[] {1, 2, 3}, 0, 3);
				Assert.Equal(3, stream.Position);
				stream.Seek(0, SeekOrigin.Begin);
				Assert.Equal(0, stream.Position);
				byte[] buffer = new byte[3];
				// TODO: var bytesRead = stream.Read(buffer);
				var bytesRead = stream.Read(buffer, 0, buffer.Length);
				Assert.Equal(3, bytesRead);
				Assert.Equal(3, stream.Position);
			}
		}

		[Fact]
		public void PositionSetSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(2), "", match => match.Value))
			{
				Assert.Equal(0, stream.Position);
				stream.Position = 1;
				Assert.Equal(1, stream.Position);
			}
		}

		[Fact]
		public void SetLengthSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(1), "", match => match.Value))
			{
				Assert.NotEqual(2, stream.Length);
				stream.SetLength(2);
				Assert.Equal(2, stream.Length);
			}
		}

		[Fact]
		public void FlushSucceeds()
		{
			using (var stream = new RegexStream(new MemoryStream(1), "", match => match.Value))
			{
				stream.Flush();
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
