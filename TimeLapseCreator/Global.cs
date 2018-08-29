using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeLapseCreator
{
   
    public static class Global
    {
        static bool IsApplicationMakingVedio = false;
        private static ReaderWriterLockSlim lock_ = new ReaderWriterLockSlim(); // lock to write on single file 
        public static void AppendTexttoFile(string filepath, string content)
        {
            lock_.EnterWriteLock(); // Locking the File
            try
            {
                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine(content);
                    }

                }
                else
                {
                    using (StreamWriter sw = File.AppendText(filepath))
                    {
                        sw.WriteLine(content);
                    }
                }
            }
            finally
            {
                lock_.ExitWriteLock(); // Un locking the file 
            }
        }
    }
}
