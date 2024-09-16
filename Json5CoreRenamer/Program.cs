using Mono.Cecil;
using System;
using System.IO;

namespace Json5Core5Renamer
{
	class Program
	{
		static void Main(string[] args)
		{
			AssemblyDefinition library = AssemblyDefinition.ReadAssembly(args[0]);
			foreach (ModuleDefinition module in library.Modules)
			{
				foreach (TypeDefinition typeDef in module.Types)
				{
					if (typeDef.FullName.StartsWith("Json5Core.")) typeDef.Namespace = "Json5Core5" + typeDef.Namespace[8..];
					if (typeDef.Name == "JSON") typeDef.Name = "JSON5";
					if (typeDef.Name == "JSONParameters") typeDef.Name = "JSON5Parameters";
				}
			}
			library.Write(args[1]);
			string docFileName = args[1];
			if (docFileName.EndsWith(".dll")) docFileName = docFileName[..^4] + ".xml";
			string contents = File.ReadAllText(docFileName);
			contents = contents.Replace("Json5Core", "Json5Core5").Replace("Json5Core5.JSON", "Json5Core5.JSON5").Replace("Json5Core5.JSONParameters", "Json5Core5.JSON5Parameters").Replace("<name>Json5Core55</name>", "<name>Json5Core5</name>");
			File.WriteAllText(docFileName, contents);
		}
	}
}
