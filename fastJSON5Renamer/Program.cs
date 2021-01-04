using Mono.Cecil;
using System;

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
		}
	}
}
