using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Metasia.Core.Objects;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reflection;
using Metasia.Core.Attributes;
using System.Collections.Generic;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ObjectTypeInfo
{
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Type ObjectType { get; set; } = typeof(Text);
    public string Identifier { get; set; } = string.Empty;
}

public class NewObjectSelectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IMetasiaObject?> OkCommand { get; }
    public ReactiveCommand<Unit, IMetasiaObject?> CancelCommand { get; }

    public ObservableCollection<ObjectTypeInfo> AvailableObjectTypes { get; } = new();
    public ObservableCollection<ObjectTypeInfo> FilteredObjectTypes { get; } = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

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

        // 検索テキストの変更を監視してフィルタリングを実行
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => FilterObjectTypes());
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
                ObjectType = objectType.Type,
                Identifier = identifier
            });
        }

        // 初期状態ではすべてのオブジェクトを表示
        FilterObjectTypes();

        if (FilteredObjectTypes.Count > 0)
        {
            SelectedObjectType = FilteredObjectTypes[0];
        }
    }

    private void FilterObjectTypes()
    {
        FilteredObjectTypes.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // 検索テキストが空の場合はすべて表示
            foreach (var item in AvailableObjectTypes)
            {
                FilteredObjectTypes.Add(item);
            }
        }
        else
        {
            // 検索テキストでフィルタリング
            var searchText = SearchText.ToLowerInvariant();
            var filteredItems = AvailableObjectTypes.Where(item =>
                item.DisplayName.ToLowerInvariant().Contains(searchText) ||
                item.Description.ToLowerInvariant().Contains(searchText) ||
                item.Identifier.ToLowerInvariant().Contains(searchText));

            foreach (var item in filteredItems)
            {
                FilteredObjectTypes.Add(item);
            }
        }

        // 現在の選択がフィルタ結果に含まれていない場合は最初の項目を選択
        if (SelectedObjectType is null || !FilteredObjectTypes.Contains(SelectedObjectType))
        {
            SelectedObjectType = FilteredObjectTypes.FirstOrDefault();
        }
    }

    private string GetDisplayNameFromIdentifier(string identifier)
    {
        return identifier switch
        {
            "Text" => "テキスト",
            "HelloObject" => "kariHelloオブジェクト",
            "ImageObject" => "画像",
            "VideoObject" => "動画",
            "AudioObject" => "音声",
            "Layer" => "レイヤー",
            _ => identifier
        };
    }
}
