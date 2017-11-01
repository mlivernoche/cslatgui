using CudaSharper;
using PropertyChanged;
using System.Collections.ObjectModel;

namespace CSLAutomatedTestApp
{
    [AddINotifyPropertyChangedInterface]
    public class DetectedGpuCollection : ObservableCollection<CudaDevice>
    {
        public DetectedGpuCollection()
        {
            for(int i = 0; i < CudaSettings.CudaDeviceCount; i++)
            {
                Add(new CudaDevice(i, 0));
            }
        }
    }
}
