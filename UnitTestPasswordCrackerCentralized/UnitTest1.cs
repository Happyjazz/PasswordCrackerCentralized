using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasswordCrackerCentralized;
using PasswordCrackerCentralized.util;

namespace UnitTestPasswordCrackerCentralized
{
    [TestClass]
    public class UnitTest1
    {
        private static Cracking_Test testClass;

        [ClassInitialize]
        public static void ClassInitializer(TestContext context)
        {
            //string[] usernames = new string[4];
            //usernames[0] = "Peter";
            //usernames[1] = "Martin";
            //usernames[2] = "Delfs";
            //usernames[3] = "Brian";
            
            //string[] passwords = new string[4];
            //passwords[0] = "95Melanochroi5";
            //passwords[1] = "GIFFGAFF";
            //passwords[2] = "gniltfig";
            //passwords[3] = "Womanbody";

            //PasswordFileHandler.WritePasswordFile("testPasswords.txt", usernames, passwords);
            testClass = new Cracking_Test();
        }

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
    }
}
