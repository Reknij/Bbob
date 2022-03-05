using System.Globalization;

namespace Bbob.Shared;
public static class SharedLib
{
    public static string BytesToString(byte[] bytes)
    {
        string result = "";
        foreach (byte b in bytes) result += b.ToString("x2");
        return result;
    }
    public static class DateTimeHelper
    {
        public static string GetDateTimeNowString()
        {
            return DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public static class PathHelper
    {
        public static bool FileNameEndWith(string name, string ext)
        {
            return name.IndexOf(ext) == (name.Length - ext.Length);
        }
    }

    public static class ObjectHelper
    {
        // public static void CopyPropertiesAndFieldsFrom(object to, object from)
        // {
        //     Type aType = to.GetType();
        //     Type bType = from.GetType();
        //     var aProperties = aType.GetProperties();
        //     var bProperties = aType.GetProperties();
        //     foreach (var prop in aProperties)
        //     {
        //         var bProp = bType.GetProperty(prop.Name);
        //         if (bProp != null)
        //         {
        //             prop.SetValue(to, bProp.GetValue(from));
        //         }
        //     }
        //     var aFields = aType.GetFields();
        //     var bFields = bType.GetFields();
        //     foreach (var field in aFields)
        //     {
        //         var bField = bType.GetField(field.Name);
        //         if (bField != null)
        //         {
        //             field.SetValue(to, bField.GetValue(from));
        //         }
        //     }
        // }
    }

    public static class DirectoryHelper
    {
        public static void CopyDirectory(string sourceDir, string destinationDir, string ignores = "", bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                if (ignores.Contains(file.FullName)) continue;

                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, ignores, recursive);
                }
            }
        }

        public static void DeleteDirectory(string targetDir)
        {
            File.SetAttributes(targetDir, FileAttributes.Normal);

            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }
    }

    public static class ConsoleHelper
    {
        public static void printDividingLine() => printDividingLine('-');
        public static void printDividingLine(char dividing) => printDividingLine(dividing.ToString());
        public static void printDividingLine(string dividing)
        {
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(dividing);
            }
            Console.Write('\n');
        }
    }

    public static class UrlHelper
    {
        public static string UrlCombine(string url1, string url2) //https://stackoverflow.com/questions/372865/path-combine-for-urls/2806717#2806717
        {
            if (url1.Length == 0)
            {
                return url2;
            }

            if (url2.Length == 0)
            {
                return url1;
            }

            url1 = url1.TrimEnd('/', '\\');
            url2 = url2.TrimStart('/', '\\');

            return string.Format("{0}/{1}", url1, url2);
        }
    }
}
