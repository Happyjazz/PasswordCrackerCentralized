using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordCrackerCentralized;

namespace UnitTestPasswordCrackerCentralized
{
    class Cracking_Test : Cracking
    {
        public BlockingCollection<String> GetDictionary()
        {
            return this._dictionaryBuffer;
        }
    }
}
