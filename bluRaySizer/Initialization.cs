using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bluRaySizer
{
    public class Initialization
    {
        //Fields and properties
        private string _exceptionError = "EXCEPTION--------------------------------------------------";

        private string _iniFilePath = "C:\\Users\\Public\\Documents\\DataToDisc.ini";
        public string IniFilePath
        {
            get { return _iniFilePath; }
        }

        private string _sourceHeader = "source=";
        public string SourceHeader
        {
            get { return _sourceHeader; }
        }

        private string _destinationHeader = "destination=";
        public string DestinationHeader
        {
            get { return _destinationHeader; }
        }

        private string _sizeHeader = "disc=";
        public string SizeHeader
        {
            get { return _sizeHeader; }
        }

        private string _exceptionsHeader = "exceptions=";
        public string ExceptionsHeader
        {
            get { return _exceptionsHeader; }
        }

        public string DataSource { get; set; }
        public string DataDestination { get; set; }
        public double DataSize { get; set; }  
        public List<string> FileExceptions { get; set; }
        public List<string> CopyDirectories { get; set; }
        public List<string> WildCardExceptions { get; set; }

        //Method to check the source/destination is valid
        public bool CheckSourceDestination(string sourceDestination)
        {
            if (!Directory.Exists(sourceDestination)) return false;
            
            return true;
        }

        //Method that will check if exceptions are valid
        public void CheckExceptions (string[] exceptions)
        {
            //Instantiate FileExceptions property
            FileExceptions = new List<string>();

            if (Array.Find(exceptions, z => z.StartsWith("*.")) != "") WildCardExceptions = new List<string>();

            CopyDirectories = SafeFileEnumerator.EnumerateDirectories(DataSource, "*", SearchOption.TopDirectoryOnly).ToList();

            //Check if there are exceptions
            if (exceptions[0] != "")
            {
                //Iterate through exceptions to see if paths are valid
                for (int i = 0; i < exceptions.Length; i++)
                {
                    try
                    {   
                        //Does exception exist
                        if (File.Exists(exceptions[i]) || Directory.Exists(exceptions[i]))
                        {
                            //Get file attributes of exception to determine if it's a file or directory
                            FileAttributes fileAttributes = File.GetAttributes(exceptions[i]);

                            if ((fileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                if (CopyDirectories.Contains(exceptions[i])) CopyDirectories.Remove(exceptions[i]);

                                else 
                                {
                                    foreach (string x in SafeFileEnumerator.EnumerateFiles(exceptions[i], "*", SearchOption.AllDirectories))
                                    {
                                        FileExceptions.Add(x);
                                    }
                                }

                            }
                            else FileExceptions.Add(exceptions[i]);
                        }
                        else if (exceptions[i].Contains("*."))
                        {
                            WildCardExceptions.Add(exceptions[i].Remove(0, 1));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogWrite(_exceptionError);
                        Log.LogWrite(ex.Message);
                        continue;
                    }
                }
  
            }
        }
    }

}
