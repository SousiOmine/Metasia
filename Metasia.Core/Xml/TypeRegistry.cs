using System.Reflection;
using Metasia.Core.Objects;
using Metasia.Core.Coordinate.InterpolationLogic;

namespace Metasia.Core.Xml;

/// <summary>
/// シリアライズ、デシリアライズの際に、文字列の型IDと実際のTypeを対応付ける
/// </summary>
public class TypeRegistry
{
    private readonly Dictionary<string, Type> _types = new();

    private readonly Dictionary<Type, string> _typeIds = new();

    private readonly Dictionary<string, string> _typeIdByTypeName= new();

    public void Register(string prefix, string typeName, Type type)
    {
        _types[prefix + ":" + typeName] = type;
        _typeIds[type] = prefix + ":" + typeName;
        _typeIdByTypeName[type.Name] = _typeIds[type];
    }

    public void RegisterAssemblyTypes(string prefix, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            // 静的クラス、抽象クラス、インターフェースは除外
            if (type.IsAbstract || type.IsInterface)
                continue;
            // IMetasiaObjectを実装していない型も除外
            if (!typeof(IMetasiaObject).IsAssignableFrom(type) && !typeof(IInterpolationLogic).IsAssignableFrom(type))
                continue;
            Register(prefix, type.Name, type);
        }
    }

    public Type? GetType(string typeId)
    {
        if (_types.TryGetValue(typeId, out var type))
        {
            return type;
        }
        return null;
    }

    public string? GetTypeId(Type type)
    {
        if (_typeIds.TryGetValue(type, out var typeId))
        {
            return typeId;
        }
        return null;
    }

    public string? GetTypeIdByTypeName(string typeName)
    {
        if (_typeIdByTypeName.TryGetValue(typeName, out var typeId))
        {
            return typeId;
        }
        return null;
    }

    public Type? GetTypeByTypeName(string typeName)
    {
        var typeId = GetTypeIdByTypeName(typeName);
        return typeId is null ? null : GetType(typeId);
    }

    public Type[] GetAllRegisteredTypes()
    {
        return _types.Values.ToArray();
    }
}
