using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Gamlor.Db4oPad
{
    /// <summary>
    /// Hack to include the users assemblies! We want to add the assemblies the user
    /// had specified when creating the connection. 
    /// However we cannot add those to the compile context, 
    /// because the LINQPad DynamicDataContextDriver-class
    /// has only a GetAssembliesToAdd()-method which doesn't give us the connection context.
    /// 
    /// Therefore we've no information at query-compile time which additional assemblies are required.
    /// </summary>
    class UserAssembliesProvider
    {
        internal const string IdKey = "LINQPadInstanceID";
        private const string Db4oAssemblies = "Db4objects.Db4o";

        private readonly IEnumerable<string> assemblies;

        private UserAssembliesProvider(IEnumerable<string> assemblies)
        {
            this.assemblies = assemblies;
        }
        

        public static UserAssembliesProvider CreateForCurrentAssemblyContext(string assemblyList)
        {
            if(null==assemblyList)
            {
                assemblyList = "";
            }
            var paths = from path in assemblyList.Split(Environment.NewLine.ToCharArray())
                        where File.Exists(path) && !Path.GetFileName(path).StartsWith(Db4oAssemblies)
                        select path;
            return new UserAssembliesProvider(CopyAll(paths));
        }

        public static UserAssembliesProvider Restore()
        {
            var path = GetContextPath();
            if(Directory.Exists(path))
            {
                return new UserAssembliesProvider(Directory.GetFiles(path));     
            }
            return new UserAssembliesProvider(new string[0]);
        }


        public IEnumerable<string> GetAssemblies()
        {
            return assemblies;

        }
        private static IEnumerable<string> CopyAll(IEnumerable<string> paths)
        {
            var targetDir = GetContextPath();
            Directory.CreateDirectory(targetDir);
            var result = new List<string>();
            foreach (var path in paths)
            {
                var targetLocation = targetDir + @"\" + Path.GetFileName(path);
                if (!File.Exists(targetLocation) || !IsFileLocked(new FileInfo(targetLocation)))
                {
                    File.Copy(path, targetLocation, true);
                }
                result.Add(targetLocation);
            }
            return result;
        }

        private static bool IsFileLocked(FileInfo file)
        {
            
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        private static string GetContextPath()
        {
            return Path.Combine(Path.Combine(Path.GetTempPath(), "db4oLinqPadDriver"),
                                ContextID());
        }

        private static string ContextID()
        {
            var id= (string)AppDomain.CurrentDomain.GetData(IdKey);
            if(null==id)
            {
                return "emptx";
            }
            return id;
        }
    }
}