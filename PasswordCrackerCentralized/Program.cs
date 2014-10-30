using System;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            String passwordFile = "passwords.txt";
            String dictionaryFile = "webster-dictionary.txt";
            int noOfEncryptionTasks = 5;
            int uiRefreshRate = 100;

            try
            {
                Cracking cracker = new Cracking();
                cracker.RunCracking(passwordFile, dictionaryFile, noOfEncryptionTasks, uiRefreshRate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
