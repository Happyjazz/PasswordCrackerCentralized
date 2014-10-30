using System;
using System.Collections.Concurrent;
using PasswordCrackerCentralized;
using PasswordCrackerCentralized.model;

namespace UnitTestPasswordCrackerCentralized
{
    /// <summary>
    /// This Class is used as a test-harness, for the unit tests.
    /// In order to access the private mehods of the Cracking-class, this class is used to return these, instead of making them public and thus changing the running build too radically.
    /// </summary>
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
