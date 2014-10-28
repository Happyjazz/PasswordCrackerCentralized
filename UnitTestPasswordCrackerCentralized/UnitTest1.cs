using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasswordCrackerCentralized;
using PasswordCrackerCentralized.util;

namespace UnitTestPasswordCrackerCentralized
{
    [TestClass]
    public class UnitTest1
    {
        private Cracking_Test testClass;

        [ClassInitialize]
        public void ClassInitializer()
        {
            string[] usernames = new string[4];
            usernames[0] = "Peter";
            usernames[1] = "Martin";
            usernames[2] = "Delfs";
            usernames[3] = "Brian";
            
            string[] passwords = new string[4];
            passwords[0] = "95Melanochroi5";
            passwords[1] = "GIFFGAFF";
            passwords[2] = "gniltfig";
            passwords[3] = "Womanbody";

            PasswordFileHandler.WritePasswordFile("testPasswords.txt", usernames, passwords);
            testClass = new Cracking_Test();
        }

        [TestMethod]
        public void TestDictionaryReader()
        {

            //Assert.AreEqual();

        }
    }
}
