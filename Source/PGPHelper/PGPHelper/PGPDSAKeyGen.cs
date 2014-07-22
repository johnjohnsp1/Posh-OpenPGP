﻿using System;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Bcpg;

namespace PGPHelper
{
    class PgpdsaKeyGen
    {
        /// <summary>
        /// Generate a DSA2 Key pair given its bit size.
        /// </summary>
        /// <param name="keySize">"Key bit size of 1024, 2048 or 3072"</param>
        /// <returns>"DSA2 key pair for the given size"</returns>
        public AsymmetricCipherKeyPair Dsa2KeyGen(int keySize)
        {
            // Check that we got a proper key size
            int[] allowedKeySizes = {1024, 2048, 3072};
            if (!(allowedKeySizes.Contains(keySize)))
            {
                throw new ArgumentException("KeySize provided is not 1024, 2048 or 3072.", "keySize");
            }

            // Set the proper N parameter depending on the bit key size.
            int dsa2NParam;
            if (keySize == 1024)
            {
                dsa2NParam = 160;
            }
            else
            {
                dsa2NParam = 256;
            }

            var secRand = new SecureRandom();
            var dsa2Genertor = GeneratorUtilities.GetKeyPairGenerator("DSA");

            // Generate the proper parameters for the DSA2 Key.
            var digest = new Sha256Digest();
            var paramGen = new DsaParametersGenerator(digest);
            var dsaParamsList = new DsaParameterGenerationParameters(keySize, dsa2NParam, 80, secRand);
            paramGen.Init(dsaParamsList);

            // This will take a while since it has to find a valid random prime number for use.
            var dsaParams = paramGen.GenerateParameters();

            var dsaOptions = new DsaKeyGenerationParameters(secRand,dsaParams);
            var keyPair = dsa2Genertor.GenerateKeyPair();

            return keyPair;
        }

        /// <summary>
        /// Generates an El Gamal Key pair given its bit size.
        /// </summary>
        /// <param name="keySize">Bit size for keys of 1024, 2048, 3072 or 4096.</param>
        /// <returns></returns>
        public AsymmetricCipherKeyPair ElGamalkeyGen(int keySize)
        {
            var secRand = new SecureRandom();
            var probPrime = GetMODRFC3526(keySize);
            var elGenertor = GeneratorUtilities.GetKeyPairGenerator("ELGAMAL");
            var elBaseGenerator = new BigInteger("2", 16);
            var elGamalParameterSet = new ElGamalParameters(probPrime, elBaseGenerator);
            var elParams = new ElGamalKeyGenerationParameters(secRand, elGamalParameterSet);
            elGenertor.Init(elParams);
            var keypair = elGenertor.GenerateKeyPair();
            return keypair;
        }


