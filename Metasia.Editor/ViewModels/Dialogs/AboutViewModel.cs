using System;
using System.Reflection;

namespace Metasia.Editor.ViewModels.Dialogs;

public class AboutViewModel : ViewModelBase
{
    public string ProductName { get; }
    public string Version { get; }
    public string Copyright { get; }

    public AboutViewModel()
    {
        var assembly = Assembly.GetEntryAssembly();

        var productAttr = assembly?.GetCustomAttribute<AssemblyProductAttribute>();
        ProductName = productAttr?.Product ?? "Metasia Editor";

        var version = assembly?.GetName().Version;
        Version = version is not null
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "0.1.0";

        Copyright = $"Copyright (C) 2026 SousiOmine";
    }
}
