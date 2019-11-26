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
|                    Method |        Mean |     Error |    StdDev | 
|-------------------------- |------------:|----------:|----------:|
|         LiteStringBuilder |    532.0 ns |  10.67 ns |  14.61 ns | 
|              String_Added |    653.0 ns |  12.98 ns |  27.39 ns | 
|             String_Concat |    675.3 ns |  15.43 ns |  42.76 ns |
|             StringBuilder |    741.9 ns |  14.63 ns |  16.27 ns | 
|       String_Interpolated |    802.0 ns |  17.25 ns |  49.78 ns |
|   Large_LiteStringBuilder | 15,598.1 ns | 228.05 ns | 202.16 ns | 
|       Large_String_Concat | 16,164.7 ns |  70.41 ns |  58.80 ns | 
| Large_String_Interpolated | 16,283.3 ns |  68.37 ns |  60.61 ns | 
|        Large_String_Added | 16,384.8 ns | 286.11 ns | 238.91 ns |
|       Large_StringBuilder | 34,526.7 ns | 498.72 ns | 416.45 ns | 


``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.17763.775 (1809/October2018Update/Redstone5)
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 8 logical and 8 physical cores
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-PNLSCM : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

Runtime=.NET Core 3.0  

```
|                    Method |        Mean |     Error |    StdDev |
|-------------------------- |------------:|----------:|----------:|
|         LiteStringBuilder |    445.8 ns |   1.29 ns |   1.21 ns |
|              String_Added |    447.9 ns |   4.29 ns |   4.01 ns | 
|             String_Concat |    491.6 ns |  12.36 ns |  14.71 ns | 
|             StringBuilder |    584.9 ns |   5.99 ns |   5.31 ns |
|       String_Interpolated |    634.8 ns |  12.46 ns |  13.85 ns |
|   Large_LiteStringBuilder |  9,402.2 ns | 104.21 ns |  97.48 ns | 
|       Large_String_Concat | 11,581.3 ns | 145.70 ns | 129.16 ns |
|        Large_String_Added | 11,709.0 ns | 252.74 ns | 280.92 ns |
| Large_String_Interpolated | 11,992.4 ns | 171.29 ns | 143.03 ns |
|       Large_StringBuilder | 28,484.4 ns | 175.88 ns | 146.86 ns |

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


