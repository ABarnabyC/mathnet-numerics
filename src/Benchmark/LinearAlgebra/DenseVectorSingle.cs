using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.Providers.Common.Mkl;
using MathNet.Numerics.Providers.LinearAlgebra;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System.Linq;
using Complex = System.Numerics.Complex;

namespace Benchmark.LinearAlgebra
{
    //[DisassemblyDiagnoser(printSource:true,exportHtml:true, exportCombinedDisassemblyReport:true)]
    [Config(typeof(Config))]
    public class DenseVectorFloat
    {
        class Config : ManualConfig
        {
            public Config()
            {
                //AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
                //AddJob(Job.Default.WithRuntime(ClrRuntime.Net461).WithPlatform(Platform.X86).WithJit(Jit.LegacyJit));
#if !NET461
                AddJob(Job.Default.WithRuntime(CoreRuntime.Core31).WithPlatform(Platform.X64).WithJit(Jit.RyuJit));
#endif
            }
        }

        public enum ProviderId
        {
            Managed,
            ManagedReference,
            NativeMKL,
        }

        [Params(4, 32, 128, 4096, 16384, 131072, 524288)]
        public int N { get; set; }

        [ParamsAllValues]
        public bool UseSIMD { get; set; }

        [Params(ProviderId.Managed)]//, ProviderId.ManagedReference)] //, ProviderId.NativeMKL)]
        public ProviderId Provider { get; set; }

        float[] r;
        float[] _a;
        float[] _b;
        Complex[] _ac;
        Complex[] _bc;
        //Vector<float> _av;
        //Vector<float> _bv;

        [GlobalSetup]
        public void Setup()
        {
            switch (Provider)
            {
                case ProviderId.Managed:
                    Control.UseManaged();
                    Control.UseSIMD = UseSIMD;
                    break;
                case ProviderId.ManagedReference:
                    Control.UseManagedReference();
                    break;
                case ProviderId.NativeMKL:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Single, MklAccuracy.High);
                    break;
            }

            _a = Generate.Normal(N, 2.0, 10.0).Select((x)=>(float)x).ToArray();
            _b = Generate.Normal(N, 200.0, 10.0).Select((x) => (float)x).ToArray();
            //_ac = Generate.Map2(_a, Generate.Normal(N, 2.0, 10.0), (a, i) => new Complex(a, i));
            //_bc = Generate.Map2(_b, Generate.Normal(N, 200.0, 10.0), (b, i) => new Complex(b, i));
            //_av = Vector<float>.Build.Dense(_a);
            //_bv = Vector<float>.Build.Dense(_b);

            r = new float[N];
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderAddArrays()
        {
            LinearAlgebraControl.Provider.AddVectorToScaledVector(_a, 2.0f, _b, r);
            return r;

            //Complex[] r = new Complex[_a.Length];
            //LinearAlgebraControl.Provider.AddArrays(_ac, _bc, r);
            //return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderAddScaledArrays()
        {
            LinearAlgebraControl.Provider.AddArrays(_a, _b, r);
            return r;

            //Complex[] r = new Complex[_a.Length];
            //LinearAlgebraControl.Provider.AddArrays(_ac, _bc, r);
            //return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderScaleArrays()
        {
            LinearAlgebraControl.Provider.ScaleArray(2.0f, _a, r);
            return r;
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public float ProviderDotProduct()
        {
            return LinearAlgebraControl.Provider.DotProduct(_a, _b);
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderPointMultiply()
        {
            LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_a, _b, r);
            return r;

            //Complex[] r = new Complex[_a.Length];
            //LinearAlgebraControl.Provider.PointWiseMultiplyArrays(_ac, _bc, r);
            //return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderPointDivide()
        {
            LinearAlgebraControl.Provider.PointWiseDivideArrays(_a, _b, r);
            return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        public float[] ProviderPointPower()
        {
            LinearAlgebraControl.Provider.PointWisePowerArrays(_a, _b, r);
            return r;
        }

        //[Benchmark(OperationsPerInvoke = 1)]
        //public Vector<float> VectorAddOp()
        //{
        //    return _av + _bv;
        //}

        //[Benchmark(OperationsPerInvoke = 1, Baseline = true)]
        //public float[] ForLoop()
        //{
        //    float[] r = new float[_a.Length];
        //    for (int i = 0; i < r.Length; i++)
        //    {
        //        r[i] = _a[i] + _b[i];
        //    }

        //    return r;
        //}
    }
}
