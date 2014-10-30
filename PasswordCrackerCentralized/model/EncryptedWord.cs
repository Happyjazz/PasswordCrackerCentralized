using System;

namespace PasswordCrackerCentralized.model
{
    /// <summary>
    /// This class is used to store a word, along with its SHA1-hash.
    /// </summary>
    public class EncryptedWord
    {
        public byte[] EncryptedWordInBytes { get; set; }
        public String UnencryptedWord { get; set; }

        public EncryptedWord(byte[] encryptedWordInBytes, String unencryptedWord)
        {
            EncryptedWordInBytes = encryptedWordInBytes;
            UnencryptedWord = unencryptedWord;

        }
    }
}
