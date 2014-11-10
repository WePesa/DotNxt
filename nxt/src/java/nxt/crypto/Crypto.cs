using System;
using System.Threading;

namespace nxt.crypto
{

	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using CipherParameters = org.bouncycastle.crypto.CipherParameters;
	using InvalidCipherTextException = org.bouncycastle.crypto.InvalidCipherTextException;
	using AESEngine = org.bouncycastle.crypto.engines.AESEngine;
	using CBCBlockCipher = org.bouncycastle.crypto.modes.CBCBlockCipher;
	using PaddedBufferedBlockCipher = org.bouncycastle.crypto.paddings.PaddedBufferedBlockCipher;
	using KeyParameter = org.bouncycastle.crypto.params.KeyParameter;
	using ParametersWithIV = org.bouncycastle.crypto.params.ParametersWithIV;


	public sealed class Crypto
	{

		private static final ThreadLocal<SecureRandom> secureRandom = new ThreadLocal<SecureRandom>()
		{
			protected SecureRandom initialValue()
			{
				return new SecureRandom();
			}
		}

		private Crypto() //never
		{
		}

		public static MessageDigest getMessageDigest(string algorithm)
		{
			try
			{
				return MessageDigest.getInstance(algorithm);
			}
			catch(NoSuchAlgorithmException e)
			{
				Logger.logMessage("Missing message digest algorithm: " + algorithm);
				throw new Exception(e.Message, e);
			}
		}

		public static MessageDigest sha256()
		{
			return getMessageDigest("SHA-256");
		}

		public static sbyte[] getPublicKey(string secretPhrase)
		{
			sbyte[] publicKey = new sbyte[32];
			Curve25519.keygen(publicKey, null, Crypto.sha256().digest(Convert.toBytes(secretPhrase)));
//        
//            if (! Curve25519.isCanonicalPublicKey(publicKey)) {
//                throw new RuntimeException("Public key not canonical");
//            }
//            
			return publicKey;
		}

		public static sbyte[] getPrivateKey(string secretPhrase)
		{
			sbyte[] s = Crypto.sha256().digest(Convert.toBytes(secretPhrase));
			Curve25519.clamp(s);
			return s;
		}

		public static void curve(sbyte[] Z, sbyte[] k, sbyte[] P)
		{
			Curve25519.curve(Z, k, P);
		}

		public static sbyte[] sign(sbyte[] message, string secretPhrase)
		{

			sbyte[] P = new sbyte[32];
			sbyte[] s = new sbyte[32];
			MessageDigest digest = Crypto.sha256();
			Curve25519.keygen(P, s, digest.digest(Convert.toBytes(secretPhrase)));

			sbyte[] m = digest.digest(message);

			digest.update(m);
			sbyte[] x = digest.digest(s);

			sbyte[] Y = new sbyte[32];
			Curve25519.keygen(Y, null, x);

			digest.update(m);
			sbyte[] h = digest.digest(Y);

			sbyte[] v = new sbyte[32];
			Curve25519.sign(v, h, x, s);

			sbyte[] signature = new sbyte[64];
			Array.Copy(v, 0, signature, 0, 32);
			Array.Copy(h, 0, signature, 32, 32);

//        
//            if (!Curve25519.isCanonicalSignature(signature)) {
//                throw new RuntimeException("Signature not canonical");
//            }
//            
			return signature;

		}

		public static bool verify(sbyte[] signature, sbyte[] message, sbyte[] publicKey, bool enforceCanonical)
		{

			if(enforceCanonical && !Curve25519.isCanonicalSignature(signature))
			{
				Logger.logDebugMessage("Rejecting non-canonical signature");
				return false;
			}

			if(enforceCanonical && !Curve25519.isCanonicalPublicKey(publicKey))
			{
				Logger.logDebugMessage("Rejecting non-canonical public key");
				return false;
			}

			sbyte[] Y = new sbyte[32];
			sbyte[] v = new sbyte[32];
			Array.Copy(signature, 0, v, 0, 32);
			sbyte[] h = new sbyte[32];
			Array.Copy(signature, 32, h, 0, 32);
			Curve25519.verify(Y, v, h, publicKey);

			MessageDigest digest = Crypto.sha256();
			sbyte[] m = digest.digest(message);
			digest.update(m);
			sbyte[] h2 = digest.digest(Y);

			return Array.Equals(h, h2);
		}

