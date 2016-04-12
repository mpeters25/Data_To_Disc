using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bluRaySizer
{
    public static class SafeFileEnumerator
    {
        //Fields
        private static string _exceptionError = "EXCEPTION--------------------------------------------------";

        //Method that will enumerate directories
        public static IEnumerable<string> EnumerateDirectories(string parentDirectory, string searchPattern, SearchOption searchOpt)
        {
            Console.WriteLine("Enumerating directories in " + parentDirectory);
            try
            {
                var directories = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    directories = Directory.EnumerateDirectories(parentDirectory)
                        .SelectMany(x => EnumerateDirectories(x, searchPattern, searchOpt));
                }
                return directories.Concat(Directory.EnumerateDirectories(parentDirectory, searchPattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.LogWrite(_exceptionError);
                Log.LogWrite(ex.Message);
                Log.LogWrite(ex.Source);
                return Enumerable.Empty<string>();
            }
        }

        //Method that will enumerate files safely 
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            Console.WriteLine("Enumerating files in " + path);

            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path)
                                        .SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.LogWrite(_exceptionError);
                Log.LogWrite(ex.Message);
                Log.LogWrite(ex.Source);
                return Enumerable.Empty<string>();
            }
        }
    }
}
