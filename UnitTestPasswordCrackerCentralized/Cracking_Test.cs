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

        
    }
}
