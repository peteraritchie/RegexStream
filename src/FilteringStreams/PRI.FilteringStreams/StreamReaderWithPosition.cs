using System;
using System.IO;
using System.Text;

namespace PRI.FilteringStreams.RegexStream
{
	internal class StreamReaderWithPosition : TextReader
	{
		private readonly StreamReader _inner;
		private StreamReaderWithPosition(StreamReader inner)
		{
			_inner = inner;
		}

		public StreamReaderWithPosition(Stream wrappedStream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
			: this(new StreamReader(wrappedStream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen))
		{
		}

		public Encoding CurrentEncoding => _inner.CurrentEncoding;

		public override int Peek() => _inner.Peek();
		public override int Read()
		{
			var c = _inner.Read();
			if (c >= 0)
				AdvancePosition((char)c);
			return c;
		}

		private int _linePos;

		private int _charPos;
		private int _position;
		private int _matched;
		//public int LinePos => _linePos;
		//public int CharPos => _charPos;
		public int Position => _position;


		private void AdvancePosition(char c)
		{
			_position++;
			if (Environment.NewLine[_matched] == c)
			{
				_matched++;
				if (_matched != Environment.NewLine.Length) return;
				_linePos++;
				_charPos = 0;
				_matched = 0;
			}
			else
			{
				_matched = 0;
				_charPos++;
			}
		}
	}
}
