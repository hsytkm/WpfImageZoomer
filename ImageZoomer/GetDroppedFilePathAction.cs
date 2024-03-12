using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ImageZoomer;

public sealed class GetDroppedFilePathAction : TriggerAction<DependencyObject>
{
    // ドロップされた先頭ファイルのPATH
    public static readonly DependencyProperty DroppedPathProperty =
        DependencyProperty.Register(nameof(DroppedPath), typeof(string), typeof(GetDroppedFilePathAction));
    public string? DroppedPath
    {
        get => (string?)GetValue(DroppedPathProperty);
        set => SetValue(DroppedPathProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is UIElement element)
            element.AllowDrop = true;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is UIElement element)
            element.AllowDrop = false;

        base.OnDetaching();
    }

    protected override void Invoke(object parameter)
    {
        if (parameter is not DragEventArgs e)
            return;

        var paths = getDroppedPaths(e.Data);
        if (getHeadFilePath(paths) is { } file)
            DroppedPath = file;

        static IEnumerable<string> getDroppedPaths(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is string[] ss)
                {
                    foreach (var s in ss)
                        yield return s;
                }
                else
                {
                    throw new FormatException();
                }
            }
            else
            {
                var path = data.GetData(DataFormats.Text)?.ToString() ?? "";
                yield return path;
            }
        }

        static string? getHeadFilePath(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (System.IO.File.Exists(path))
                    return path;
            }
            return null;
        }
    }
}
