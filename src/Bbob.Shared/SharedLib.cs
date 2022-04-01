﻿using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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

    public static class HashHelper
    {
        static SHA256 sha256 = SHA256.Create();
        public static string GetFileContentHash(string filePath)
        {
            return BytesToString(sha256.ComputeHash(File.ReadAllBytes(filePath)));
        }

        public static string GetContentHash(string content)
        {
            return BytesToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(content)));
        }

        public static string GetContentHash(Stream stream)
        {
            return BytesToString(sha256.ComputeHash(stream));
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
        public static void CopyDirectory(string sourceDir, string destinationDir, string ignores = "", bool recursive = true, bool overwrite = false)
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
                file.CopyTo(targetFilePath, overwrite);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, ignores, recursive, overwrite);
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

        public static bool IsValid(string url)
        {
            return Regex.IsMatch(url, @"^(http|https)://|[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,6}(:[0-9]{1,5})?(/.*)?$/ix
");
        }
    }

    public static class JsonHelper
    {
        public static bool Indented { get; set; } = false;
        public static bool Nullable { get; set; } = true;
        public static string Merge(string? originalJson, string? newContent, JsonDocument? originalDoc = null, JsonDocument? newDoc = null)
        {
            var outputBuffer = new ArrayBufferWriter<byte>();
            using (JsonDocument jDoc1 = originalDoc == null ? JsonDocument.Parse(originalJson ?? throw new NullReferenceException("originalJson is null")) : originalDoc)
            using (JsonDocument jDoc2 = newDoc == null ? JsonDocument.Parse(newContent ?? throw new NullReferenceException("newContent is null")) : newDoc)
            using (var jsonWriter = new Utf8JsonWriter(outputBuffer, new JsonWriterOptions { Indented = Indented }))
            {
                JsonElement root1 = jDoc1.RootElement;
                JsonElement root2 = jDoc2.RootElement;

                if (root1.ValueKind != JsonValueKind.Array && root1.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidOperationException($"The original JSON document to merge new content into must be a container type. Instead it is {root1.ValueKind}.");
                }

                if (root1.ValueKind != root2.ValueKind)
                {
                    return originalJson ?? JsonSerializer.Serialize(originalDoc);
                }

                if (root1.ValueKind == JsonValueKind.Array)
                {
                    MergeArrays(jsonWriter, root1, root2);
                }
                else
                {
                    MergeObjects(jsonWriter, root1, root2);
                }
            }

            return Encoding.UTF8.GetString(outputBuffer.WrittenSpan);
        }

        private static void MergeObjects(Utf8JsonWriter jsonWriter, JsonElement root1, JsonElement root2)
        {
            jsonWriter.WriteStartObject();

            // Write all the properties of the first document.
            // If a property exists in both documents, either:
            // * Merge them, if the value kinds match (e.g. both are objects or arrays),
            // * Completely override the value of the first with the one from the second, if the value kind mismatches (e.g. one is object, while the other is an array or string),
            // * Or favor the value of the first (regardless of what it may be), if the second one is null (i.e. don't override the first).
            foreach (JsonProperty property in root1.EnumerateObject())
            {
                string propertyName = property.Name;

                JsonValueKind newValueKind;

                if (root2.TryGetProperty(propertyName, out JsonElement newValue) && ((newValueKind = newValue.ValueKind) != JsonValueKind.Null || Nullable))
                {
                    jsonWriter.WritePropertyName(propertyName);

                    JsonElement originalValue = property.Value;
                    JsonValueKind originalValueKind = originalValue.ValueKind;

                    if (newValueKind == JsonValueKind.Object && originalValueKind == JsonValueKind.Object)
                    {
                        MergeObjects(jsonWriter, originalValue, newValue); // Recursive call
                    }
                    else if (newValueKind == JsonValueKind.Array && originalValueKind == JsonValueKind.Array)
                    {
                        MergeArrays(jsonWriter, originalValue, newValue);
                    }
                    else
                    {
                        newValue.WriteTo(jsonWriter);
                    }
                }
                else
                {
                    property.WriteTo(jsonWriter);
                }
            }

            // Write all the properties of the second document that are unique to it.
            foreach (JsonProperty property in root2.EnumerateObject())
            {
                if (!root1.TryGetProperty(property.Name, out _))
                {
                    property.WriteTo(jsonWriter);
                }
            }

            jsonWriter.WriteEndObject();
        }

        private static void MergeArrays(Utf8JsonWriter jsonWriter, JsonElement root1, JsonElement root2)
        {

            jsonWriter.WriteStartArray();

            // Write all the elements from both JSON arrays
            foreach (JsonElement element in root1.EnumerateArray())
            {
                element.WriteTo(jsonWriter);
            }
            foreach (JsonElement element in root2.EnumerateArray())
            {
                element.WriteTo(jsonWriter);
            }

            jsonWriter.WriteEndArray();
        }
    }
}
