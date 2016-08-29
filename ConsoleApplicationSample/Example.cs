using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zerobased;

namespace ConsoleApplicationSample
{
    class Example
    {
        public static void Run(string filePath)
        {
            filePath = Check.NotNullOrEmpty(filePath, nameof(filePath));
            var text = File.ReadAllText(filePath);
            var obj = JsonConvert.DeserializeObject<JObject>(text);
            Console.WriteLine(obj.ToString());
        }
    }
}
