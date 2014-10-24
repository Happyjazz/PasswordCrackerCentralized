using System;

namespace PasswordCrackerCentralized
{
    class Program
    {
        static void Main()
        {
            try
            {
                Cracking cracker = new Cracking();
                cracker.RunCracking2();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
