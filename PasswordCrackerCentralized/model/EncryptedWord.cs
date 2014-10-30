using System;

namespace PasswordCrackerCentralized.model
{
    public class EncryptedWord
    {
        public byte[] EncryptedWordInBytes { get; set; }
        public String EncryptedWordAsString { get; set; }
        public String UnencryptedWord { get; set; }

        public EncryptedWord(byte[] encryptedWordInBytes, String unencryptedWord)
        {
            EncryptedWordInBytes = encryptedWordInBytes;
            UnencryptedWord = unencryptedWord;


        }
    }
}
