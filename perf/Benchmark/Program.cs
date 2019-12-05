using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {

            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(MemoryDiagnoser.Default);
      

            //  AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            BenchmarkRunner.Run<StringBenchmark>(
       config
                //.With(Job.RyuJitX64)
              //  .With(Job.Clr)
                 //      .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
                 // .With(new BenchmarkDotNet.Diagnosers.CompositeDiagnoser())
                 .With(ExecutionValidator.FailOnError)
                 .With(Job.Default.With(ClrRuntime.Net48))

             );
            Console.WriteLine("DONE!!!!");
            Console.ReadLine();
        }
    }
}
