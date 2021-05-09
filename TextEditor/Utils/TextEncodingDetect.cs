// Copyright 2015-2016 Jonathan Bennett <jon@autoitscript.com>
// 
// https://www.autoitscript.com 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;

namespace TextEditor.Utils
{
	public static class TextEncodingDetect
	{
		public static IEnumerable<string> PossibleEncodingNames { get; } = Enum.GetNames(typeof(BinaryStringEncoding));

		private static BinaryStringEncoding? CheckBom(byte[] buffer)
		{
			// Check for BOM
			if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE) return BinaryStringEncoding.Utf16LE;
			if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF) return BinaryStringEncoding.Utf16BE;
			else if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF) return BinaryStringEncoding.Utf8;
			else return null;
		}

		/// <summary>
		/// Automatically detects the encoding of a text buffer
		/// </summary>
		public static BinaryStringEncoding DetectEncoding([ReadOnlyArray] byte[] buffer)
		{
			// First check if we have a BOM and return that if so
			var encoding = CheckBom(buffer);
			if (encoding.HasValue) return encoding.Value;

			// Now check for valid UTF8
			if (IsUtf8(buffer)) return BinaryStringEncoding.Utf8;
			else if (IsUtf16(buffer, out Endianness endianness)) // Now try UTF16
			{
				return endianness switch
				{
					Endianness.BigEndian => BinaryStringEncoding.Utf16BE,
					Endianness.LittleEndian => BinaryStringEncoding.Utf16LE,
					_ => BinaryStringEncoding.Utf8
				};
			}

			return BinaryStringEncoding.Utf8;
		}

		const bool notUtf16 = false;
		const bool utf16le = true;
		static readonly bool? utf16be = null;

		private enum Endianness : byte
		{
			LittleEndian = 0,
			BigEndian = 1,
			Unknown = 2
		}

		private static Endianness FromConstant(bool? value)
		{
			return value switch
			{
				utf16le => Endianness.LittleEndian,
				notUtf16 => Endianness.Unknown,
				_ => Endianness.BigEndian
			};
		}

		private static bool IsUtf16(byte[] buffer, out Endianness endianness)
		{
			bool? result = IsUtf16Normal(buffer);
			if (result == notUtf16) result = IsUtf16Ascii(buffer);
			endianness = FromConstant(result);
			return result == utf16be || result == utf16le;
		}

		private static bool? IsUtf16Normal(byte[] buffer)
		{
			int size = buffer.Length;
			if (size < 2) return false;
			// Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
			size--;
			int leControlChars = 0;
			int beControlChars = 0;
			uint pos = 0;
			while (pos < size)
			{
				byte ch1 = buffer[pos++];
				byte ch2 = buffer[pos++];

				if (ch1 == 0)
				{
					if (ch2 == 0x0a || ch2 == 0x0d)
					{
						++beControlChars;
					}
				}
				else if (ch2 == 0)
				{
					if (ch1 == 0x0a || ch1 == 0x0d)
					{
						++leControlChars;
					}
				}

				// If we are getting both LE and BE control chars then this file is not utf16
				if (leControlChars > 0 && beControlChars > 0) return notUtf16;
			}

			if (leControlChars > 0) return utf16le;
			else return beControlChars > 0 ? utf16be : notUtf16;
		}

		const int ExpectedNullThreshold = 70 / 100;
		const int UnexpectedNullThreshold = 10 / 100;

		private static bool? IsUtf16Ascii(byte[] buffer)
		{
			var numOddNulls = 0;
			var numEvenNulls = 0;

			// Get even nulls
			uint pos = 0;
			while (pos < buffer.Length)
			{
				if (buffer[pos] == 0) numEvenNulls++;
				pos += 2;
			}

			// Get odd nulls
			pos = 1;
			while (pos < buffer.Length)
			{
				if (buffer[pos] == 0) numOddNulls++;
				pos += 2;
			}

			double evenNullThreshold = numEvenNulls * 2.0 / buffer.Length;
			double oddNullThreshold = numOddNulls * 2.0 / buffer.Length;

			// Lots of odd nulls, low number of even nulls
			if (evenNullThreshold < UnexpectedNullThreshold && oddNullThreshold > ExpectedNullThreshold) return true;
			// Lots of even nulls, low number of odd nulls
			else if (oddNullThreshold < UnexpectedNullThreshold && evenNullThreshold > ExpectedNullThreshold) return null;
			// Don't know
			return false;
		}

		private static bool IsUtf8(byte[] buffer)
		{
			// UTF8 Valid sequences
			// 0xxxxxxx  ASCII
			// 110xxxxx 10xxxxxx  2-byte
			// 1110xxxx 10xxxxxx 10xxxxxx  3-byte
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  4-byte
			//
			// Width in UTF8
			// Decimal      Width
			// 0-127        1 byte
			// 194-223      2 bytes
			// 224-239      3 bytes
			// 240-244      4 bytes
			//
			// Subsequent chars are in the range 128-191
			uint pos = 0;
			while (pos < buffer.Length)
			{
				byte ch = buffer[pos++];
				if (ch == 0) return true;
				int moreChars;
				if (ch <= 127) moreChars = 0;
				else if (ch >= 194 && ch <= 223) moreChars = 1;
				else if (ch >= 224 && ch <= 239) moreChars = 2;
				else if (ch >= 240 && ch <= 244) moreChars = 3;
				else return false;

				// Check secondary chars are in range if we are expecting any
				while (moreChars > 0 && pos < buffer.Length)
				{
					ch = buffer[pos++];
					if (ch < 128 || ch > 191) return false;
					--moreChars;
				}
			}

			return true;
		}
	}
}