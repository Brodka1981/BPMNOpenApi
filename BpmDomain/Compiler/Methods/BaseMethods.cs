using BpmDomain.Common;
using BpmInfrastructure.Common;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Methods;

public class BaseMethods(List<JsonObject> fields, JsonObject field, List<JsonObject>? warnings)
{
    private readonly List<JsonObject> _fields = fields;
    private readonly JsonObject _field = field;
    private readonly List<JsonObject>? _warnings = warnings;

    public dynamic GetProperty(string propertyName)
    {
        dynamic? result = null;

        try
        {
            result = _field != null && _field.ContainsKey(propertyName) ? _field[propertyName].GetValueFromObject() : null;
        }
        catch (Exception)
        {
            //nothing
        }

        result ??= String.Empty;

        return result;
    }

    public List<JsonObject> SetPropertyByName(string name, string propertyName, dynamic propertyValue)
    {
        try
        {
            foreach (var item in _fields)
            {
                string? _name = item["name"]?.ToString();
                bool propertyExists = item[propertyName]?.ToString() != null;
                if (_name != null && _name.Equals(name) && !propertyExists)
                    item.Add(propertyName, propertyValue);
                else if (_name != null && _name.Equals(name) && propertyExists)
                    item[propertyName] = propertyValue;
            }
        }
        catch (Exception)
        {
            //nothing
        }

        return _fields;
    }

    public dynamic GetPropertyByName(string name, string propertyName)
    {
        dynamic? result = null;

        try
        {
            foreach (var item in _fields)
            {
                string? _name = item["name"]?.ToString();
                if (_name != null && _name.Equals(name))
                    result = item?[propertyName].GetValueFromObject();
            }
        }
        catch (Exception)
        {
            //nothing
        }

        result ??= String.Empty;

        return result;
    }

    public JsonObject? GetJsonObjectByName(string name)
    {
        JsonObject? result = null;

        try
        {
            foreach (var item in _fields)
            {
                string? _name = item["name"]?.ToString();
                if (_name != null && _name.Equals(name))
                    result = item;
            }
        }
        catch (Exception)
        {
            //nothing
        }

        return result;
    }

    public string GetPropertyValueByEvidenza(string evidenzaName)
    {
        dynamic? result = null;

        try
        {
            foreach (var item in _warnings.ToListJsonObject())
            {
                string? _name = item[evidenzaName]?.ToString();
                if (_name != null)
                    result = item?[evidenzaName].GetValueFromObject();
            }
        }
        catch (Exception)
        {
            //nothing
        }

        result ??= String.Empty;

        return result;
    }

    public dynamic GetPropertyValue()
    {
        return GetProperty("value");
    }

    public dynamic GetPropertyValueByName(string name)
    {
        return GetPropertyByName(name, "value");
    }

    public List<JsonObject> SetPropertyVisibleByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, "visible", propertyValue);
    }

    public List<JsonObject> SetPropertyValueByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, "value", propertyValue);
    }

    public List<JsonObject> SetEnabledByNameAndEvidenza(string name, string evidenzaName)
    {
        List<JsonObject> result;

        var evidenzaValue = GetPropertyValueByEvidenza(evidenzaName);

        if (evidenzaValue != null && evidenzaValue != String.Empty)
            result = SetPropertyByName(name, "enable", true);
        else
            result = SetPropertyByName(name, "enable", false);

        return result;
    }
}