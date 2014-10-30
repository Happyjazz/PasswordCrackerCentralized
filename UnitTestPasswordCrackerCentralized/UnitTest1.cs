using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasswordCrackerCentralized.model;

namespace UnitTestPasswordCrackerCentralized
{
    [TestClass]
    public class UnitTest1
    {
        private static Cracking_Test testClass;
        private static TestVerificationMethods testMethods;

        [ClassInitialize]
        public static void ClassInitializer(TestContext context)
        {
            testClass = new Cracking_Test();
            testMethods = new TestVerificationMethods();
        }

        /// <summary>
        /// This method tests whether the supplied dictionary file exists.
        /// </summary>
        [TestMethod]
        public void TestIfDictionaryDoesNotExists()
        {
            try
            {
                BlockingCollection<String> dictionaryBuffer = new BlockingCollection<string>();
                testClass.TestRunDictionaryReader("FileDoesNotExist.txt", dictionaryBuffer);

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("File does not exist.", ex.Message);
            }
        }

        /// <summary>
        /// This method tests whether the buffer content is equal to the content of the actual dictionary-file.
        /// </summary>
        [TestMethod]
        public void TestBuffer()
        {
            const string dictionaryFileName = "webster-dictionary-reduced.txt";

            BlockingCollection<String> dictionaryBuffer = new BlockingCollection<string>();
            testClass.TestRunDictionaryReader(dictionaryFileName, dictionaryBuffer);

            List<String> dictionaryBufferList = dictionaryBuffer.ToList();
            List<String> dictionaryFileList = new List<string>();

            using (FileStream fs = new FileStream(dictionaryFileName, FileMode.Open, FileAccess.Read))
            using (StreamReader dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {
                    String dictionaryEntry = dictionary.ReadLine();
                    dictionaryFileList.Add(dictionaryEntry);
                }
            }

            List<String> comparedList = dictionaryFileList.Except(dictionaryBufferList).ToList();

            Assert.AreEqual(0, comparedList.Count);
        }

        /// <summary>
        /// This method runs a single word through the RunWordVariationGenerator and uses different methods in the Cracking_Test class to verify that an example of the different word variations exists in the buffer.
        /// </summary>
        [TestMethod]
        public void TestWordVariations()
        {
            BlockingCollection<String> dictionaryBuffer = new BlockingCollection<string>();
            BlockingCollection<String> wordVariationBuffer = new BlockingCollection<string>();

            String wordToTest = "giffgaff";
            dictionaryBuffer.Add(wordToTest);
            dictionaryBuffer.CompleteAdding();

            testClass.TestRunWordVariationGenerator(dictionaryBuffer, wordVariationBuffer);

            List<String> bufferWordList = wordVariationBuffer.ToList();
            List<String> testWordList = new List<string>();

            String plainWord = wordToTest;
            String upperCaseWord = wordToTest.ToUpper();
            String capitalizedWord = testMethods.CapitalizeString(wordToTest);
            String reverseWord = testMethods.ReverseString(wordToTest);
            String startDigitWord = testMethods.AddStartDigit(wordToTest);
            String endDigitWord = testMethods.AddEndDigit(wordToTest);
            String startEndDigit = testMethods.AddStartEndDigit(wordToTest);
            
            testWordList.Add(plainWord);
            testWordList.Add(upperCaseWord);
            testWordList.Add(capitalizedWord);
            testWordList.Add(reverseWord);
            testWordList.Add(startDigitWord);
            testWordList.Add(endDigitWord);
            testWordList.Add(startEndDigit);

            int lort = bufferWordList.Count;

            List<String> comparedList = testWordList.Except(bufferWordList).ToList();

            Assert.AreEqual(0, comparedList.Count);
        }
        
        /// <summary>
        /// This method runs a word through the EncryptWord method and uses the GetSha1 method to verify that the word has been encrypted correctly.
        /// </summary>
        [TestMethod]
        public void TestEncryptWord()
        {
            BlockingCollection<String> wordVariationBuffer = new BlockingCollection<string>();
            BlockingCollection<EncryptedWord> encryptedWordBuffer = new BlockingCollection<EncryptedWord>();

            String wordToTest = "giffgaff";
            wordVariationBuffer.Add(wordToTest);
            wordVariationBuffer.CompleteAdding();

            testClass.TestEncryptWord(wordVariationBuffer, encryptedWordBuffer);

            byte[] wordToVerifyBytes = encryptedWordBuffer.Take().EncryptedWordInBytes;
            byte[] verificationWordBytes = testMethods.GetSha1(wordToTest);

            string wordToVerify = Convert.ToBase64String(verificationWordBytes);
            string verificationWord = Convert.ToBase64String(wordToVerifyBytes);

            Assert.AreEqual(wordToVerify, verificationWord);
        }


    }
}
