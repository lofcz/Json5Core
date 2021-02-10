using Mono.Cecil;
using System;
using System.IO;

namespace fastJSON5Renamer
{
	class Program
	{
		static void Main(string[] args)
		{
			var library = AssemblyDefinition.ReadAssembly(args[0]);
			foreach (var module in library.Modules)
			{
				foreach (var typeDef in module.Types)
				{
					if (typeDef.FullName.StartsWith("fastJSON.")) typeDef.Namespace = "fastJSON5" + typeDef.Namespace[8..];
					if (typeDef.Name == "JSON") typeDef.Name = "JSON5";
					if (typeDef.Name == "JSONParameters") typeDef.Name = "JSON5Parameters";
				}
			}
			library.Write(args[1]);
			string docFileName = args[1];
			if (docFileName.EndsWith(".dll")) docFileName = docFileName[..^4] + ".xml";
			var contents = File.ReadAllText(docFileName);
			contents = contents.Replace("fastJSON", "fastJSON5").Replace("fastJSON5.JSON", "fastJSON5.JSON5").Replace("fastJSON5.JSONParameters", "fastJSON5.JSON5Parameters").Replace("<name>fastJSON55</name>", "<name>fastJSON5</name>");
			File.WriteAllText(docFileName, contents);
		}
	}
}
