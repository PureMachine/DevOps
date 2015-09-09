using GalaSoft.MvvmLight;

namespace OctopusManager.ViewModel
{
    public class SelectProjectViewModel : ObservableObject
    {
        private bool _isSelected;

        public string Id { get; set; }
        public string Name { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    RaisePropertyChanged(() => IsSelected);
                }
            }
        }
    }
}
