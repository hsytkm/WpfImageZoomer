using System.Windows.Media.Imaging;

namespace ImageZoomer;

internal sealed class MainWindowViewModel : BindableBase
{
    private IDisposable? _disposable;

    public string? ImagePath
    {
        get => _imagePath;
        set
        {
            if (SetProperty(ref _imagePath, value))
            {
                var path = value ?? "";

                if (path.Length > 2 && path[0] == '\"' && path[path.Length - 1] == '\"')
                    path = path.Substring(1, path.Length - 2);

                Bitmap = BitmapImageExtension.FromFile(path);
            }
        }
    }
    private string? _imagePath;

    public int ScaleValue
    {
        get => _scaleValue;
        set => SetProperty(ref _scaleValue, value);
    }
    private int _scaleValue = 4;

    public BitmapSource? Bitmap
    {
        get => _bitmap;
        private set => SetProperty(ref _bitmap, value);
    }
    private BitmapSource? _bitmap;

    public MainWindowViewModel() { }

    public RelayCommand CopyCommand => new(() =>
    {
        _disposable?.Dispose();
        _disposable = Bitmap?.CopyToClipboard(ScaleValue);
    });
}
