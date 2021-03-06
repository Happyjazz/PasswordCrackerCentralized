﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UnitTestPasswordCrackerCentralized
{
    /// <summary>
    /// This Class is used by the unit tests, to provide alternative methods for generating results to be compared with the actual results.
    /// </summary>
    class TestVerificationMethods
    {
        public String CapitalizeString(String word)
        {
            if (String.IsNullOrEmpty(word))
            {
                throw new Exception("String cannot be null or empty");
            }
            return word.First().ToString().ToUpper() + word.Substring(1);
        }
        public String ReverseString(String word)
        {
            if (String.IsNullOrEmpty(word))
            {
                throw new Exception("String cannot be null or empty");
            }
            char[] cArray = word.ToCharArray();
            string reverse = String.Empty;
            for (int i = cArray.Length - 1; i > -1; i--)
            {
                reverse += cArray[i];
            }
            return reverse;
        }

        public string AddStartDigit(String word)
        {
            if (String.IsNullOrEmpty(word))
            {
                throw new Exception("String cannot be null or empty");
            }
            Random random = new Random();

            int startDigit = random.Next(0, 99);

            return startDigit + word;
        }
        public string AddEndDigit(String word)
        {
            if (String.IsNullOrEmpty(word))
            {
                throw new Exception("String cannot be null or empty");
            }
            Random random = new Random();

            int endDigit = random.Next(0, 99);

            return word + endDigit;
        }
        public string AddStartEndDigit(String word)
        {
            if (String.IsNullOrEmpty(word))
            {
                throw new Exception("String cannot be null or empty");
            }
            Random random = new Random();

            int startDigit = random.Next(0, 9);
            int endDigit = random.Next(0, 9);

            return startDigit + word + endDigit;
        }
        public byte[] GetSha1(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] hashInBytes = sha.ComputeHash(bytes);

            return hashInBytes;
        }

        public bool CompareByteArrays(byte[] byteArray1, byte[] byteArray2)
        {
            return byteArray1.SequenceEqual(byteArray2);
        }

    }
}
