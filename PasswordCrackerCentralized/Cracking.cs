using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
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

        protected BlockingCollection<String> _dictionaryBuffer;
        protected BlockingCollection<String> _wordVariationsBuffer;
        protected BlockingCollection<EncryptedWord> _encryptedWordBuffer;
        protected BlockingCollection<UserInfoClearText> _crackedUsers;

        private Stopwatch stopwatch;

        public Cracking()
        {
            _messageDigest = new SHA1CryptoServiceProvider();
            //_messageDigest = new MD5CryptoServiceProvider();
            // seems to be same speed
        }

        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        public void RunCracking(String fileName, String dictionaryFileName)
        {
            stopwatch = Stopwatch.StartNew();
            int bufferSize = 10000;

            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile(fileName);
            List<UserInfoClearText> result = new List<UserInfoClearText>();
            
            List<Task> taskList = new List<Task>();

            _dictionaryBuffer = new BlockingCollection<string>(bufferSize);
            Task dictionaryTask = new Task(() => RunDictionaryReader(dictionaryFileName, _dictionaryBuffer));
            dictionaryTask.Start();
            
            taskList.Add(dictionaryTask);

            //RunDictionaryReader("webster-dictionary.txt", _dictionaryBuffer);

            _wordVariationsBuffer = new BlockingCollection<string>(bufferSize);
            Task checkVariations = Task.Run(() => RunWordVariationGenerator(_dictionaryBuffer, _wordVariationsBuffer));
            taskList.Add(checkVariations);

            _encryptedWordBuffer = new BlockingCollection<EncryptedWord>(bufferSize);
            Task encryptWords = Task.Run(() => EncryptWord(_wordVariationsBuffer, _encryptedWordBuffer));
            taskList.Add(encryptWords);

            _crackedUsers = new BlockingCollection<UserInfoClearText>();
            Task compareEncryptedWords =
                Task.Run(
                    () =>
                        CompareEncryptedPassword(_encryptedWordBuffer, userInfos, _crackedUsers));
            taskList.Add(compareEncryptedWords);

            Task bufferStatusTask = Task.Run(() => BufferStatus());
            taskList.Add(bufferStatusTask);

            Task.WaitAll(taskList.ToArray());
            stopwatch.Stop();
            BufferStatus();
            Console.ReadLine();
        }

        protected void RunDictionaryReader(String dictionaryFileName, BlockingCollection<String> dictionaryBuffer)
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
            wordVariationBuffer.CompleteAdding();
        }

        

        private void EncryptWord(BlockingCollection<String> wordVariationBuffer, BlockingCollection<EncryptedWord> encryptedWordBuffer)
        {
            while (!wordVariationBuffer.IsCompleted)
            {
                String currentWord = wordVariationBuffer.Take();
                char[] charArray = currentWord.ToCharArray();
                byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());
                byte[] encryptedPassword = _messageDigest.ComputeHash(passwordAsBytes);
                //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);
                EncryptedWord encryptedWord = new EncryptedWord(encryptedPassword, currentWord);
                encryptedWordBuffer.Add(encryptedWord);
            }
            encryptedWordBuffer.CompleteAdding();

        }

        private void BufferStatus()
        {
            while (!_dictionaryBuffer.IsCompleted && !_wordVariationsBuffer.IsCompleted && !_encryptedWordBuffer.IsCompleted && !_crackedUsers.IsCompleted)
            {
                Console.Clear();
                Console.WriteLine("Buffer Name \t\tWords in buffer");
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("DictionaryBuffer: \t{0}",_dictionaryBuffer.Count);
                Console.WriteLine("WordVariationBuffer: \t{0}", _wordVariationsBuffer.Count);
                Console.WriteLine("EncryptedWordBuffer: \t{0}", _encryptedWordBuffer.Count);
                Console.WriteLine("CrackedUsersBuffer: \t{0}", _crackedUsers.Count);
                Console.WriteLine("Time elapsed: \t\t{0}", stopwatch.Elapsed);
                Console.WriteLine();
                Console.WriteLine("User Name \t\tPassword");
                Console.WriteLine("---------------------------------------");
                foreach (var e in _crackedUsers)
                {
                    if (e.UserName.Length <= 6)
                    {
                        Console.WriteLine("{0} \t\t\t{1}", e.UserName, e.Password);
                    }
                    else if (e.UserName.Length >= 11)
                    {
                        Console.WriteLine("{0} \t{1}", e.UserName, e.Password);
                    }
                    else
                    {
                        Console.WriteLine("{0} \t\t{1}", e.UserName, e.Password);
                    }
                }
                Thread.Sleep(100);
            }

            Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine("Buffer Name \t\tWords in buffer");
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("DictionaryBuffer: \t{0}", _dictionaryBuffer.Count);
            Console.WriteLine("WordVariationBuffer: \t{0}", _wordVariationsBuffer.Count);
            Console.WriteLine("EncryptedWordBuffer: \t{0}", _encryptedWordBuffer.Count);
            Console.WriteLine("CrackedUsersBuffer: \t{0}", _crackedUsers.Count);
            Console.WriteLine("Time elapsed: \t\t{0}", stopwatch.Elapsed);
            Console.WriteLine();
            Console.WriteLine("User Name \t\tPassword");
            Console.WriteLine("---------------------------------------");
            foreach (var e in _crackedUsers)
            {
                if (e.UserName.Length <= 6)
                {
                    Console.WriteLine("{0} \t\t\t{1}", e.UserName, e.Password);
                }
                else if (e.UserName.Length >= 11)
                {
                    Console.WriteLine("{0} \t{1}", e.UserName, e.Password);
                }
                else
                {
                    Console.WriteLine("{0} \t\t{1}", e.UserName, e.Password);
                }
            }
            Console.WriteLine();
            Console.WriteLine("DONE!");
        }

        private void CompareEncryptedPassword(BlockingCollection<EncryptedWord> encryptedWordBuffer, IEnumerable<UserInfo> userInfos, BlockingCollection<UserInfoClearText> crackedUsers)
        {
            while (!encryptedWordBuffer.IsCompleted)
            {
                EncryptedWord encryptedWord = encryptedWordBuffer.Take();
                byte[] possiblePassword = encryptedWord.EncryptedWordInBytes;

                foreach (UserInfo userInfo in userInfos)
                {
                    if (CompareBytes(userInfo.EntryptedPassword, possiblePassword))
                    {
                        crackedUsers.Add(new UserInfoClearText(userInfo.Username, encryptedWord.UnencryptedWord));
                    }
                }
            }
            crackedUsers.CompleteAdding();
            
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
