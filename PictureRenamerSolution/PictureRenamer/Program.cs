using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PictureRenamer
{
    class Program
    {
        public static string Directory { get; private set; }

        private static bool IsAllNumeric(string aString)
        {
            char[] chararray = aString.ToCharArray();
            List<bool> possibilities = chararray.ToList().Select(c => Char.IsNumber(c)).ToList();

            if (possibilities.Contains(false))
                return false;
            else
                return true;
        }

        private static string ChangeFileFormat(string aOriginal)
        {
            bool onedigitmonth;
            string newDate = "";

            if (aOriginal.Substring(4, 1) == "0")
            {
                newDate = aOriginal.Remove(4, 1);
                onedigitmonth = true;
            }
            else
            {
                newDate = aOriginal;
                onedigitmonth = false;
            }

            newDate = newDate.Insert(4, ".");

            if (onedigitmonth == true)
            {
                newDate = newDate.Insert(6, ".");
            }
            else
            {
                newDate = newDate.Insert(7, ".");
            }

            return newDate;
        }

        private static bool IsValidDate(string aOriginal)
        {
            char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] ends = { ' ', '.', '_' };
            int start = aOriginal.IndexOfAny(chars);

            string substring = aOriginal.Substring(aOriginal.IndexOfAny(chars), aOriginal.Length - aOriginal.IndexOfAny(chars));
            substring = substring.Substring(0, substring.LastIndexOfAny(ends));

            if (substring.Length >= 7)
                return true;
            else
                return false;
        }

        private static Tuple<int, int, string> FindDate(string aOriginal)
        {
            char[] chars = { '0', '1', '2', '3','4', '5', '6', '7', '8', '9' };
            char[] ends = { ' ', '.', '_' };
            int start = aOriginal.IndexOfAny(chars);
            int end = aOriginal.LastIndexOfAny(ends);
            int blankspace = 0;
            
            string substring = aOriginal.Substring(start, end - start);
            
            if(substring.Length > 7)
            {
                blankspace = substring.IndexOf(' ') + 1;
                if(blankspace <= 3)
                {
                    start = start + blankspace;
                    substring = aOriginal.Substring(start, end - start);
                }
                else
                {
                    end -= (substring.Length - blankspace);
                    substring = aOriginal.Substring(start, end - start - 1);
                }   
            }
           
            return new Tuple<int, int, string>(start, end, substring);
        }

        private static string RenameDate(string aDate)
        {
            char[] chars = { '/', '-' };
            chars.ToList().ForEach(i => aDate = aDate.Replace(i, '.'));
            string year = aDate.Substring(aDate.LastIndexOf('.') + 1, aDate.Length - aDate.LastIndexOf('.') - 1);

            if (year.Length == 2)
                year = "20" + year + ".";
            else
                year = year + ".";

            aDate = year + aDate.Remove(aDate.LastIndexOf('.'));

            return aDate;
        }

        private static string GiveNewNameOfFile(FileInfo aFileInfo)
        {
            string fileName = aFileInfo.Name;
            char[] numchars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            char[] chars =  {' ', '/', '_', '.' };
            char[] stringtoArray = aFileInfo.Name.ToCharArray();

            string fileStart = "";
            try 
	        {
                fileStart = fileName.Substring(0, fileName.IndexOfAny(chars));
	        }
	        catch (Exception)
	        {
	        }

            if (Char.IsNumber(fileName.First()))
            {
                if (fileName.Substring(4, 1) == "." && IsAllNumeric(fileName.Substring(0,4)) == true )
                {
                    return fileName;
                }
                else if(fileName.Substring(2,1) == ".")
                {
                    fileName = fileName.Insert(6, "20");
                    string sub = fileName.Substring(6, 4);
                    fileName = fileName.Remove(5, 5);
                    fileName = fileName.Insert(0, sub + ".");
                    return fileName;
                }
                else
                {
                    if (IsAllNumeric(fileStart) == true)
                    {
                        return ChangeFileFormat(fileName);
                    }
                    else
                    {
                        return "Edge Case";
                    }
                }
            }
            else
            {
                string nextpart = fileName.Substring(fileName.IndexOfAny(chars)+1, fileName.Length - 1 - fileName.IndexOfAny(chars));
                string subnextpart = "";

                if(nextpart.Length >= 4)
                {
                    subnextpart = nextpart.Substring(0, nextpart.IndexOfAny(chars));
                }
                else
                {
                    subnextpart = nextpart;
                }

                if (IsAllNumeric(subnextpart) == true && subnextpart.Length >= 8)
                {
                    return ChangeFileFormat(nextpart);
                }
                else
                {
                    if (numchars.Any(i => stringtoArray.Contains(i)) && IsValidDate(fileName))
                    {
                        Tuple<int, int, string> startend = FindDate(fileName);
                        fileName = fileName.Remove(startend.Item1 - 1, (startend.Item2 - startend.Item1) + 1);
                        fileName = fileName.Insert(0, RenameDate(startend.Item3) + ' ');

                        return fileName;
                    }
                    else
                    {
                        string lastWriteTime = aFileInfo.LastWriteTime.ToShortDateString();
                        string daterename = RenameDate(lastWriteTime);
                        fileName = fileName.Insert(0, daterename + ' ');

                        return fileName;
                    }

                }
            }
        }

        private static void DirectoryReWriter()
        {
            DirectoryInfo di = new DirectoryInfo(Directory);
            string[] extensions = { ".JPG", ".jpg", ".jpeg", ".gif", ".MOV", ".mp4"};

            List<FileInfo> files = di.EnumerateFiles().Where(f => extensions.Contains(f.Extension)).ToList();

            if (File.Exists("logfile.txt"))
                File.Delete("logfile.txt");

            WriteFileLog("Renaming Files:" + Environment.NewLine + Environment.NewLine);

            foreach(FileInfo i in files)
            {
                try
                {
                    string newname =  GiveNewNameOfFile(i);

                    i.MoveTo(i.Directory + "\\" + newname);
                    WriteFileLog("Renamed file: " + i.FullName + "\tTo: " + i.Directory + "\\" + newname + Environment.NewLine);
                }
                catch (Exception)
                {
                    WriteFileLog("Failed file: " + i.FullName + Environment.NewLine);
                }
            }
        }

        static void WriteFileLog(string aContents)
        {
            string logfile = "logfile.txt";

            using(StreamWriter sw = new StreamWriter(logfile, true))
            {
                sw.Write(aContents);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Which Directory do you wish to rename files on?");
            Directory =
                 @"D:\Baby Pics\Babies";
            //@"D:\Test";
            //Console.ReadLine();
            DirectoryReWriter();

            //FindDate("Twins for Christmas 12-13-13 2.jpg");

            Console.ReadLine();
        }
    }
}
