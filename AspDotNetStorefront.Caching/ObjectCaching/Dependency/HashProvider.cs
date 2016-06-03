// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AspDotNetStorefront.Caching.ObjectCaching.Dependency
{
	/// <summary>
	/// Provides fast, non-secure hash algorithms for cache dependencies.
	/// </summary>
	public class HashProvider
	{
		public long Hash(IEnumerable<object> inputs)
		{
			// Transform a collection of objects into a collection of byte arrays
			var normalizedInputs = inputs
				.Select(input => Normalize(input))
				.Where(data => data != null);

			// Get an MD5 hash of the byte arrays
			var md5 = MD5.Create();

			byte[] hash;
			using(var inputByteStream = new EnumerableByteStream(normalizedInputs))
				hash = md5.ComputeHash(inputByteStream);

			// Copy out the first 64 bits of the hash as a long
			return BitConverter.ToInt64(hash, 0);
		}

		/// <summary>
		/// Converts the inputs into a standardized <see cref="byte[]"/> form to ensure consistent hashing.
		/// </summary>
		byte[] Normalize(object input)
		{
			if(input == null)
				return null;

			if(input is DateTime)
				return BitConverter.GetBytes(((DateTime)input).Ticks);

			if(input is int)
				return BitConverter.GetBytes((int)input);

			if(input is long)
				return BitConverter.GetBytes((long)input);

			if(input is string)
				return Encoding.UTF8.GetBytes((string)input);

			throw new NotImplementedException("Can't hash a value of type " + input.GetType().FullName);
		}

		/// <summary>
		/// Exposes an <see cref="IEnumerable<byte[]>"/> as a single forward-only, read-only stream of bytes.
		/// </summary>
		class EnumerableByteStream : Stream
		{
			public override bool CanRead
			{ get { return true; } }

			public override bool CanSeek
			{ get { return false; } }

			public override bool CanWrite
			{ get { return false; } }

			public override long Length
			{ get { throw new NotImplementedException(); } }

			public override long Position
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			readonly IEnumerator<byte[]> DataEnumerator;
			int CurrentEnumerationRemainingBytes;
			bool DoneReading;

			public EnumerableByteStream(IEnumerable<byte[]> data)
			{
				DataEnumerator = (data ?? Enumerable.Empty<byte[]>()).GetEnumerator();
				CurrentEnumerationRemainingBytes = 0;
				DoneReading = false;
			}

			public override void Flush()
			{
				throw new NotImplementedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				// Track how many bytes we've copied and how many more we need to copy before we reach the requested count.
				var bytesCopied = 0;
				var copyRemainingBytes = count;

				while(true)
				{
					// If we've copied all of the bytes we're supposed to, we can leave
					if(copyRemainingBytes == 0)
						return bytesCopied;

					// If the current enumerated array is out of bytes
					if(CurrentEnumerationRemainingBytes == 0)
						if(!(DoneReading = DataEnumerator.MoveNext()))      // Switch to the next one
							return bytesCopied;                             // If there is no next one, we're done copying
						else
							CurrentEnumerationRemainingBytes = DataEnumerator.Current.Length;   // If there is a next one, 
																								// we'll start counting down 
																								// how many bytes are left in it

					// Determine where in the current enumerated array we should start copying from
					var sourceIndex = DataEnumerator.Current.Length - CurrentEnumerationRemainingBytes;

					// Determine how many bytes we can copy on this iteration through the loop
					var copyLength = Math.Min(copyRemainingBytes, CurrentEnumerationRemainingBytes);

					// Copy from the enumerated array into the output buffer array
					Array.ConstrainedCopy(
						sourceArray: DataEnumerator.Current,
						sourceIndex: sourceIndex,
						destinationArray: buffer,
						destinationIndex: bytesCopied,
						length: copyLength);

					// Update our various tracking states with the number of bytes we just copied
					copyRemainingBytes -= copyLength;
					CurrentEnumerationRemainingBytes -= copyLength;
					bytesCopied += copyLength;
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException();
			}
		}
	}
}
