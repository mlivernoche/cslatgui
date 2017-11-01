using CslAutomatedTestApi;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace CSLAutomatedTestApp
{
    [AddINotifyPropertyChangedInterface]
    public class TestParametersCollection : ObservableCollection<CudaTestParameters>
    {
        public TestParametersCollection() : base()
        {
        }
    }
}
