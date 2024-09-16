[![NuGet](https://img.shields.io/nuget/v/Json5Core.svg)](https://www.nuget.org/packages/Json5Core/latest)

# Json5Core

The spiritual successor to [fastJSON5](https://github.com/hamarb123/fastJSON5) designed for .NET Core. Fast, conformant, and polymorphic.  
With fewer `unsafe` parts (with plans to completely remove all unsafe blocks), support for modern collections such as `HashSet<>` out of the box, nullability annotations, familiar API and more.

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
