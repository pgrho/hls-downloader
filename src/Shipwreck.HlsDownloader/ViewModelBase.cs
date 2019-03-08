using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shipwreck.HlsDownloader
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (field?.Equals(value) ?? value?.Equals(field) ?? true)
            {
                return false;
            }
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T? field, T? value, [CallerMemberName]string propertyName = null)
            where T : struct, IEquatable<T>
        {
            if (field?.Equals(value) ?? value?.Equals(field) ?? true)
            {
                return false;
            }
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}