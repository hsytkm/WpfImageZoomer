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
        private set
        {
            if (SetProperty(ref _bitmap, value))
            {
                // 画像更新の度にクリップボードに代入してみています
                CopyToClipboard(_bitmap, ScaleValue);
            }
        }
    }
    private BitmapSource? _bitmap;

    public MainWindowViewModel() { }

    public RelayCommand CopyCommand => new(() => CopyToClipboard(Bitmap, ScaleValue));

    private void CopyToClipboard(BitmapSource? bitmap, int scaleValue)
    {
        _disposable?.Dispose();

        if (bitmap != null)
            _disposable = bitmap?.CopyToClipboard(scaleValue);
    }
}
