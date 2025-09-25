using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Metasia.Core.Objects;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reflection;
using Metasia.Core.Attributes;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ObjectTypeInfo
{
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Type ObjectType { get; set; } = typeof(Text);
}

public class NewObjectSelectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IMetasiaObject?> OkCommand { get; }
    public ReactiveCommand<Unit, IMetasiaObject?> CancelCommand { get; }

    public ObservableCollection<ObjectTypeInfo> AvailableObjectTypes { get; } = new();

    private ObjectTypeInfo? _selectedObjectType;
    public ObjectTypeInfo? SelectedObjectType
    {
        get => _selectedObjectType;
        set => this.RaiseAndSetIfChanged(ref _selectedObjectType, value);
    }

    public NewObjectSelectViewModel()
    {
        LoadAvailableObjectTypes();

        var canExecuteOk = this.WhenAnyValue(x => x.SelectedObjectType)
            .Select(selected => selected is not null);

        OkCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedObjectType?.ObjectType is not null)
            {
                var instance = Activator.CreateInstance(SelectedObjectType.ObjectType) as IMetasiaObject;
                if (instance is not null)
                {
                    // ランダムなUUIDを生成してIDに設定
                    instance.Id = Guid.NewGuid().ToString();
                }
                return instance;
            }
            return null;
        }, canExecuteOk);

        CancelCommand = ReactiveCommand.Create(() => (IMetasiaObject?)null);
    }

    private void LoadAvailableObjectTypes()
    {
        AvailableObjectTypes.Clear();

        // ClipTypeIdentifier属性を持つクラスを自動で収集
        var objectTypes = Assembly.GetAssembly(typeof(ClipObject))!
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ClipObject)))
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<ClipTypeIdentifierAttribute>()
            })
            .Where(x => x.Attribute is not null)
            .OrderBy(x => x.Attribute!.Identifier)
            .ToList();

        foreach (var objectType in objectTypes)
        {
            var identifier = objectType.Attribute!.Identifier;
            var displayName = GetDisplayNameFromIdentifier(identifier);
            var description = $"{displayName}オブジェクトを追加します";

            AvailableObjectTypes.Add(new ObjectTypeInfo
            {
                DisplayName = displayName,
                Description = description,
                ObjectType = objectType.Type
            });
        }

        if (AvailableObjectTypes.Count > 0)
        {
            SelectedObjectType = AvailableObjectTypes[0];
        }
    }

    private string GetDisplayNameFromIdentifier(string identifier)
    {
        return identifier switch
        {
            "Text" => "テキスト",
            "HelloObject" => "kariHelloオブジェクト",
            "Layer" => "レイヤー",
            _ => identifier
        };
    }
}