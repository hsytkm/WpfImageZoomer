using System.Reflection;
using ImageZoomer;

public class Program
{
    [STAThread]
    public static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

        App app = new();
        app.InitializeComponent();
        app.Run(new MainWindow());
    }

    // for test
    internal static System.Text.StringBuilder LoadedAssemblies { get; } = new();

    private static Assembly? OnResolveAssembly(object sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);
        string path = assemblyName.Name + ".dll";
        if (!assemblyName.CultureInfo.Equals(System.Globalization.CultureInfo.InvariantCulture))
        {
            path = $"{assemblyName.CultureInfo}\\" + path;
        }

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

        if (stream is null)
            return null;

        // 以降は同フォルダにdllが存在しない場合のみ通るようです
        LoadedAssemblies.AppendLine($"{args.Name} : Size={stream.Length} bytes");

        stream.Position = 0;
        var assemblyRawBytes = new byte[stream.Length];
        stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
        return Assembly.Load(assemblyRawBytes);
    }
}