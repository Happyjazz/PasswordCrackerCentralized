using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackerCentralized;
using PasswordCrackerCentralized.model;
using PasswordCrackerCentralized.util;

namespace UnitTestPasswordCrackerCentralized
{
    class Cracking_Test : Cracking
    {
        public BlockingCollection<String> GetDictionaryBuffer()
        {
            return _dictionaryBuffer;
        }
        public BlockingCollection<String> GetWordVariationsBuffer()
        {
            return _wordVariationsBuffer;
        }
        public BlockingCollection<EncryptedWord> GetEncryptedWordBuffer()
        {
            return _encryptedWordBuffer;
        }
        public BlockingCollection<UserInfoClearText> GetCrackedUsersBuffer()
        {
            return _crackedUsers;
        }

        public void TestRunDictionaryReader(String dictionaryFileName, BlockingCollection<String> dictionaryBuffer)
        {
            RunDictionaryReader(dictionaryFileName, dictionaryBuffer);
        }

        public void TestRunWordVariationGenerator(BlockingCollection<String> dictionaryBuffer, BlockingCollection<String> wordVariationBuffer)
        {
            RunWordVariationGenerator(dictionaryBuffer, wordVariationBuffer);
        }

        public void TestEncryptWord(BlockingCollection<String> wordVariationBuffer,
            BlockingCollection<EncryptedWord> encryptedWordBuffer)
        {
            EncryptWord(wordVariationBuffer, encryptedWordBuffer);
        }

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
    }
}
