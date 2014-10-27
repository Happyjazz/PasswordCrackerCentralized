using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using PasswordCrackerCentralized.model;
using PasswordCrackerCentralized.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace PasswordCrackerCentralized
{
    public class Cracking
    {
        /// <summary>
        /// The algorithm used for encryption.
        /// Must be exactly the same algorithm that was used to encrypt the passwords in the password file
        /// </summary>
        private readonly HashAlgorithm _messageDigest;

        private BlockingCollection<String> _dictionaryBuffer;
        private BlockingCollection<String> _wordVariationsBuffer;
        private BlockingCollection<Byte[]> _encryptedWordBuffer;
        private BlockingCollection<IEnumerable<UserInfoClearText>> _crackedUsers;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
        }

        public void RunCracking2()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile("passwords.txt");
            List<UserInfoClearText> result = new List<UserInfoClearText>();
            
            List<Task> taskList = new List<Task>();

            _dictionaryBuffer = new BlockingCollection<string>();
            Task dictionaryTask = new Task(() => RunDictionaryReader("webster-dictionary.txt", _dictionaryBuffer));
            dictionaryTask.Start();
            
            taskList.Add(dictionaryTask);

            //RunDictionaryReader("webster-dictionary.txt", _dictionaryBuffer);

            _wordVariationsBuffer = new BlockingCollection<string>();
            Task checkVariations = Task.Run(() => RunWordVariationGenerator(_dictionaryBuffer, _wordVariationsBuffer));
            taskList.Add(checkVariations);

            _encryptedWordBuffer = new BlockingCollection<byte[]>();
            Task encryptWords = Task.Run(() => EncryptWord(_wordVariationsBuffer, _encryptedWordBuffer));
            taskList.Add(encryptWords);


            Task.WaitAll(taskList.ToArray());
        }

        private void RunDictionaryReader(String dictionaryFileName, BlockingCollection<String> dictionaryBuffer)
        {
            Console.WriteLine("Rundictionary started");
            using (FileStream fs = new FileStream(dictionaryFileName, FileMode.Open, FileAccess.Read))
            using (StreamReader dictionary = new StreamReader(fs))
            {
                while (!dictionary.EndOfStream)
                {
                    String dictionaryEntry = dictionary.ReadLine();
                    dictionaryBuffer.Add(dictionaryEntry);
                }
                dictionaryBuffer.CompleteAdding();
            }
        }

        private void RunWordVariationGenerator(BlockingCollection<String> dictionaryBuffer, BlockingCollection<String> wordVariationBuffer)
        {
            while (!dictionaryBuffer.IsCompleted)
            {
                String dictionaryEntry = dictionaryBuffer.Take();

                String possiblePassword = dictionaryEntry;
                wordVariationBuffer.Add(possiblePassword);

                String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
                wordVariationBuffer.Add(possiblePasswordUpperCase);


                String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
                wordVariationBuffer.Add(possiblePasswordCapitalized);

                String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
                wordVariationBuffer.Add(possiblePasswordReverse);

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordEndDigit = dictionaryEntry + i;
                    wordVariationBuffer.Add(possiblePasswordEndDigit);
                }

                for (int i = 0; i < 100; i++)
                {
                    String possiblePasswordStartDigit = i + dictionaryEntry;
                    wordVariationBuffer.Add(possiblePasswordStartDigit);
                }

                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                        wordVariationBuffer.Add(possiblePasswordStartEndDigit);
                    }
                }
            }
        }

        private void EncryptWord(BlockingCollection<String> wordVariationBuffer, BlockingCollection<Byte[]> encryptedWordBuffer)
        {
            while (!wordVariationBuffer.IsCompleted)
            {
                String currentWord = wordVariationBuffer.Take();
                char[] charArray = currentWord.ToCharArray();
                byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());
                byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
                //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

                encryptedWordBuffer.Add(encryptedPassword);
            }
            
        }


        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        //public void RunCracking()
        //{
        //    Stopwatch stopwatch = Stopwatch.StartNew();

        //    List<UserInfo> userInfos =
        //        PasswordFileHandler.ReadPasswordFile("passwords.txt");
        //    List<UserInfoClearText> result = new List<UserInfoClearText>();
        //    using (FileStream fs = new FileStream("webster-dictionary.txt", FileMode.Open, FileAccess.Read))
        //    using (StreamReader dictionary = new StreamReader(fs))
        //    {
        //        while (!dictionary.EndOfStream)
        //        {
        //            String dictionaryEntry = dictionary.ReadLine();
        //            IEnumerable<UserInfoClearText> partialResult = CheckWordWithVariations(dictionaryEntry, userInfos);
        //            result.AddRange(partialResult);
        //        }
        //    }
        //    stopwatch.Stop();
        //    Console.WriteLine(string.Join(", ", result));
        //    Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
        //}

        /// <summary>
        /// Generates a lot of variations, encrypts each of the and compares it to all entries in the password file
        /// </summary>
        /// <param name="dictionaryEntry">A single word from the dictionary</param>
        /// <param name="userInfos">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckWordWithVariations(String dictionaryEntry, List<UserInfo> userInfos)
        {
            List<UserInfoClearText> result = new List<UserInfoClearText>();

            String possiblePassword = dictionaryEntry;
            IEnumerable<UserInfoClearText> partialResult = CheckSingleWord(userInfos, possiblePassword);
            result.AddRange(partialResult);

            String possiblePasswordUpperCase = dictionaryEntry.ToUpper();
            IEnumerable<UserInfoClearText> partialResultUpperCase = CheckSingleWord(userInfos, possiblePasswordUpperCase);
            result.AddRange(partialResultUpperCase);

            String possiblePasswordCapitalized = StringUtilities.Capitalize(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultCapitalized = CheckSingleWord(userInfos, possiblePasswordCapitalized);
            result.AddRange(partialResultCapitalized);

            String possiblePasswordReverse = StringUtilities.Reverse(dictionaryEntry);
            IEnumerable<UserInfoClearText> partialResultReverse = CheckSingleWord(userInfos, possiblePasswordReverse);
            result.AddRange(partialResultReverse);

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordEndDigit = dictionaryEntry + i;
                IEnumerable<UserInfoClearText> partialResultEndDigit = CheckSingleWord(userInfos, possiblePasswordEndDigit);
                result.AddRange(partialResultEndDigit);
            }

            for (int i = 0; i < 100; i++)
            {
                String possiblePasswordStartDigit = i + dictionaryEntry;
                IEnumerable<UserInfoClearText> partialResultStartDigit = CheckSingleWord(userInfos, possiblePasswordStartDigit);
                result.AddRange(partialResultStartDigit);
            }

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    String possiblePasswordStartEndDigit = i + dictionaryEntry + j;
                    IEnumerable<UserInfoClearText> partialResultStartEndDigit = CheckSingleWord(userInfos, possiblePasswordStartEndDigit);
                    result.AddRange(partialResultStartEndDigit);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks a single word (or rather a variation of a word): Encrypts and compares to all entries in the password file
        /// </summary>
        /// <param name="userInfos"></param>
        /// <param name="possiblePassword">List of (username, encrypted password) pairs from the password file</param>
        /// <returns>A list of (username, readable password) pairs. The list might be empty</returns>
        private IEnumerable<UserInfoClearText> CheckSingleWord(IEnumerable<UserInfo> userInfos, String possiblePassword)
        {
            char[] charArray = possiblePassword.ToCharArray();
            byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());
            byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
            //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);

            List<UserInfoClearText> results = new List<UserInfoClearText>();
            foreach (UserInfo userInfo in userInfos)
            {
                if (CompareBytes(userInfo.EntryptedPassword, encryptedPassword))
                {
                    results.Add(new UserInfoClearText(userInfo.Username, possiblePassword));
                    Console.WriteLine(userInfo.Username + " " + possiblePassword);
                }
            }
            return results;
        }

        /// <summary>
        /// Compares to byte arrays. Encrypted words are byte arrays
        /// </summary>
        /// <param name="firstArray"></param>
        /// <param name="secondArray"></param>
        /// <returns></returns>
        private static bool CompareBytes(IList<byte> firstArray, IList<byte> secondArray)
        {
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("firstArray");
            //}
            //if (secondArray == null)
            //{
            //    throw new ArgumentNullException("secondArray");
            //}
            if (firstArray.Count != secondArray.Count)
            {
                return false;
            }
            for (int i = 0; i < firstArray.Count; i++)
            {
                if (firstArray[i] != secondArray[i])
                    return false;
            }
            return true;
        }

    }
}
