using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PRI.FilteringStreams.RegexStream
{
	/// <summary>
	/// A filter stream to perform regular expression replacements on an input stream.
	/// </summary>
	public sealed class RegexStream : Stream, IDisposable
	{
		private readonly Stream _wrappedStream;
		private readonly MatchEvaluator _matchEvaluator;
		private readonly StreamReaderWithPosition _streamReader;
		private readonly Regex _regex;

		/// <summary>
		/// Create a regular expression filtering stream to wrap <paramref name="stream"/> and apply
		/// <paramref name="matchEvaluator"/> on expressions matching <paramref name="regex"/>
		/// </summary>
		/// <example>
		/// <code>
		/// using (var input = File.OpenRead("input.txt"))
		///	{
		/// 	using (var output = File.Create("output.txt"))
		/// 	{
		///			var stream = new RegexStream(input, @"\bspeeling\b", match => "spelling");
		///			await stream.CopyToAsync(output);
		/// 	}
		/// }
		/// </code>
		/// </example>
		/// <remarks>Default Encoding is <seealso cref="Encoding.ASCII"/></remarks>
		/// <param name="stream">The stream to filter</param>
		/// <param name="regex">The regular expression to evaluate each line with.</param>
		/// <param name="matchEvaluator">The text use for each match</param>
		public RegexStream(Stream stream, string regex, MatchEvaluator matchEvaluator)
			: this(stream, regex, matchEvaluator, Encoding.ASCII)
		{
		}

		/// <summary>
		/// Create a regular expression filtering stream to wrap <paramref name="stream"/> and apply
		/// <paramref name="matchEvaluator"/> on expressions matching <paramref name="regex"/>
		/// </summary>
		/// <example>
		/// <code>
		/// using (var input = File.OpenRead("input.txt"))
		///	{
		/// 	using (var output = File.Create("output.txt"))
		/// 	{
		///			var stream = new RegexStream(input, @"\bspeeling\b", match => "spelling", Encoding.ASCII);
		///			await stream.CopyToAsync(output);
		/// 	}
		/// }
		/// </code>
		/// </example>
		/// <param name="stream">The stream to filter</param>
		/// <param name="regex">The regular expression to evaluate each line with.</param>
		/// <param name="matchEvaluator">The text use for each match</param>
		/// <param name="encoding">The encoding to use in the output.</param>
		public RegexStream(Stream stream, string regex, MatchEvaluator matchEvaluator, Encoding encoding)
		{
			if (regex == null) throw new ArgumentNullException(nameof(regex));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));
			_wrappedStream = stream ?? throw new ArgumentNullException(nameof(stream));
			_matchEvaluator = matchEvaluator ?? throw new ArgumentNullException(nameof(matchEvaluator));
			_regex = new Regex(regex);
			_streamReader = new StreamReaderWithPosition(stream,
				encoding,
				detectEncodingFromByteOrderMarks: false,
				bufferSize: 1024, leaveOpen: false);
		}

		private int _tempBufferCurrentOffset;
		private byte[] _tempBuffer;

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the <paramref name="buffer"/> length.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/>is negative.</exception>
		/// <exception cref="IOException">An I/O error occurs.</exception>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
		public override int Read(byte[] buffer, int offset, int count)
		{
			int totalBytesRead = 0;
			while (true)
			{
				if (_tempBuffer == null || (_tempBufferCurrentOffset + 1) >= _tempBuffer.Length)
				{
					var preReadOffset = _streamReader.Position;
					var line = _streamReader.ReadLine();
					if (line == null) return totalBytesRead;
					var postReadOffset = _streamReader.Position;
					var readNewLine = (postReadOffset - preReadOffset - line.Length) > 0;
					line = _regex.Replace(line, _matchEvaluator);
					_tempBuffer = _streamReader.CurrentEncoding.GetBytes(readNewLine ? line + Environment.NewLine : line);
					_tempBufferCurrentOffset = 0;
				}
				var bytesToCopy = Math.Min(count, _tempBuffer.Length - _tempBufferCurrentOffset);
				Buffer.BlockCopy(_tempBuffer, _tempBufferCurrentOffset, buffer, offset, bytesToCopy);
				count -= bytesToCopy;
				offset += bytesToCopy;
				_tempBufferCurrentOffset += bytesToCopy;
				totalBytesRead += bytesToCopy;
				if (count < 1) break;
			}

			return totalBytesRead;
		}

		/// <summary>
		/// <seealso cref="Stream.Flush"/>
		/// </summary>
		public override void Flush() => _wrappedStream.Flush();

		/// <summary>
		/// <seealso cref="Stream.Seek"/>
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin) => _wrappedStream.Seek(offset, origin);

		/// <summary>
		/// <seealso cref="Stream.SetLength"/>
		/// </summary>
		public override void SetLength(long value) => _wrappedStream.SetLength(value);

		/// <summary>
		/// <seealso cref="Stream.Write"/>
		/// </summary>
		public override void Write(byte[] buffer, int offset, int count) => _wrappedStream.Write(buffer, offset, count);

		/// <summary>
		/// <seealso cref="Stream.CanRead"/>
		/// </summary>
		public override bool CanRead => _wrappedStream.CanRead;

		/// <summary>
		/// <seealso cref="Stream.CanSeek"/>
		/// </summary>
		public override bool CanSeek => _wrappedStream.CanSeek;

		/// <summary>
		/// <seealso cref="Stream.CanWrite"/>
		/// </summary>
		public override bool CanWrite => _wrappedStream.CanWrite;

		/// <summary>
		/// <seealso cref="Stream.Length"/>
		/// </summary>
		public override long Length => _wrappedStream.Length;

		/// <summary>
		/// <seealso cref="Stream.Position"/>
		/// </summary>
		public override long Position
		{
			get => _wrappedStream.Position;
			set => _wrappedStream.Position = value;
		}

		/// <summary>
		/// <seealso cref="Stream.Dispose"/>
		/// </summary>
		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// <seealso cref="Stream.Dispose(bool)"/>
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_wrappedStream?.Dispose();
				_streamReader?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
