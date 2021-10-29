using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Linq;
using System.Runtime.InteropServices;

namespace BGUFS
{
    class Program
    {
        public static List<Header> header;
        public static List<long[]> contentDeleted;
        public static List<long[]> content;

        public static long[] AddContent(String fileSystem, Content c, long p)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            long[] ar = new long[2];
            try
            {
                FileStream writerFileStream = new FileStream(fileSystem, FileMode.Open, FileAccess.Write);
                if (p != 0)
                    writerFileStream.Position = p;
                else
                    writerFileStream.Position = writerFileStream.Length;
                ar[0] = writerFileStream.Position;
                formatter.Serialize(ms, c);
                writerFileStream.Write(ms.ToArray(), 0, ms.ToArray().Length);
                ar[1] = writerFileStream.Length - ar[0];
                writerFileStream.Close();
                return ar;
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to save the files information");
            }
            return ar;
        }

        public static void ChangeContent(String fileSystem, Content c, long p)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            try
            {
                FileStream writerFileStream = new FileStream(fileSystem, FileMode.Open, FileAccess.Write);
                writerFileStream.Position = p;
                formatter.Serialize(ms, c);
                writerFileStream.Write(ms.ToArray(), 0, ms.ToArray().Length);
                long f = writerFileStream.Length;
                writerFileStream.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to save the files information");
            }
        }

        public static Content readContent(String fileSystem, long index, long len)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            FileStream readerFileStream = new FileStream(fileSystem, FileMode.Open, FileAccess.Read);
            readerFileStream.Position = index;
            byte[] arr = new byte[len];
            readerFileStream.Read(arr, 0, arr.Length);
            ms.Write(arr, 0, arr.Length);
            ms.Seek(0, SeekOrigin.Begin);
            Content temp = (Content)formatter.Deserialize(ms);
            readerFileStream.Close();
            return temp;
        }

        public static void Load(String fileSystem)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            try
            {
                FileStream readerFileStream = new FileStream(fileSystem, FileMode.Open, FileAccess.Read);
                header = (List<Header>)formatter.Deserialize(readerFileStream);
                contentDeleted = (List<long[]>)formatter.Deserialize(readerFileStream);
                content = (List<long[]>)formatter.Deserialize(readerFileStream);
                readerFileStream.Close();

            }
            catch (Exception)
            {
                Console.WriteLine("Unable to load the files information");
            }
        }

        public static void Save(String fileSystem)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                FileStream writerFileStream = new FileStream(fileSystem, FileMode.Open, FileAccess.Write);
                formatter.Serialize(writerFileStream, header);
                formatter.Serialize(writerFileStream, contentDeleted);
                formatter.Serialize(writerFileStream, content);
                writerFileStream.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to save the files information");
            }
        }

        public static void Create(String name)
        {
            header = new List<Header>();
            contentDeleted = new List<long[]>();
            content = new List<long[]>();
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                FileStream writerFileStream = new FileStream(name, FileMode.Create, FileAccess.Write);
                formatter.Serialize(writerFileStream, header);
                formatter.Serialize(writerFileStream, contentDeleted);
                formatter.Serialize(writerFileStream, content);
                writerFileStream.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to save the files information");
            }
        }

        public static void Add(String fileSystem, String path)
        {
            String name = path.Substring(path.LastIndexOf('\\') + 1);
            Load(fileSystem);

            foreach (Header h in header)
            {
                if (h.GetFileName().Equals(name))
                {
                    Console.WriteLine("file already exist");
                    System.Environment.Exit(0);
                }
            }

            Byte[] bytes = File.ReadAllBytes(path);
            String file = Convert.ToBase64String(bytes);

            long len = new System.IO.FileInfo(path).Length;

            Header h1 = new Header(name, len, DateTime.Now, false, null);

            System.Security.Cryptography.HashAlgorithm md5Algo = null;
            md5Algo = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5Algo.ComputeHash(bytes);
            h1.SetHash(String.Concat(Array.ConvertAll(hash, x => x.ToString("X2"))));


            for (int i = 0; i < contentDeleted.Count; i++)
            {
                long[] arr = contentDeleted[i];
                Content c = readContent(fileSystem, arr[0], arr[1]);
                if (c.getSize() >= len)
                {
                    c.Setfile(file, len);
                    c.SetHash(String.Concat(Array.ConvertAll(hash, x => x.ToString("X2"))));
                    h1.SetLen(arr[1]);
                    AddContent(fileSystem, c, arr[0]);
                    h1.Setindex(arr[0]);
                    contentDeleted.RemoveAt(i);
                    content.Add(arr);
                    Save(fileSystem);
                    System.Environment.Exit(0);
                }

            }

            Content c1 = new Content(file, len);
            c1.SetHash(String.Concat(Array.ConvertAll(hash, x => x.ToString("X2"))));

            long[] ar;
            if (header.Count == 0)
                ar = AddContent(fileSystem, c1, 50000);
            else
            {
                contentDeleted = contentDeleted.OrderBy(r => r[0]).ToList();
                content = content.OrderBy(r => r[0]).ToList();
                long l, l1, l2;
                if (contentDeleted.Count != 0)
                {
                    l = contentDeleted[contentDeleted.Count - 1][1] + contentDeleted[contentDeleted.Count - 1][0];
                    l1 = content[content.Count - 1][1] + content[content.Count - 1][0];
                    if (l < l1)
                        l2 = l1;
                    else
                        l2 = l;
                }
                else
                    l1 = content[content.Count - 1][1] + content[content.Count - 1][0];
                ar = AddContent(fileSystem, c1, l1);
            }
            h1.SetLen(ar[1]);
            h1.Setindex(ar[0]);
            content.Add(ar);
            header.Add(h1);
            Save(fileSystem);
        }

        public static void Remove(String fileSystem, String name)
        {
            Load(fileSystem);

            foreach (Header h in header)
            {
                if (h.GetFileName().Equals(name))
                {
                    if (h.IsLink() == true)
                    {
                        header.Remove(h);
                        Save(fileSystem);
                        System.Environment.Exit(0);
                    }
                    for (int i = 0; i < content.Count; i++)
                    {
                        long[] arr = content[i];
                        if (arr[0] == h.GetIndex())
                        {
                            Content c = readContent(fileSystem, arr[0], arr[1]);
                            if (c.GetHash().Equals(h.GetHash()))
                            {
                                c.SetDeleted();
                                content.RemoveAt(i);
                                contentDeleted.Add(arr);
                            }
                        }
                    }
                    header.Remove(h);
                    for (int i = 0; i < header.Count; i++)
                    {
                        if (header[i].IsLink() == true && header[i].GetLink().GetFileName().Equals(name))
                        {
                            header.Remove(header[i]);
                            i--;
                        }

                    }

                    Save(fileSystem);
                    System.Environment.Exit(0);
                }
            }
            Console.WriteLine("file does not exist");
        }

        public static void Rename(String fileSystem, String name, String newName)
        {
            Load(fileSystem);

            foreach (Header i in header)
            {
                if (i.GetFileName().Equals(name))
                {
                    foreach (Header j in header)
                    {
                        if (j.GetFileName().Equals(newName))
                        {
                            Console.WriteLine("file" + newName + "already exists");
                            System.Environment.Exit(0);
                        }
                    }
                    i.SetFileName(newName);
                    Save(fileSystem);
                    System.Environment.Exit(0);
                }
            }
            Console.WriteLine("file does not exist");
        }

        public static void Extract(String fileSystem, String name, String path)
        {
            Load(fileSystem);

            foreach (Header h in header)
            {
                if (h.GetFileName().Equals(name))
                {
                    Byte[] bytes2;
                    if (h.IsLink() == true)
                    {
                        Content c = readContent(fileSystem, h.GetLink().GetIndex(), h.GetLink().GetLen());
                        bytes2 = Convert.FromBase64String(c.GetFile());
                    }
                    else
                    {
                        Content c = readContent(fileSystem, h.GetIndex(), h.GetLen());
                        bytes2 = Convert.FromBase64String(c.GetFile());
                    }
                    File.WriteAllBytes(path, bytes2);
                    System.Environment.Exit(0);
                }
            }
            Console.WriteLine("file does not exist");
        }

        public static void Hash(String fileSystem, String name)
        {
            Load(fileSystem);
            foreach (Header h in header)
            {
                if (h.GetFileName().Equals(name))
                {
                    String f;
                    if (h.IsLink() == true)
                        f = h.GetLink().GetHash();
                    else
                        f = h.GetHash();
                    Console.WriteLine(f);
                    System.Environment.Exit(0);
                }
            }
            Console.WriteLine("file does not exist");
        }

        public static void Dir(String fileSystem)
        {
            Load(fileSystem);

            foreach (Header h in header)
            {
                Console.WriteLine(h.toString());
            }
        }

        public static void SortAB(String fileSystem)
        {
            Load(fileSystem);
            header = header.OrderBy(x => x.GetFileName()).ToList();
            Save(fileSystem);
        }

        public static void SortDate(String fileSystem)
        {
            Load(fileSystem);
            header = header.OrderBy(x => x.GetDate()).ToList();
            Save(fileSystem);
        }

        public static void SortSize(String fileSystem)
        {
            Load(fileSystem);
            header = header.OrderBy(x => x.GetSize()).ToList();
            Save(fileSystem);
        }

        public static void AddLink(String fileSystem, String nameFile, String existingfilename)
        {
            Load(fileSystem);

            foreach (Header h1 in header)
            {
                if (h1.GetFileName().Equals(nameFile))
                {
                    Console.WriteLine("file already exist");
                    System.Environment.Exit(0);
                }
            }
            foreach (Header h in header)
            {
                if (h.GetFileName().Equals(existingfilename))
                {
                    Header l = new Header(nameFile, h.GetSize(), DateTime.Now, true, h);
                    header.Add(l);
                    Save(fileSystem);
                }
            }
            Console.WriteLine("file already exist");
            System.Environment.Exit(0);

        }

        public static void Optimize(String fileSystem)
        {
            Load(fileSystem);
            contentDeleted = contentDeleted.OrderBy(r => r[0]).ToList();
            content = content.OrderBy(r => r[0]).ToList();
            if (contentDeleted.Count != 0)
            {
                long temp = contentDeleted[0][0];
                long[] a;
                for (int i = 0; i < content.Count; i++)
                {
                    long[] arr = content[i];
                    if (arr[0] > temp)
                    {
                        Content c = readContent(fileSystem, arr[0], arr[1]);
                        a = AddContent(fileSystem, c, temp);
                        foreach (Header h in header)
                        {
                            if (c.GetHash().Equals(h.GetHash()))
                            {
                                h.Setindex(a[0]);
                                temp = a[0] + h.GetLen();
                            }
                        }
                        content[i][0] = a[0];
                    }
                }
                contentDeleted = new List<long[]>();
                Save(fileSystem);
            }
        }

        static void Main(string[] args)
        {
            String operation = args[0];
            String fileSystem = args[1];
            String str = fileSystem.Substring(0, 6);
            if (!str.Equals("BGUFS_"))
            {
                Console.WriteLine("Not a BGUFS file");
                System.Environment.Exit(0);
            }
            if (operation.Equals("-create"))
            {
                Create(fileSystem);
            }
            if (operation.Equals("-add"))
            {
                String path = args[2];
                Add(fileSystem, path);
            }
            if (operation.Equals("-remove"))
            {
                String path = args[2];
                Remove(fileSystem, path);
            }
            if (operation.Equals("-rename"))
            {
                String path = args[2];
                String name = args[3];
                Rename(fileSystem, path,name);
            }
            if (operation.Equals("-extract"))
            {
                String path = args[2];
                String name = args[3];
                Extract(fileSystem, path, name);
            }
            if (operation.Equals("-dir"))
            {
                Dir(fileSystem);
            }
            if (operation.Equals("-hash"))
            {
                String path = args[2];
                Hash(fileSystem, path);
            }
            if (operation.Equals("-sortAB"))
            {
                SortAB(fileSystem);
            }
            if (operation.Equals("-sortDate"))
            {
                SortDate(fileSystem);
            }
            if (operation.Equals("-sortSize"))
            {
                SortSize(fileSystem);
            }
            if (operation.Equals("-addLink"))
            {
                String link = args[2];
                String name = args[3];
                AddLink(fileSystem, link,name);
            }
            if (operation.Equals("-optimize"))
            {
                Optimize(fileSystem);
            }
        }
    }
}
