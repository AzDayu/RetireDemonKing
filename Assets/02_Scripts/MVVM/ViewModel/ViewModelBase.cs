using System.ComponentModel;

public class ViewModelBase : INotifyPropertyChanged
{
    // INotifyPropertyChanged를 쓸려면 꼭 필요한 부분을 Base로 그냥 통합
    // Model 구현할 때 이걸 상속받으면 끝

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}