        public void GenerateDsaElGamalKeyPair(
            AsymmetricCipherKeyPair dsa2KeyPair,
            AsymmetricCipherKeyPair elGamalKeyPair,
            string                  symmAlgorithm,
            string					identity,
            char[]					passPhrase,
            bool					armor,
            string[]                preferdHashAlgo,
            string[]                preferedSymAlgo,
            string[]                preferedCompAlgo)
        {
            // Set the Symetric algorithum for encrypting the key
            SymmetricKeyAlgorithmTag symtype;

            if (string.Equals(symmAlgorithm, "Aes256", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Aes256;
            }
            else if (string.Equals(symmAlgorithm, "Aes192", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Aes192;
            }
            else if (string.Equals(symmAlgorithm, "Aes128", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Aes128;
            }
            else if (string.Equals(symmAlgorithm, "Blowfish", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Blowfish;
            }
            else if (string.Equals(symmAlgorithm, "Twofish", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Twofish;
            }
            else if (string.Equals(symmAlgorithm, "Cast5", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Cast5;
            }
            else if (string.Equals(symmAlgorithm, "Idea", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Idea;
            }
            else if (string.Equals(symmAlgorithm, "DES", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.Des;
            }
            else if (string.Equals(symmAlgorithm, "3DES", StringComparison.CurrentCultureIgnoreCase))
            {
                symtype = SymmetricKeyAlgorithmTag.TripleDes;
            }
            else
            {
                symtype = SymmetricKeyAlgorithmTag.Twofish;
            }

        }

        /// <summary>
        /// Returns a known good prime based on RFC 3526 for
        /// bit sizes of 1024, 2048, 3072 and 4096.
        /// </summary>
        /// <param name="BitSize">Bit size of 1024, 2048, 3072 or 4096.</param>
        /// <returns></returns>
        private BigInteger GetMODRFC3526(int BitSize)
        {
            string sbs;
            switch (BitSize)
            {
                case 4096:
                    sbs = "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1" +
                           "29024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
                           "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245" +
                           "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED" +
                           "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D" +
                           "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F" +
                           "83655D23DCA3AD961C62F356208552BB9ED529077096966D" +
                           "670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B" +
                           "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9" +
                           "DE2BCBF6955817183995497CEA956AE515D2261898FA0510" +
                           "15728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64" +
                           "ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7" +
                           "ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6B" +
                           "F12FFA06D98A0864D87602733EC86A64521F2B18177B200C" +
                           "BBE117577A615D6C770988C0BAD946E208E24FA074E5AB31" +
                           "43DB5BFCE0FD108E4B82D120A92108011A723C12A787E6D7" +
                           "88719A10BDBA5B2699C327186AF4E23C1A946834B6150BDA" +
                           "2583E9CA2AD44CE8DBBBC2DB04DE8EF92E8EFC141FBECAA6" +
                           "287C59474E6BC05D99B2964FA090C3A2233BA186515BE7ED" +
                           "1F612970CEE2D7AFB81BDD762170481CD0069127D5B05AA9" +
                           "93B4EA988D8FDDC186FFB7DC90A6C08F4DF435C934063199" +
                           "FFFFFFFFFFFFFFFF";
                    return new BigInteger(sbs,16);

                case 3072:
                    sbs = "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1"+
                          "29024E088A67CC74020BBEA63B139B22514A08798E3404DD"+
                          "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245"+
                          "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED"+
                          "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D"+
                          "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F"+
                          "83655D23DCA3AD961C62F356208552BB9ED529077096966D"+
                          "670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B"+
                          "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9"+
                          "DE2BCBF6955817183995497CEA956AE515D2261898FA0510"+
                          "15728E5A8AAAC42DAD33170D04507A33A85521ABDF1CBA64"+
                          "ECFB850458DBEF0A8AEA71575D060C7DB3970F85A6E1E4C7"+
                          "ABF5AE8CDB0933D71E8C94E04A25619DCEE3D2261AD2EE6B"+
                          "F12FFA06D98A0864D87602733EC86A64521F2B18177B200C"+
                          "BBE117577A615D6C770988C0BAD946E208E24FA074E5AB31"+
                          "43DB5BFCE0FD108E4B82D120A93AD2CAFFFFFFFFFFFFFFFF";
                    return new BigInteger(sbs,16);
            

                case 2048:
                    sbs = "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1" +
                          "29024E088A67CC74020BBEA63B139B22514A08798E3404DD" +
                          "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245"+
                          "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED"+
                          "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D"+
                          "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F"+
                          "83655D23DCA3AD961C62F356208552BB9ED529077096966D"+
                          "670C354E4ABC9804F1746C08CA18217C32905E462E36CE3B"+
                          "E39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9"+
                          "DE2BCBF6955817183995497CEA956AE515D2261898FA0510"+
                          "15728E5A8AACAA68FFFFFFFFFFFFFFFF";
                     return new BigInteger(sbs,16);
            

                case 1024: 
                    sbs = "FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD1"+
                          "29024E088A67CC74020BBEA63B139B22514A08798E3404DD"+
                          "EF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245"+
                          "E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7ED"+
                          "EE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3D"+
                          "C2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F"+
                          "83655D23DCA3AD961C62F356208552BB9ED529077096966D"+
                          "670C354E4ABC9804F1746C08CA237327FFFFFFFFFFFFFFFF";
                    return new BigInteger(sbs,16);
                
                default:
                    throw new ArgumentException("KeySize provided is not 1024, 2048 or 3072.", "KeySize");
            }
        }
    }
}