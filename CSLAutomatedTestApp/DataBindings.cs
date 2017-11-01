using CslAutomatedTestApi;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace CSLAutomatedTestApp
{
    [AddINotifyPropertyChangedInterface]
    public class DataBindings
    {
        public TestParametersCollection ParametersList { get; }
        public DetectedGpuCollection GpuList { get; }
        public TestParallelismCollection ParallelismOptions { get; }

        public DataBindings()
        {
            ParametersList = new TestParametersCollection();
            GpuList = new DetectedGpuCollection();
            ParallelismOptions = new TestParallelismCollection();
        }

        public void Add(CudaTestParameters para)
        {
            ParametersList.Add(para);
        }
    }
}