		public static sbyte[] aesEncrypt(sbyte[] plaintext, sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			return aesEncrypt(plaintext, myPrivateKey, theirPublicKey, new sbyte[32]);
		}

		public static sbyte[] aesEncrypt(sbyte[] plaintext, sbyte[] myPrivateKey, sbyte[] theirPublicKey, sbyte[] nonce)
		{
			try
			{
				sbyte[] dhSharedSecret = new sbyte[32];
				Curve25519.curve(dhSharedSecret, myPrivateKey, theirPublicKey);
				for(int i = 0; i < 32; i++)
				{
					dhSharedSecret[i] ^= nonce[i];
				}
				sbyte[] key = sha256().digest(dhSharedSecret);
				sbyte[] iv = new sbyte[16];
				secureRandom.get().nextBytes(iv);
				PaddedBufferedBlockCipher aes = new PaddedBufferedBlockCipher(new CBCBlockCipher(new AESEngine()));
				CipherParameters ivAndKey = new ParametersWithIV(new KeyParameter(key), iv);
				aes.init(true, ivAndKey);
				sbyte[] output = new sbyte[aes.getOutputSize(plaintext.length)];
				int ciphertextLength = aes.processBytes(plaintext, 0, plaintext.length, output, 0);
				ciphertextLength += aes.doFinal(output, ciphertextLength);
				sbyte[] result = new sbyte[iv.Length + ciphertextLength];
				Array.Copy(iv, 0, result, 0, iv.Length);
				Array.Copy(output, 0, result, iv.Length, ciphertextLength);
				return result;
			}
			catch(InvalidCipherTextException e)
			{
				throw new Exception(e.Message, e);
			}
		}

//    
//    public static byte[] aesEncrypt(byte[] plaintext, byte[] myPrivateKey, byte[] theirPublicKey)
//            throws GeneralSecurityException, IOException {
//        byte[] dhSharedSecret = new byte[32];
//        Curve25519.curve(dhSharedSecret, myPrivateKey, theirPublicKey);
//        byte[] key = sha256().digest(dhSharedSecret);
//        SecretKeySpec keySpec = new SecretKeySpec(key, "AES");
//        byte[] iv = new byte[16];
//        secureRandom.get().nextBytes(iv);
//        IvParameterSpec ivSpec = new IvParameterSpec(iv);
//        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
//        cipher.init(Cipher.ENCRYPT_MODE, keySpec, ivSpec);
//        ByteArrayOutputStream ciphertextOut = new ByteArrayOutputStream();
//        ciphertextOut.write(iv);
//        ciphertextOut.write(cipher.doFinal(plaintext));
//        return ciphertextOut.toByteArray();
//    }
//    

		public static sbyte[] aesDecrypt(sbyte[] ivCiphertext, sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			return aesDecrypt(ivCiphertext, myPrivateKey, theirPublicKey, new sbyte[32]);
		}

