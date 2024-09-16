# fastJSON5

Very fast, conformant, and polymorphic JSON5 serializer.

| Package Name                   | Release (NuGet) |
|--------------------------------|-----------------|
| `Json5Core`         | [![NuGet](https://img.shields.io/nuget/v/Json5Core.svg)](https://www.nuget.org/packages/Json5Core/latest)

The spiritual successor to [fastJSON5] designed for .NET Core. Faster, less `unsafe` parts (with plans to completely remove all unsafe blocks), supports features such as `HashSet<>` out of the box.


## Usage

Serialize:

```cs
string json = Json5.Serialize(new
{
    myProperty = 1
});
```

Deserialize:

```cs
MyClass? instance = Json5.Deserialize<MyClass>(json5String);
```
