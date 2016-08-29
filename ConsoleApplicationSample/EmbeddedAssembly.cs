using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace ConsoleApplicationSample
{
    /// <summary>
    /// A class for loading an Embedded Assembly
    /// </summary>
    /// <remarks>
    /// https://gist.github.com/MathewSachin/af8c42b1d732c948ff13
    /// 
    /// Before any coding started, the DLLs have to be added into the project.
    /// First, add the DLL as Reference.
    /// Then, add the same DLL as file into the project. Right click the project's name > Add > Existing Item...
    /// for Referenced DLL, in the properties explorer, change Copy Local = False
    /// for Added DLL as File, in the properties explorer, change Build Action = Embedded Resource
    /// If you have any other unmanaged / native DLL that is not able to be referenced, then you won't have to reference it, just add the Unmanaged DLL as embedded resource. 
    /// Obtain EmbeddedAssembly.cs and add it into project.
    ///
    /// Load the DLL from Embedded Resource into Memory. Use EmbeddedAssembly.Load to load it into memory. 
    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         EmbeddedAssembly.Load("MyApp.System.Data.SQLite.dll", "System.Data.SQLite.dll");
    ///     }
    /// }
    ///
    /// Take note about the format of the resource string. Example: 
    /// MyApp.System.Data.SQLite.dll
    /// This string is the Embedded Resource address in the project. 
    ///
    /// MyApp is the project name, followed by the DLL filename. If the DLL is added inside a folder, the folder's name must be included in the resource string. Example:
    /// MyApp.MyFolder.System.Data.SQLite.dll
    ///
    /// The DLLs are not distributed with the application.
    /// When the application fails to locate the DLL, it raises an Event of AppDomain.CurrentDomain.AssemblyResolve.
    /// AssemblyResolve request the missing DLL. The Rest is handled by EmbeddedAssembly.cs.
    /// </remarks>
    public static class EmbeddedAssembly
    {
        static readonly Dictionary<string, Assembly> Index = new Dictionary<string, Assembly>();

        static EmbeddedAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (Index == null || Index.Count == 0) return null;
                if (Index.ContainsKey(e.Name)) return Index[e.Name];
                return null;
            };
        }

        static void Add(Assembly assembly) => Index.Add(assembly.FullName, assembly);

        /// <summary>
        /// Load Assembly, DLL from Embedded Resources into memory.
        /// </summary>
        /// <param name="embeddedResource">Embedded Resource string. Example: WindowsFormsApplication1.SomeTools.dll</param>
        /// <param name="fileName">File Name. Example: SomeTools.dll</param>
        /// <param name="assembly">Assembly of embedded resource. Default value is Assembly.GetExecutingAssembly();</param>
        public static void Load(string embeddedResource, string fileName, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            using (Stream resourceStream = assembly.GetManifestResourceStream(embeddedResource))
            {
                // Either the file is not existed or it is not mark as embedded resource
                // Troubleshooting wrong name: see all embedded resource names by assembly.GetManifestResourceNames()
                if (resourceStream == null) throw new Exception(embeddedResource + " was not found in Embedded Resources.");

                // Get byte[] from the file from embedded resource
                var byteArray = new byte[(int)resourceStream.Length];
                resourceStream.Read(byteArray, 0, (int)resourceStream.Length);

                try
                {
                    // Add the assembly/dll into dictionary
                    Add(Assembly.Load(byteArray));
                }
                catch
                {
                    bool fileNotWritten = true;

                    // Define the temporary storage location of the DLL/assembly
                    string tempFilePath = Path.GetTempPath() + fileName;

                    using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                    {
                        // Get the hash value from embedded DLL/assembly
                        string hashValue = BitConverter.ToString(sha1.ComputeHash(byteArray)).Replace("-", string.Empty);

                        // Determines whether the DLL/assembly is existed or not
                        if (File.Exists(tempFilePath))
                        {
                            // Get the hash value of the existed file
                            string hashValueOfExistingFile = BitConverter.ToString(
                                sha1.ComputeHash(File.ReadAllBytes(tempFilePath))).Replace("-", string.Empty);

                            // Compare the existed DLL/assembly with the Embedded DLL/assembly
                            if (hashValue == hashValueOfExistingFile) fileNotWritten = false;
                        }
                    }

                    // Create the file on disk
                    if (fileNotWritten) File.WriteAllBytes(tempFilePath, byteArray);

                    // Add the loaded DLL/assembly into dictionary
                    Add(Assembly.LoadFrom(tempFilePath));
                }
            }
        }
    }
}