		public static sbyte[] aesDecrypt(sbyte[] ivCiphertext, sbyte[] myPrivateKey, sbyte[] theirPublicKey, sbyte[] nonce)
		{
			try
			{
				if(ivCiphertext.length < 16 || ivCiphertext.length % 16 != 0)
				{
					throw new InvalidCipherTextException("invalid ciphertext");
				}
				sbyte[] iv = Arrays.copyOfRange(ivCiphertext, 0, 16);
				sbyte[] ciphertext = Arrays.copyOfRange(ivCiphertext, 16, ivCiphertext.length);
				sbyte[] dhSharedSecret = new sbyte[32];
				Curve25519.curve(dhSharedSecret, myPrivateKey, theirPublicKey);
				for(int i = 0; i < 32; i++)
				{
					dhSharedSecret[i] ^= nonce[i];
				}
				sbyte[] key = sha256().digest(dhSharedSecret);
				PaddedBufferedBlockCipher aes = new PaddedBufferedBlockCipher(new CBCBlockCipher(new AESEngine()));
				CipherParameters ivAndKey = new ParametersWithIV(new KeyParameter(key), iv);
				aes.init(false, ivAndKey);
				sbyte[] output = new sbyte[aes.getOutputSize(ciphertext.Length)];
				int plaintextLength = aes.processBytes(ciphertext, 0, ciphertext.Length, output, 0);
				plaintextLength += aes.doFinal(output, plaintextLength);
				sbyte[] result = new sbyte[plaintextLength];
				Array.Copy(output, 0, result, 0, result.Length);
				return result;
			}
			catch(InvalidCipherTextException e)
			{
				throw new Exception(e.Message, e);
			}
		}

//    
//    public static byte[] aesDecrypt(byte[] ivCiphertext, byte[] myPrivateKey, byte theirPublicKey[])
//            throws GeneralSecurityException {
//        if ( ivCiphertext.length < 16 || ivCiphertext.length % 16 != 0 ) {
//            throw new GeneralSecurityException("invalid ciphertext");
//        }
//        byte[] iv = Arrays.copyOfRange(ivCiphertext, 0, 16);
//        byte[] ciphertext = Arrays.copyOfRange(ivCiphertext, 16, ivCiphertext.length);
//        byte[] dhSharedSecret = new byte[32];
//        Curve25519.curve(dhSharedSecret, myPrivateKey, theirPublicKey);
//        byte[] key = sha256().digest(dhSharedSecret);
//        SecretKeySpec keySpec = new SecretKeySpec(key, "AES");
//        IvParameterSpec ivSpec = new IvParameterSpec(iv);
//        Cipher cipher = Cipher.getInstance("AES/CBC/PKCS5Padding");
//        cipher.init(Cipher.DECRYPT_MODE, keySpec, ivSpec);
//        return cipher.doFinal(ciphertext);
//    }
//    

		private static void xorProcess(sbyte[] data, int position, int length, sbyte[] myPrivateKey, sbyte[] theirPublicKey, sbyte[] nonce)
		{

			sbyte[] seed = new sbyte[32];
			Curve25519.curve(seed, myPrivateKey, theirPublicKey);
			for(int i = 0; i < 32; i++)
			{
				seed[i] ^= nonce[i];
			}

			MessageDigest sha256 = sha256();
			seed = sha256.digest(seed);

			for(int i = 0; i < length / 32; i++)
			{
				sbyte[] key = sha256.digest(seed);
				for(int j = 0; j < 32; j++)
				{
					data[position++] ^= key[j];
					seed[j] = (sbyte)(~seed[j]);
				}
				seed = sha256.digest(seed);
			}
			sbyte[] key = sha256.digest(seed);
			for(int i = 0; i < length % 32; i++)
			{
				data[position++] ^= key[i];
			}

		}

		[Obsolete]
		public static sbyte[] xorEncrypt(sbyte[] data, int position, int length, sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			sbyte[] nonce = new sbyte[32];
			secureRandom.get().nextBytes(nonce); // cfb: May block as entropy is being gathered, for example, if they need to read from /dev/random on various unix-like operating systems
			xorProcess(data, position, length, myPrivateKey, theirPublicKey, nonce);
			return nonce;
		}

		[Obsolete]
		public static void xorDecrypt(sbyte[] data, int position, int length, sbyte[] myPrivateKey, sbyte[] theirPublicKey, sbyte[] nonce)
		{
			xorProcess(data, position, length, myPrivateKey, theirPublicKey, nonce);
		}

		public static sbyte[] getSharedSecret(sbyte[] myPrivateKey, sbyte[] theirPublicKey)
		{
			try
			{
				sbyte[] sharedSecret = new sbyte[32];
				Curve25519.curve(sharedSecret, myPrivateKey, theirPublicKey);
				return sharedSecret;
			}
			catch(Exception e)
			{
				Logger.logMessage("Error getting shared secret", e);
				throw e;
			}
		}

		public static string rsEncode(long id)
		{
			return ReedSolomon.encode(id);
		}

		public static long rsDecode(string rsString)
		{
			rsString = rsString.ToUpper();
			try
			{
				long id = ReedSolomon.decode(rsString);
				if(! rsString.Equals(ReedSolomon.encode(id)))
				{
					throw new Exception("ERROR: Reed-Solomon decoding of " + rsString + " not reversible, decoded to " + id);
				}
				return id;
			}
			catch(ReedSolomon.DecodeException e)
			{
				Logger.logDebugMessage("Reed-Solomon decoding failed for " + rsString + ": " + e.ToString());
				throw new Exception(e.ToString(), e);
			}
		}

	}

}