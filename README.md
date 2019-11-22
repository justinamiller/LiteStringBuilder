# LiteStringBuilder

An alternative to the System.Text.StringBuilder C# class.

## Why?
Because System.Text.StringBuilder does actually a lot of memory allocation when appending string, very often it's just not better than a direct string concat.

### Performance
[Benchmark Test](https://github.com/justinamiller/LiteStringBuilder/blob/master/perf/Benchmark/StringBenchmark.cs)

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.17763.775 (1809/October2018Update/Redstone5)
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.3928.0), X86 LegacyJIT
  Job-GLVCMQ : .NET Framework 4.8 (4.8.3928.0), X86 LegacyJIT

Runtime=.NET 4.8  

```
|                  Method |        Mean |       Error |    StdDev | 
|------------------------ |------------:|------------:|----------:|
|       LiteStringBuilder |    536.0 ns |    12.75 ns |  13.09 ns |  
|            String_Added |    665.5 ns |    12.85 ns |  13.75 ns |  
|           String_Concat |    667.8 ns |    12.98 ns |  16.87 ns |
|           StringBuilder |    771.3 ns |    11.81 ns |  11.05 ns |
| Large_LiteStringBuilder | 16,045.8 ns |   313.23 ns | 307.63 ns | 
|     Large_String_Concat | 16,645.9 ns |   269.73 ns | 239.11 ns | 
|      Large_String_Added | 17,111.7 ns |   341.10 ns | 478.18 ns | 
|     Large_StringBuilder | 35,434.8 ns | 1,014.10 ns | 948.59 ns | 


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

    using StringHelper;
    //create through instance
    var sb = new LiteStringBuilder();
    
    //or through static call (will create a new instance)
    var sb = LiteStringBuilder.Create();
    
    //Can create instance with buffer pool size
    var sb = new LiteStringBuilder(500);
    
      //Can create instance with initial string value.
    var sb = new LiteStringBuilder("Hello World");
```

### Using LiteStringBuilder
```C#
    //Retrieve an instance from the pool
    var sb = LiteStringBuilder.Create();
 
    sb.Append("Cost: ").Append(32.11).Append(" Sent: ").Append(false);
    
    //Return instance of string;
    sb.ToString();
```


