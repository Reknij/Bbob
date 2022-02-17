﻿using System.Globalization;

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
    }
}
