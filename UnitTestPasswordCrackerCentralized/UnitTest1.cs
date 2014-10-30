using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Success is achieved when the file is not found.
        /// </summary>
        [TestMethod]
        public void TestIfDictionaryDoesNotExists()
        {
            string fileNameToFind = "FileDoesNotExist.txt";
            try
            {
                BlockingCollection<String> dictionaryBuffer = new BlockingCollection<string>();
                testClass.TestRunDictionaryReader(fileNameToFind, dictionaryBuffer);

                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("File does not exist.", ex.Message);
            }
        }

        /// <summary>
        /// This method tests whether the buffer content is equal to the content of the actual dictionary-file.
        /// Succes is achieved when the buffer-content matches the file-content.
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

            int expectedMismatches = 0;
            int actualMismatches = comparedList.Count;

            Debug.Print("Actual mismatches: {0} Expected mismatches: {1}", actualMismatches, expectedMismatches);

            Assert.AreEqual(expectedMismatches, actualMismatches);
        }

        /// <summary>
        /// This method runs a single word through the RunWordVariationGenerator and uses different methods in the Cracking_Test class to verify that an example of the different word variations exists in the buffer.
        /// Success is when the buffer of variations contain all of the check-variations generated in testWordList.
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

            List<String> comparedList = testWordList.Except(bufferWordList).ToList();

            int expectedMismatches = 0;
            int actualMismatches = comparedList.Count;

            Debug.Print("Actual mismatches: {0} Expected mismatches: {1}", actualMismatches, expectedMismatches);

            Assert.AreEqual(expectedMismatches, actualMismatches);
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

            Debug.Print("Actual: {0} Expected: {1}", wordToVerify, verificationWord);

            Assert.AreEqual(wordToVerify, verificationWord);
        }

        /// <summary>
        /// This method tests whether the method CompareEncryptedWords can properly compare an encrypted word, in bytes, to the information held in a UserInfo-class.
        /// The words are supplied in the password and wordvariation variables.
        /// This unit test succeeds when the two variables above match.
        /// </summary>
        [TestMethod]
        public void TestCompareEncryptedWordsMatch()
        {
            BlockingCollection<EncryptedWord> encryptedWordBuffer = new BlockingCollection<EncryptedWord>();
            BlockingCollection<UserInfoClearText> crackedUsersBuffer = new BlockingCollection<UserInfoClearText>();

            string userName = "Martin";
            string password = "giffgaff";
            byte[] encryptedPasswordInBytes = testMethods.GetSha1(password);
            string encryptedPasswordBase64 = Convert.ToBase64String(encryptedPasswordInBytes);

            List<UserInfo> userInfos = new List<UserInfo>();
            UserInfo userInfo = new UserInfo(userName, encryptedPasswordBase64);
            userInfos.Add(userInfo);

            string wordVariation = password;
            byte[] encryptedwordVariationInBytes = testMethods.GetSha1(wordVariation);

            EncryptedWord encryptedWord = new EncryptedWord(encryptedwordVariationInBytes, wordVariation);

            encryptedWordBuffer.Add(encryptedWord);
            encryptedWordBuffer.CompleteAdding();

            testClass.TestCompareEncryptedPassword(encryptedWordBuffer, userInfos, crackedUsersBuffer);

            bool actualMatch = crackedUsersBuffer.Count == 1;
            bool expectedMatch = testMethods.CompareByteArrays(userInfo.EncryptedPassword, encryptedPasswordInBytes);

            Debug.Print("Actual: {0} Expected: {1}", actualMatch.ToString(), expectedMatch.ToString());

            Assert.AreEqual(expectedMatch, actualMatch);
        }

        /// <summary>
        /// This method tests whether the method CompareEncryptedWords can properly compare an encrypted word, in bytes, to the information held in a UserInfo-class.
        /// The words are supplied in the password and wordvariation variables.
        /// This unit test succeeds when the two variables above do not match.
        /// </summary>
        [TestMethod]
        public void TestCompareEncryptedWordsNoMatch()
        {
            BlockingCollection<EncryptedWord> encryptedWordBuffer = new BlockingCollection<EncryptedWord>();
            BlockingCollection<UserInfoClearText> crackedUsersBuffer = new BlockingCollection<UserInfoClearText>();

            string userName = "Martin";
            string password = "giffgaff";
            byte[] encryptedPasswordInBytes = testMethods.GetSha1(password);
            string encryptedPasswordBase64 = Convert.ToBase64String(encryptedPasswordInBytes);

            List<UserInfo> userInfos = new List<UserInfo>();
            UserInfo userInfo = new UserInfo(userName, encryptedPasswordBase64);
            userInfos.Add(userInfo);

            string wordVariation = "lightbearer";
            byte[] encryptedwordVariationInBytes = testMethods.GetSha1(wordVariation);

            EncryptedWord encryptedWord = new EncryptedWord(encryptedwordVariationInBytes, wordVariation);

            encryptedWordBuffer.Add(encryptedWord);
            encryptedWordBuffer.CompleteAdding();

            testClass.TestCompareEncryptedPassword(encryptedWordBuffer, userInfos, crackedUsersBuffer);

            bool actualMatch = crackedUsersBuffer.Count == 1;
            bool expectedMatch = testMethods.CompareByteArrays(userInfo.EncryptedPassword, encryptedPasswordInBytes);

            Debug.Print("Actual: {0} Expected: {1}", actualMatch.ToString(), expectedMatch.ToString());

            Assert.AreNotEqual(expectedMatch, actualMatch);
        }
    }
}
