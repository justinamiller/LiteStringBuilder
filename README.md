# LiteStringBuilder

An alternative to the System.Text.StringBuilder C# class.

## Why?
Because System.Text.StringBuilder does actually a lot of memory allocation when appending string, very often it's just not better than a direct string concat.

## Supported Platforms
Currently;

* .Net Framework 4.0+
* .Net Standard 1.3+

## How do I use it?
*We got your samples right here*

Install the Nuget package like this;

```powershell
    PM> Install-Package >LiteStringBuilder
```
[![NuGet Badge](https://buildstats.info/nuget/LiteStringBuilder)](https://www.nuget.org/packages/LiteStringBuilder/)

Or reference the LiteStringBuilder.dll assembly that matches your app's platform.

### Creating LiteStringBuilder
```C#

    using System.Text;
    //create through instance
    var sb = new LiteStringBuilder();
    
    //or through static call (will create a new instance)
    var sb = LiteStringBuilder.Create();
    
    //Can create instance with buffer pool size
    var sb = new LiteStringBuilder(500);
    
      //Can create instance with initial string value.
    var sb = new LiteStringBuilder("Hello World");
```

### Using string builder
```C#
    //Retrieve an instance from the pool
    var sb = LiteStringBuilder.Create();
 
    sb.Append("Cost: ").Append(32.11).Append(" Sent: ").Append(false);
    
    //Return instance of string;
    sb.ToString();
```

### Performance
Here are the four tests profiled: see PerfTest.cs

The test consists of creating 1000 string by concatenation of a string, a float, a string, an int, an boolean, a short, and finally doing a replacement of 2 string occurrences.

Method | Allocation | Time
------------ | ------------- | ------------- 
string "+" | 347kb | 6.63ms
string.concat() | 416kb | 6.24ms
StringBuilder | 2,439kb | 5.76ms
LiteStringBuilder | 90kb | 3.85ms
