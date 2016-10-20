using System.Security.Cryptography;

namespace com.tinylabproductions.Cryptography {
  public static class CryptUtil {
    public static RSAParameters GetRsaParameters_2048_Bit_PKCS1_v1_5(byte[] publicKey) {
      // From RFC 2313, PKCS #1, Version 1.5:http://tools.ietf.org/html/rfc2313
      // 7.1 Public-key syntax
      //
      // An RSA public key shall have ASN.1 type RSAPublicKey:
      //
      // RSAPublicKey ::= SEQUENCE {
      //      modulus INTEGER, -- n
      //      publicExponent INTEGER -- e }
      //
      // (This type is specified in X.509 and is retained here for
      // compatibility.)
      //
      // The fields of type RSAPublicKey have the following meanings:
      //
      // o    modulus is the modulus n.
      //
      // o    publicExponent is the public exponent e.
      //            

      // BER Encoding
      // http://en.wikipedia.org/wiki/Distinguished_Encoding_Rules#DER_encoding
      //
      // ASN.1 Format with DER (subset of BER) encoding
      // http://en.wikipedia.org/wiki/Abstract_Syntax_Notation_One

      // It's important to know that the RSAPublicKey is encoded in an ASN.1 (Abstract Syntax Notation One)
      // representation using DER encoding. I had to use a couple articles on Wikipedia to understand
      // ASN.1 and then I manually decoded the public key to determine where the modulus and exponent were
      // located within the 2048 bit public key from Google.
      //
      // Bytes of sample 2048 bit Public Key (hexadecimal) with ASN.1 decoding shown for each byte
      // 30       Identifier: 30 hex = 00110000, P/C = Constructed (1), TAG = SEQUENCE (10000)
      // 82       Length: 82 hex = 130 decimal = 10000010, Long Form Length with 2 octects for length
      // 01       Byte 1/2 of long form length
      // 22       Byte 2/2 of long form length, 0x01 0x22, 00000001 00100010 = 290 bytes
      // 30       Identifier: 30 hex = 00110000, P/C = Constructed (1), TAG = SEQUENCE (10000)
      // 0d       Length: 0d hex = 13 decimal
      // 06       Identifier: 06 hex = 00000110, P/C = Primitive (0), TAG = OBJECT IDENTIFIER (00110)
      // 09       Length: 09 hex = 9 decimal
      // 2a       Byte 1/9 of OBJECT IDENTIFIER
      // 86       Byte 2/9 of OBJECT IDENTIFIER
      // 48       Byte 3/9 of OBJECT IDENTIFIER
      // 86       Byte 4/9 of OBJECT IDENTIFIER
      // f7       Byte 5/9 of OBJECT IDENTIFIER
      // 0d       Byte 6/9 of OBJECT IDENTIFIER
      // 01       Byte 7/9 of OBJECT IDENTIFIER
      // 01       Byte 8/9 of OBJECT IDENTIFIER
      // 01       Byte 9/9 of OBJECT IDENTIFIER
      // 05       Identifier: 05 hex = 00000101, P/C = Primitive (0), TAG = NULL (00101)
      // 00       Length: 00 hex = 0 decimal
      // 03       Identifier: 03 hex = 00000011, P/C = Primitive (0), TAG = BIT STRING (00011)
      // 82       Length: 82 hex = 130 decimal = 10000010, Long Form Length with 2 octects for length
      // 01       Byte 1/2 of long form length
      // 0f       Byte 2/2 of long form length, 0x01 0x0f, 00000001 00010000 = 272 bytes
      // 00       ???? Why 0, what does this mean?
      // 30       Identifier: 30 hex = 00110000, P/C = Constructed (1), TAG = SEQUENCE (10000)
      // 82       Length: 82 hex = 130 decimal = 10000010, Long Form Length with 2 octects for length
      // 01       Byte 1/2 of long form length        
      // 0a       Byte 2/2 of long form length, 0x01 0x0a, 00000001 00001010 = 266 bytes
      // 02       Identifier: 02 hex = 00000010, P/C = Primitive (0), TAG = INTEGER (00010)
      // 82       Length: 82 hex = 130 decimal = 10000010, Long Form Length with 2 octects for length
      // 01       Byte 1/2 of long form length
      // 01       Byte 2/2 of long form length, 0x01 0x01, 00000001 00000001 = 257 bytes
      // 00       Byte 1/257 of modulus (padded left with a 0, leaves 256 actual values)      
      // a9       Byte 2/257 of modulus... public key (modulus) starts here??
      // 87       Byte 3/257 of modulus
      // ....
      // 8f       Byte 255/257 of modulus
      // 14       Byte 256/257 of modulus93       Byte 257/257 of modulus
      // 02       Identifier: 02 hex = 00000010, P/C = Primitive (0), TAG = INTEGER (00010)
      // 03       Length: 03 hex = 3 decimal
      // 01       Byte 1/3 of exponent
      // 00       Byte 2/3 of exponent
      // 01       Byte 3/3 of exponent

      // Modulus starts at byte offset 33 and is 2048 bits (256 bytes)
      // Exponent starts at byte offset 291 and is 3 bytes

      RSAParameters rsaParameters = new RSAParameters();

      int modulusOffset = 33;     // See comments above
      int modulusBytes = 256;     // 2048 bit key
      int exponentOffset = 291;   // See comments above
      int exponentBytes = 3;      // See comments above

      byte[] modulus = new byte[modulusBytes];
      for (int i = 0; i < modulusBytes; i++)
        modulus[i] = publicKey[modulusOffset + i];

      byte[] exponent = new byte[exponentBytes];
      for (int i = 0; i < exponentBytes; i++)
        exponent[i] = publicKey[exponentOffset + i];

      rsaParameters.Modulus = modulus;
      rsaParameters.Exponent = exponent;

      return rsaParameters;
    }

#if ! UNITY_WP8

    public static bool VerifySignature_2048_Bit_PKCS1_v1_5(byte[] data, byte[] signature, byte[] publicKey) {
      // Links for information about PKCS #1 version 1.5:
      // RFC 2313: http://tools.ietf.org/html/rfc2313
      // PKCS #1 on Wikipedia: http://en.wikipedia.org/wiki/PKCS_1


      // Compute an SHA1 hash of the raw data
      SHA1 sha1 = SHA1.Create();
      byte[] hash = sha1.ComputeHash(data);

      // Specify the public key
      RSAParameters rsaParameters = GetRsaParameters_2048_Bit_PKCS1_v1_5(publicKey);

      // Use RSACryptoProvider to verify the signature with the public key
      RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
      rsa.ImportParameters(rsaParameters);

      RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
      rsaDeformatter.SetHashAlgorithm("SHA1");
      return rsaDeformatter.VerifySignature(hash, signature);
    }

#endif

  }
}