using System.Collections.Concurrent;
using System.Diagnostics;
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
        protected BlockingCollection<UserInfoClearText> _crackedUsersBuffer;

        private Stopwatch stopwatch;

        public Cracking()
        {
        }

        /// <summary>
        /// Runs the password cracking algorithm
        /// </summary>
        public void RunCracking(String fileName, String dictionaryFileName)
        {
            stopwatch = Stopwatch.StartNew();

            List<UserInfo> userInfos = PasswordFileHandler.ReadPasswordFile(fileName);
            List<Task> taskList = new List<Task>();

            //Initialization of the buffers
            int bufferSize = 1000000;
            _dictionaryBuffer = new BlockingCollection<string>(bufferSize);
            _wordVariationsBuffer = new BlockingCollection<string>(bufferSize);
            _encryptedWordBuffer = new BlockingCollection<EncryptedWord>(bufferSize);
            _crackedUsersBuffer = new BlockingCollection<UserInfoClearText>();

            //Task 1 - Read from dictionary and put into dictionaryBuffer
            Task dictionaryTask = Task.Run(() => RunDictionaryReader(dictionaryFileName, _dictionaryBuffer));
            taskList.Add(dictionaryTask);

            //Task 2 - Read from dictionaryBuffer, generate variations of the words and put them into the wordVariationsBuffer
            Task checkVariations = Task.Run(() => RunWordVariationGenerator(_dictionaryBuffer, _wordVariationsBuffer));
            taskList.Add(checkVariations);

            //Task 3 - Read from the wordVariationsBuffer and calculate a SHA1 hash for each word
            //Since the bottleneck of the program is this task, more tasks can be added for increased CPU-usage
            int numberOfEncryptionTasks = 5;

            for (int i = 0; i < numberOfEncryptionTasks; i++)
            {
                Task encryptWords = Task.Run(() => EncryptWord(_wordVariationsBuffer, _encryptedWordBuffer));
                taskList.Add(encryptWords);
            }

            //Task 4 - Take each encrypted word and compare it to the encrypted password og each user in the password file
            Task compareEncryptedWords =
                Task.Run(
                    () =>
                        CompareEncryptedPassword(_encryptedWordBuffer, userInfos, _crackedUsersBuffer));
            taskList.Add(compareEncryptedWords);

            //Task 5 - Delivers the output of the console
            Task bufferStatusTask = Task.Run(() => BufferStatus());
            taskList.Add(bufferStatusTask);


            Task.WaitAll(taskList.ToArray());
            stopwatch.Stop();
            BufferStatus();
            Console.ReadLine();
        }

        /// <summary>
        /// This method takes all the word of txt-file and adds it to a BlockingCollection
        /// </summary>
        /// <param name="dictionaryFileName">The filename of the txt-file containing the dictionary</param>
        /// <param name="dictionaryBuffer">The buffer to hold all words from the dictionary</param>
        protected void RunDictionaryReader(String dictionaryFileName, BlockingCollection<String> dictionaryBuffer)
        {
            if (File.Exists(dictionaryFileName))
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
            else
            {
                throw new Exception("File does not exist.");
            }
            
        }

        /// <summary>
        /// This method takes each word contained in a BlockingCollection and generates 307 variations of it.
        /// </summary>
        /// <param name="dictionaryBuffer">The buffer containing the words to be variated</param>
        /// <param name="wordVariationBuffer">The buffer that holds the variated words</param>
        protected void RunWordVariationGenerator(BlockingCollection<String> dictionaryBuffer, BlockingCollection<String> wordVariationBuffer)
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

        /// <summary>
        /// This method takes the words contained in a BlockingCollection, generates a SHA1 hash from them and sends them to a new BlockingCollection.
        /// </summary>
        /// <param name="wordVariationBuffer">The buffer containing the word to be encrypted</param>
        /// <param name="encryptedWordBuffer">The buffer to hold the encrypted words</param>
        protected void EncryptWord(BlockingCollection<String> wordVariationBuffer, BlockingCollection<EncryptedWord> encryptedWordBuffer)
        {
            HashAlgorithm messageDigest = new SHA1CryptoServiceProvider();
            while (!wordVariationBuffer.IsCompleted)
            {
                String currentWord = wordVariationBuffer.Take();
                char[] charArray = currentWord.ToCharArray();
                byte[] passwordAsBytes = Array.ConvertAll(charArray, PasswordFileHandler.GetConverter());
                byte[] encryptedPassword = messageDigest.ComputeHash(passwordAsBytes);
                //string encryptedPasswordBase64 = System.Convert.ToBase64String(encryptedPassword);
                EncryptedWord encryptedWord = new EncryptedWord(encryptedPassword, currentWord);
                encryptedWordBuffer.Add(encryptedWord);
            }
            encryptedWordBuffer.CompleteAdding();

        }

        /// <summary>
        /// This method provides a UI to keep track of the progress of the runCracking method.
        /// </summary>
        private void BufferStatus()
        {
            while (!_dictionaryBuffer.IsCompleted && !_wordVariationsBuffer.IsCompleted && !_encryptedWordBuffer.IsCompleted && !_crackedUsersBuffer.IsCompleted)
            {
                Console.Clear();
                Console.WriteLine("Buffer Name \t\tWords in buffer");
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("DictionaryBuffer: \t{0}",_dictionaryBuffer.Count);
                Console.WriteLine("WordVariationBuffer: \t{0}", _wordVariationsBuffer.Count);
                Console.WriteLine("EncryptedWordBuffer: \t{0}", _encryptedWordBuffer.Count);
                Console.WriteLine("CrackedUsersBuffer: \t{0}", _crackedUsersBuffer.Count);
                Console.WriteLine("Time elapsed: \t\t{0}", stopwatch.Elapsed);
                Console.WriteLine();
                Console.WriteLine("User Name \t\tPassword");
                Console.WriteLine("---------------------------------------");
                foreach (var e in _crackedUsersBuffer)
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
            Console.WriteLine("CrackedUsersBuffer: \t{0}", _crackedUsersBuffer.Count);
            Console.WriteLine("Time elapsed: \t\t{0}", stopwatch.Elapsed);
            Console.WriteLine();
            Console.WriteLine("User Name \t\tPassword");
            Console.WriteLine("---------------------------------------");
            foreach (var e in _crackedUsersBuffer)
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

        /// <summary>
        /// This methods compares a SHA1-hash from a BlockingCollection, with a supplied UserInfo, to check whether the encrypted password matches the SHA1-hash.
        /// </summary>
        /// <param name="encryptedWordBuffer">The buffer containing the encrypted words as Strings</param>
        /// <param name="userInfos">The user name and password that are being cracked, as UserInfo</param>
        /// <param name="crackedUsers">The buffer containing the cracked user names and passwords, as UserInfoClearText</param>
        protected void CompareEncryptedPassword(BlockingCollection<EncryptedWord> encryptedWordBuffer, IEnumerable<UserInfo> userInfos, BlockingCollection<UserInfoClearText> crackedUsers)
        {
            while (!encryptedWordBuffer.IsCompleted)
            {
                EncryptedWord encryptedWord = encryptedWordBuffer.Take();
                byte[] possiblePassword = encryptedWord.EncryptedWordInBytes;

                foreach (UserInfo userInfo in userInfos)
                {
                    if (CompareBytes(userInfo.EncryptedPassword, possiblePassword))
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
