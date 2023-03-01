using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
// using Newtonsoft.Json;
// using ProtoBuf;

namespace IO
{
    public class IO
    {

        //创建文件夹
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        //获取文件夹路径
        public static string GetDirectoryPath(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }
        //获取文件名称
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }
        //获取文件拓展名
        public static string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
        //读取文件信息，大小创建日期等
        public static void GetFileInfo(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            Console.WriteLine($"文件大小为：{fileInfo.Length}");
            Console.WriteLine($"文件创建日期为：{fileInfo.CreationTime}");
        }
        //读取文件大小
        public static long GetDirectoryLength(string directoryPath)
        {
            DirectoryInfo info = new DirectoryInfo(directoryPath);
            long length = 0;
            foreach (var fileInfo in info.GetFiles())
            {
                length += fileInfo.Length;
            }

            foreach (var directoryInfo in info.GetDirectories())
            {
                length += GetDirectoryLength(directoryInfo.FullName);
            }

            return length;
        }
        //读取文件创建时间
        public static string GetDirectoryCreationTime(string directoryPath)
        {
            return Directory.GetCreationTime(directoryPath).ToString(CultureInfo.InvariantCulture);
        }
        
        //以流的方式写入文件
        public static void WriteToFile(string filePath,string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content);
            using FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            long len = stream.Length;
            stream.Seek(len, SeekOrigin.Begin);
            stream.Write(buffer, 0, buffer.Length);
        }
        //以流的方式读文件
        public static string ReadFile(string filePath)
        {
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[stream.Length];
            int len = stream.Read(buffer, 0, (int)stream.Length);
            if (len == 0)
                return "";
            return Encoding.Default.GetString(buffer);
        }
        //BinaryWriter
         public static void WriteToFileByBinaryWriter<T>(string filePath, T content)
         {
             BinaryFormatter serializer = new BinaryFormatter();
             using BinaryWriter writer = new BinaryWriter(File.OpenWrite(filePath));
             using  MemoryStream memoryStream = new MemoryStream();
             serializer.Serialize(memoryStream, content);
             writer.Write(memoryStream.ToArray());
         }
         //BinaryReader
         public static T ReadFileByBinaryReader<T>(string filePath)
         {
             BinaryFormatter serializer = new BinaryFormatter();
             using FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);
             using BinaryReader reader = new BinaryReader(stream);
             byte[] buffer = new byte[stream.Length];
             int len = reader.Read(buffer, 0, (int)stream.Length);
             if (len == 0)
                 return default;
             return default;
         }
        // //Json.Net
        // public static string SerializeObject(object obj)
        // {
        //     return JsonConvert.SerializeObject(obj);
        // }
        //
        // public static T DeserializeObject<T>(string json)
        // {
        //     return JsonConvert.DeserializeObject<T>(json);
        // }
        // //Protobuf
        // public static byte[] Serialize(IExtensible msgBase)
        // {
        //     using MemoryStream memory = new MemoryStream();
        //     Serializer.Serialize(memory, msgBase);
        //     return memory.ToArray();
        // }
        //
        // public static IExtensible Deserialize(string protoName, byte[] bytes, int offset, int count)
        // {
        //     using MemoryStream memory = new MemoryStream(bytes, offset, count);
        //     Type t = Type.GetType(protoName);
        //     return (IExtensible)Serializer.Deserialize(t, memory);
        // }
    }
}
