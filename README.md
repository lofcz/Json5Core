# fastJSON5

Very fast, conformant, and polymorphic JSON5 serializer.

| Package Name                   | Release (NuGet) | Nightly (NuGet) |
|--------------------------------|-----------------|-----------------|
| `fastJSON5`         | [![NuGet](https://img.shields.io/nuget/v/fastJSON5.svg)](https://www.nuget.org/packages/fastJSON5/latest) | [![NuGet](https://img.shields.io/nuget/vpre/fastJSON5.svg)](https://www.nuget.org/packages/fastJSON5/absoluteLatest) |

The [fastJSON how to](https://github.com/mgholam/fastJSON/blob/master/Howto.md) is also applicable to this project, but using the fastJSON5 namespace and the JSON5 class name etc.

When building the project for yourself, open the fastJSONCore.sln solution and build the fastJSON5Builder project to create the executables. Use the fastJSON project at fastJSONcore/fastJSON.csproj (fastJSON in the sln file) to modify the library files. The UnitTests/UnitTestsCore.csproj project (UnitTestsCore in the sln file) is used to do the testing, so use it for anything relating to tests.
You may encounter issues building if you have the fastJSON.csproj file open in Visual Studio, try closing it if you do.

Other files: history.txt - history of the fastJSON project, history_json5.txt - history of the fastJSON5 project, fastJSON.nuspec - used to create the nuget package, fastJSON5Renamer - used to rename the relevant classes and namespaces to JSON5 from JSON.

## Security Warning (only applicable if UseExtensions is manually set to true)

It has come to my attention from the *HP Enterprise Security Group* that using the `$type` extension has the potential to be unsafe, so use it with **common sense** and known json sources and not public facing ones to be safe.

## Security Warning Update (only applicable if UseExtensions is manually set to true)
I have added `JSONParameters.BadListTypeChecking` which defaults to `true` to check for known `$type` attack vectors from the paper published from *HP Enterprise Security Group*, when enabled it will throw an exception and stop processing the json. 
