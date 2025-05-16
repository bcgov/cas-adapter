using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace RedHat.OpenShift.Utils
{
    public static class CertificateLoader
    {
        public static X509Certificate2 LoadCertificateWithKey(string certificateFile, string keyFile)
        {
            var certificate = new X509Certificate2(certificateFile);
            return certificate.CopyWithPrivateKey(ReadPrivateKeyAsRSA(keyFile));
        }

        private static RSA ReadPrivateKeyAsRSA(string keyFile)
        {
            using (var reader = new StreamReader(new MemoryStream(File.ReadAllBytes(keyFile))))
            {
                var obj = new PemReader(reader).ReadObject();
                if (obj is AsymmetricCipherKeyPair)
                {
                    var cipherKey = (AsymmetricCipherKeyPair)obj;
                    obj = cipherKey.Private;
                }

                var privKey = (RsaPrivateCrtKeyParameters)obj;
                return RSA.Create(DotNetUtilities.ToRSAParameters(privKey));
            }
        }

        private static class DotNetUtilities
        {
            /*
            +    // This class was derived from:
            +    // https://github.com/bcgit/bc-csharp/blob/master/crypto/src/security/DotNetUtilities.cs
            +    // License:
            +    // The Bouncy Castle License
            +    // Copyright (c) 2000-2018 The Legion of the Bouncy Castle Inc.
            +    // (https://www.bouncycastle.org)
            +    // Permission is hereby granted, free of charge, to any person obtaining a
            +    // copy of this software and associated documentation files (the "Software"), to deal in the
            +    // Software without restriction, including without limitation the rights to use, copy, modify, merge,
            +    // publish, distribute, sub license, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
            +    // The above copyright notice and this permission notice shall be included
            +    // in all copies or substantial portions of the Software.
            */

            public static RSAParameters ToRSAParameters(RsaPrivateCrtKeyParameters privKey)
            {
                var rp = new RSAParameters();
                rp.Modulus = privKey.Modulus.ToByteArrayUnsigned();
                rp.Exponent = privKey.PublicExponent.ToByteArrayUnsigned();
                rp.P = privKey.P.ToByteArrayUnsigned();
                rp.Q = privKey.Q.ToByteArrayUnsigned();
                rp.D = ConvertRSAParametersField(privKey.Exponent, rp.Modulus.Length);
                rp.DP = ConvertRSAParametersField(privKey.DP, rp.P.Length);
                rp.DQ = ConvertRSAParametersField(privKey.DQ, rp.Q.Length);
                rp.InverseQ = ConvertRSAParametersField(privKey.QInv, rp.Q.Length);
                return rp;
            }

            private static byte[] ConvertRSAParametersField(Org.BouncyCastle.Math.BigInteger n, int size)
            {
                var bs = n.ToByteArrayUnsigned();

                if (bs.Length == size)
                    return bs;

                if (bs.Length > size)
                    throw new ArgumentException("Specified size too small", "size");

                var padded = new byte[size];
                Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
                return padded;
            }
        }
    }
}
