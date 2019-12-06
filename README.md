# LiteStringBuilder
[![NuGet Badge](https://buildstats.info/nuget/LiteStringBuilder)](https://www.nuget.org/packages/LiteStringBuilder/)

An alternative to the System.Text.StringBuilder C# class.



## Why?
Because System.Text.StringBuilder does actually a lot of memory allocation when appending string, very often it's just not better than a direct string concat.

### Performance
[Benchmark Test](https://github.com/justinamiller/LiteStringBuilder/blob/master/perf/Benchmark/StringBenchmark.cs)

##### Framework 4.8
``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4042.0), X86 LegacyJIT
  Job-GLVCMQ : .NET Framework 4.8 (4.8.4042.0), X86 LegacyJIT

Runtime=.NET 4.8  

```
|                    Method |        Mean |     Error |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------- |------------:|----------:|------------:|-------:|------:|------:|----------:|
|              String_Added |    613.7 ns |  12.02 ns |    11.24 ns | 0.0725 |     - |     - |     345 B |
|             String_Concat |    619.3 ns |   6.88 ns |     5.37 ns | 0.0858 |     - |     - |     408 B |
|         LiteStringBuilder |    738.3 ns |  13.77 ns |    12.88 ns | 0.0429 |     - |     - |     204 B |
|       String_Interpolated |    740.5 ns |   7.73 ns |     7.23 ns |  0.0782 |     - |     - |     369 B |
|             StringBuilder |    823.9 ns |  16.27 ns |    14.43 ns | 0.1116 |     - |     - |     529 B |
|   Large_LiteStringBuilder | 13,043.1 ns |  79.89 ns |    62.37 ns | 1.7242 |     - |     - |    8192 B |
|       Large_String_Concat | 15,707.7 ns | 813.10 ns |   798.58 ns | 3.3875 |     - |     - |   16077 B |
| Large_String_Interpolated | 16,491.2 ns | 382.03 ns | 1,108.35 ns | 3.4027 |     - |     - |   16132 B |
|        Large_String_Added | 17,051.5 ns | 255.97 ns |   226.91 ns |  3.3875 |     - |     - |   16077 B |
|       Large_StringBuilder | 33,344.6 ns | 652.89 ns | 1,035.56 ns |  6.0425 |     - |     - |   28699 B |


##### Core 3.0

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.17763.775 (1809/October2018Update/Redstone5)
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 8 logical and 8 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-OQEAOY : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

Runtime=.NET Core 3.0  

```
|                    Method |        Mean |     Error |    StdDev |      Median |  Gen 0 |  Gen 1 | Allocated |
|-------------------------- |------------:|----------:|----------:|------------:|-------:|-------:|----------:|
|              String_Added |    458.7 ns |   3.58 ns |   3.35 ns |    457.7 ns | 0.0467 |      - |     392 B |
|         LiteStringBuilder |    505.3 ns |  10.04 ns |   9.86 ns |    501.2 ns | 0.0410 |      - |     344 B |
|             String_Concat |    538.9 ns |  13.65 ns |  38.29 ns |    527.1 ns | 0.0668 |      - |     560 B |
|       String_Interpolated |    609.5 ns |  12.06 ns |  12.39 ns |    603.9 ns |0.0448 |      - |     376 B |
|             StringBuilder |    682.1 ns |  15.40 ns |  23.51 ns |    677.2 ns | 0.0544 |      - |     456 B |
|   Large_LiteStringBuilder |  8,930.6 ns | 175.61 ns | 180.34 ns |  8,869.1 ns | 0.9766 | 0.0153 |    8208 B |
|       Large_String_Concat | 12,359.0 ns | 214.07 ns | 200.24 ns | 12,350.1 ns |1.9073 | 0.0763 |   16048 B |
| Large_String_Interpolated | 12,612.2 ns | 189.40 ns | 177.17 ns | 12,607.5 ns |  1.9226 | 0.0610 |   16112 B |
|        Large_String_Added | 12,776.4 ns | 296.47 ns | 549.52 ns | 12,611.3 ns | 1.9073 | 0.0763 |   16048 B |
|       Large_StringBuilder | 30,215.0 ns | 569.50 ns | 504.85 ns | 30,011.7 ns | 3.4485 | 0.1831 |   28952 B |

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


