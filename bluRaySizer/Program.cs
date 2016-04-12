using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace bluRaySizer
{
    class Program
    {
        //Fields
        private static string _exceptionError = "EXCEPTION--------------------------------------------------";

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            //TODO: implement exit handler routine
            Environment.Exit(5);
            return true;
        }

        private static DateTime StartTime { get; set; }
        private static DateTime FinishTime { get; set; }

        private static TimeSpan ElapsedTime
        {
            get { return FinishTime.Subtract(StartTime); }
        }

        //Main Constructor
        static void Main(string[] args)
        {
            StartTime = DateTime.Now;

            Log.LogWrite("//////////////////////////PROJECT COMMENCED!\\\\\\\\\\\\\\\\\\\\\\\\\\");

            HandlerRoutine handlerRoutine = new HandlerRoutine(ConsoleCtrlCheck);

            GC.KeepAlive(handlerRoutine);
            SetConsoleCtrlHandler(handlerRoutine, true);

            //Check for arguments
            if (args.Count() > 0 && args[0] == "-?") ErrorMsg(3);

            //Instatiate required objects
            Initialization initialization = new Initialization();

            List<SimpleFileInfo> files = new List<SimpleFileInfo>();

            List<string> filteredFiles = new List<string>();

            List<string> rawFiles = new List<string>();

            //Read config file and set Initialization properties
            if (File.Exists(initialization.IniFilePath))
            {
                //Get all lines in config file
                string[] iniData = File.ReadAllLines(initialization.IniFilePath);

                //Iterate through lines and retreive data, if data is present, ensure it is valid.
                foreach (string s in iniData)
                {
                    if (s.Contains(initialization.SourceHeader))
                    {
                        initialization.DataSource = s.Remove(0, initialization.SourceHeader.Count());
                        if (!initialization.CheckSourceDestination(initialization.DataSource)) ErrorMsg(2);
                    }
                    if (s.Contains(initialization.DestinationHeader))
                    {
                        initialization.DataDestination = s.Remove(0, initialization.DestinationHeader.Count());
                        if (!initialization.CheckSourceDestination(initialization.DataDestination)) ErrorMsg(1);
                    }
                    if (s.Contains(initialization.SizeHeader))
                    {
                        initialization.DataSize = Convert.ToDouble(s.Remove(0, initialization.SizeHeader.Count()));
                        if (initialization.DataSize < 210) ErrorMsg(4);
                    }
                    if (s.Contains(initialization.ExceptionsHeader))
                    {
                        string t = s.Remove(0, initialization.ExceptionsHeader.Count());
                        string[] exceptions;

                        if (t.Contains(",")) exceptions = t.Split(',');

                        else if (t.Count() > 0) exceptions = new string[1] { t };

                        else exceptions = new string[1] { "" };

                        initialization.CheckExceptions(exceptions);
                    }
                }
            }

            //No config file
            else ErrorMsg(3);

            //Enumerate files and get file info
            files = GetFileData(initialization.FileExceptions, initialization.CopyDirectories, initialization.WildCardExceptions).ToList();

            //Pack files into bins sorted by file size
            List<Bin> packedBins = PackFiles(files, initialization.DataSize);

            //Create text files for packed bins
            CreateOutputFiles(packedBins, initialization.DataDestination);

            FinishTime = DateTime.Now;

            Log.LogWrite(String.Format("////////////PROJECT COMPLETE! Elapsed Time = {0:hh\\:mm}\\\\\\\\\\\\", ElapsedTime));

            ConsoleCtrlCheck(CtrlTypes.CTRL_CLOSE_EVENT);
        }

        private static void CreateOutputFiles(List<Bin> packedBins, string destination)
        {
            //Track discs to append file name
            int discCount = 0;

            //Write bin contents to text file
            foreach (Bin x in packedBins)
            {
                discCount++;
                string fileName = String.Format("{0}\\{1}_{2}_backup_disc{3}.txt", destination, DateTime.Now.ToShortDateString().Replace('/', '_'), DateTime.Now.ToString("HHmm"), discCount);

                if (File.Exists(fileName)) File.Delete(fileName);

                Console.WriteLine("Creating " + fileName);

                using (StreamWriter streamWriter = new StreamWriter(File.Open(fileName, FileMode.Append)))
                {
                    foreach (SimpleFileInfo y in x.Files)
                    {
                        streamWriter.WriteLine(y.Path);
                        y.Dispose();
                    }
                }
                x.Dispose();
            }
        }

        //Method to enumerates files then retrieve file info for those files
        private  static List<SimpleFileInfo> GetFileData(List<string> exceptions, List<string> copyDirectories, List<string> wildCards)
        {
            //Instantiate objects for use
            List<SimpleFileInfo> files = new List<SimpleFileInfo>();
            FileInfo fileInfo;

            //Used to calculate bin packing
            //const int bytesPerMb = 1048576;
            
            //Use SafeFileEnumerator to get files then retrieve file info for those files
            for (int i = 0; i < copyDirectories.Count(); i++)
            {
                foreach (string x in SafeFileEnumerator.EnumerateFiles(copyDirectories[i], "*", SearchOption.AllDirectories).Except(exceptions))
                {
                    try
                    {
                        
                        fileInfo = new FileInfo(x);
                        SimpleFileInfo simpleFileInfo = new SimpleFileInfo
                        {
                            Path = fileInfo.FullName,
                            SizeMb = (double)Math.Ceiling((decimal)(fileInfo.Length)) // / bytesPerMb))
                        };
                        files.Add(simpleFileInfo);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.LogWrite(_exceptionError);
                        Log.LogWrite(ex.Message);
                        Log.LogWrite(ex.Source);
                        continue;
                    } 
                }              
            }

            if (wildCards != null)
            {
                foreach (string x in wildCards)
                {
                    files.RemoveAll(z => z.Path.EndsWith(x));
                }
            }

            //Sort list by descending to aid in packing
            //return files.OrderByDescending(f => f.SizeMb).ToList();
            return files.ToList();
        }

        //Method to iterate through files and pack them into bins
        private static List<Bin> PackFiles(List<SimpleFileInfo> files, double maxBinSizeMb)
        {
            List<Bin> bins = new List<Bin>();

            foreach (SimpleFileInfo file in files)
            {
                Console.WriteLine("Packing " + file.Path.ToString());
                Bin firstAvailableBin = bins.Where(b => b.RemainingSizeMb > file.SizeMb).FirstOrDefault();

                if (firstAvailableBin == null)
                {
                    Bin newBin = new Bin(maxBinSizeMb);
                    bins.Add(newBin);
                    newBin.Files.Add(file);
                }
                else firstAvailableBin.Files.Add(file);
            }

            foreach (Bin bin in bins)
            {

                Log.LogWrite(bin.Files.Sum(x => x.SizeMb).ToString());
            }

            return bins;
        }

        //Method to display errors
        private static void ErrorMsg(int errorCode)
        {
            string errorLogLine1 = "ERROR------------------------------------------------------";
            string errorLogLine3 = "Program exited.";

            if (errorCode != 3) Log.LogWrite(errorLogLine1);

            switch (errorCode)
            {
                case 1:
                    MessageBox.Show("Destination path invalid. Program exiting.","Error!");
                    Log.LogWrite("Destination path invalid.");
                    break;
                case 2:
                    MessageBox.Show("Source path invalid. Program exiting.", "Error!");
                    Log.LogWrite("Source path invalid.");
                    break;
                case 3:
                    DialogResult dialogResult = MessageBox.Show("This program uses an initialization file to load parameters. " + 
                        "This file can be found at C:\\Users\\Public\\Documents. If an initialization file is not present the program won't run. " + 
                        "Would you like to create an initialization file?", 
                        "Help", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //TODO: Ini file creator
                    }
                    break;
                case 4:
                    MessageBox.Show("Disc size invalid. Program exiting.", "Error!");
                    Log.LogWrite("Disc size invalid.");
                        break;
            }
            Log.LogWrite(errorLogLine3);
            Environment.Exit(errorCode);
        }
    }
}
