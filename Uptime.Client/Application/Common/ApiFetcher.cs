using Uptime.Client.StateManagement.Common;
using Uptime.Shared.Common;
using System.ComponentModel;

namespace Uptime.Client.Application.Common;

public class ApiFetcher<T>(Func<Task<Result<T>>> fetchFunction) : INotifyPropertyChanged
{
    private QueryStatus _status = QueryStatus.Loading;
    public QueryStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }
    }

    private Result<T>? _result;
    public Result<T>? Result
    {
        get => _result;
        set
        {
            if (_result != value)
            {
                _result = value;
                NotifyPropertyChanged();
            }
        }
    }

    public async Task FetchAsync()
    {
        Status = QueryStatus.Loading;

        await Task.Delay(3000);

        try
        {
            Result = await fetchFunction();
        }
        catch (Exception ex)
        {
            Result = Result<T>.Failure(ex.Message);
        }

        Status = QueryStatus.Loaded;
    }

    public void Reset()
    {
        Result = null;
        Status = QueryStatus.Loading;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}