using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace HT.Framework.XLua
{
    public static class ZipFile
    {
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="zipPath">压缩后的zip路径</param>
        public static void Compress(string folderPath, string zipPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Log.Error("压缩文件夹失败：" + folderPath + " 不是有效的文件夹路径！");
                return;
            }

            folderPath = folderPath.Replace("/", "\\");

            using (FileStream fileStream = File.Create(zipPath))
            {
                using (ZipOutputStream outputStream = new ZipOutputStream(fileStream))
                {
                    Zip(folderPath, outputStream, "");
                }
            }
        }

        /// <summary>
        /// 解压缩文件
        /// </summary>
        /// <param name="zipPath">解压的zip路径</param>
        /// <param name="targetPath">目标路径</param>
        /// <param name="password">解压密码</param>
        /// <param name="overWrite">是否覆盖已存在的文件</param>
        public static void DeCompress(string zipPath, string targetPath, string password, bool overWrite)
        {
            if (targetPath == "")
                targetPath = Directory.GetCurrentDirectory();
            if (!targetPath.EndsWith("\\"))
                targetPath = targetPath + "\\";

            using (ZipInputStream zipStream = new ZipInputStream(File.OpenRead(zipPath)))
            {
                zipStream.Password = password;
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    string pathToZip = "";
                    string directoryName = "";
                    string fileName = "";
                    
                    pathToZip = entry.Name;
                    if (pathToZip != "") directoryName = Path.GetDirectoryName(pathToZip) + "\\";
                    fileName = Path.GetFileName(pathToZip);

                    Directory.CreateDirectory(targetPath + directoryName);

                    if (fileName != "")
                    {
                        string filePath = targetPath + directoryName + fileName;
                        if ((File.Exists(filePath) && overWrite) || (!File.Exists(filePath)))
                        {
                            using (FileStream fileStream = File.Create(filePath))
                            {
                                int size = 2048;
                                byte[] data = new byte[size];
                                while (true)
                                {
                                    size = zipStream.Read(data, 0, data.Length);

                                    if (size > 0)
                                        fileStream.Write(data, 0, size);
                                    else
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void Zip(string folderPath, ZipOutputStream outStream, string parentPath)
        {
            if (folderPath[folderPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                folderPath += Path.DirectorySeparatorChar;
            }
            
            string[] filePaths = Directory.GetFileSystemEntries(folderPath);
            foreach (string filePath in filePaths)
            {
                if (Directory.Exists(filePath))
                {
                    string pPath = parentPath;
                    pPath += filePath.Substring(filePath.LastIndexOf("\\") + 1);
                    pPath += "\\";
                    Zip(filePath, outStream, pPath);
                }
                else
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        byte[] buffer = new byte[fileStream.Length];
                        fileStream.Read(buffer, 0, buffer.Length);

                        string fileName = parentPath + filePath.Substring(filePath.LastIndexOf("\\") + 1);
                        ZipEntry entry = new ZipEntry(fileName);

                        entry.DateTime = DateTime.Now;
                        entry.Size = buffer.Length;

                        Crc32 crc = new Crc32();
                        crc.Reset();
                        crc.Update(buffer);

                        entry.Crc = crc.Value;
                        outStream.PutNextEntry(entry);
                        outStream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
    }
}