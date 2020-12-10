using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Runtime.CompilerServices;

namespace ProjectCodeEditor.Helpers
{
    /// <summary>
    /// Extends observable object with BitArray
    /// </summary>
    public class FlagsModel : ObservableObject
    {
        protected FlagsModel(int flagCapacity)
        {
            FlagsStorage = new(flagCapacity);
        }

        protected void SetFlag(int index, bool value, [CallerMemberName] string propertyName = null)
        {
            if (FlagsStorage[index] != value)
            {
                FlagsStorage[index] = value;
                OnPropertyChanged(propertyName);
            }
        }

        protected bool GetFlag(int index) => FlagsStorage[index];

        private readonly BitArray FlagsStorage;
    }
}
