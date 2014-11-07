using System;

namespace nxt.crypto
{

	using NxtException = nxt.NxtException;


	public sealed class EncryptedData
	{

		private static final ThreadLocal<SecureRandom> secureRandom = new ThreadLocal<SecureRandom>()
		{
			protected SecureRandom initialValue()
			{
				return new SecureRandom();
			}
		}

		public static final EncryptedData EMPTY_DATA = new EncryptedData(new sbyte[0], new sbyte[0]);

		public static EncryptedData encrypt(sbyte[] plaintext, sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			if(plaintext.length == 0)
			{
				return EMPTY_DATA;
			}
			using (ByteArrayOutputStream bos = new ByteArrayOutputStream(), GZIPOutputStream gzip = new GZIPOutputStream(bos))
			{
				gzip.write(plaintext);
				gzip.flush();
				gzip.close();
				sbyte[] compressedPlaintext = bos.toByteArray();
				sbyte[] nonce = new sbyte[32];
				secureRandom.get().nextBytes(nonce);
				sbyte[] data = Crypto.aesEncrypt(compressedPlaintext, myPrivateKey, theirPublicKey, nonce);
				return new EncryptedData(data, nonce);
			}
			catch(IOException e)
			{
				throw new Exception(e.Message, e);
			}
		}

		public static EncryptedData readEncryptedData(ByteBuffer buffer, int length, int maxLength) throws NxtException.NotValidException
		{
			if(length == 0)
			{
				return EMPTY_DATA;
			}
			if(length > maxLength)
			{
				throw new NxtException.NotValidException("Max encrypted data length exceeded: " + length);
			}
			sbyte[] noteBytes = new sbyte[length];
			buffer.get(noteBytes);
			sbyte[] noteNonceBytes = new sbyte[32];
			buffer.get(noteNonceBytes);
			return new EncryptedData(noteBytes, noteNonceBytes);
		}

		private final sbyte[] data;
		private final sbyte[] nonce;

		public EncryptedData(sbyte[] data, sbyte[] nonce)
		{
			this.data = data;
			this.nonce = nonce;
		}

		public sbyte[] decrypt(sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			if(data.Length == 0)
			{
				return data;
			}
			sbyte[] compressedPlaintext = Crypto.aesDecrypt(data, myPrivateKey, theirPublicKey, nonce);
			using (ByteArrayInputStream bis = new ByteArrayInputStream(compressedPlaintext), GZIPInputStream gzip = new GZIPInputStream(bis), ByteArrayOutputStream bos = new ByteArrayOutputStream())
			{
				sbyte[] buffer = new sbyte[1024];
				int nRead;
				while((nRead = gzip.read(buffer, 0, buffer.Length)) > 0)
				{
					bos.write(buffer, 0, nRead);
				}
				bos.flush();
				return bos.toByteArray();
			}
			catch(IOException e)
			{
				throw new Exception(e.Message, e);
			}
		}

		public sbyte[] Data
		{
			return data;
		}

		public sbyte[] Nonce
		{
			return nonce;
		}

		public int Size
		{
			return data.Length + nonce.Length;
		}

	}

}