using System;

namespace LiveDisplay.Services.Configuration 
{
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string Key { get; set; }

        public object Value { get; set; }
        public Type ValueType { get; set; }

    }

}