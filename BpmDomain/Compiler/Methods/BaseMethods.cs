using BpmDomain.Common;
using BpmDomain.Compiler.Models;
using BpmDomain.NLog;
using BpmInfrastructure.Common;
using NLog;
using System.Reflection;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Methods;

public class BaseMethods(MethodsParameters methodsParameters)
{
    private readonly MethodsParameters _methodsParameters = methodsParameters;
    const string propertyValueConst = "value";
    const string propertyVisibleConst = "visible";
    const string propertyCssClassConst = "cssClass";
    const string propertyEnableConst = "enable";
    const string propertyExecutableConst = "executable";
    const string propertyNameConst = "name";
    const string propertyErrorConst = "error";
    const string propertyValuesConst = "values";
    const string propertyColorConst = "color";

    public dynamic GetProperty(string propertyName)
    {
        dynamic? result = null;

        try
        {
            result = _methodsParameters.Field != null && _methodsParameters.Field.ContainsKey(propertyName) ? _methodsParameters.Field[propertyName].GetValueFromObject() : null;
        }
        catch (Exception)
        {
            //nothing
        }

        result ??= String.Empty;

        return result;
    }

    public JsonObject? SetProperty(string propertyName, dynamic propertyValue)
    {
        try
        {
            if(_methodsParameters.Field != null)
                _methodsParameters.Field[propertyName] = propertyValue;
        }
        catch (Exception)
        {
            //nothing
        }

        return _methodsParameters.Field;
    }

    public List<JsonObject>? SetPropertyByName(string name, string propertyName, dynamic propertyValue)
    {
        try
        {
            foreach (var item in _methodsParameters.Fields.ToListJsonObject())
            {
                string? _name = item[propertyNameConst]?.ToString();
                if (_name != null && _name.Equals(name))
                    item[propertyName] = propertyValue;
            }
        }
        catch (Exception)
        {
            //nothing
        }

        return _methodsParameters.Fields;
    }

    public dynamic? GetPropertyByName(string name, string propertyName)
    {
        dynamic? result = null;

        try
        {
            foreach (var item in _methodsParameters.Fields.ToListJsonObject())
            {
                string? _name = item[propertyNameConst]?.ToString();
                if (_name != null && _name.Equals(name))
                    result = item?[propertyName].GetValueFromObject();
            }
        }
        catch (Exception)
        {
            //nothing
        }

        return result;
    }

    public string GetValueByWarning(string warningName)
    {
        dynamic? result = null;

        try
        {
            foreach (var item in _methodsParameters.Warnings.ToListJsonObject())
            {
                string? _name = item[warningName]?.ToString();
                if (_name != null)
                    result = item?[warningName].GetValueFromObject();
            }
        }
        catch (Exception)
        {
            //nothing
        }

        result ??= String.Empty;

        return result.ToString();
    }

    public dynamic GetPropertyValue()
    {
        return GetProperty(propertyValueConst);
    }

    public string GetPropertyValueInString()
    {
        return GetProperty(propertyValueConst).ToString();
    }

    public dynamic GetPropertyValues()
    {
        return GetProperty(propertyValuesConst);
    }

    public dynamic GetPropertyValueByName(string name)
    {
        var result = GetPropertyByName(name, propertyValueConst);
        result ??= String.Empty;
        return result;
    }

    public dynamic? GetPropertyValueNullableByName(string name)
    {
        return GetPropertyByName(name, propertyValueConst);
    }

    public List<JsonObject> SetPropertyVisibleByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyVisibleConst, propertyValue);
    }

    public List<JsonObject> SetPropertyValueByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, BaseMethods.propertyValueConst, propertyValue);
    }

    public List<JsonObject> SetPropertyCssClassByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyCssClassConst, propertyValue);
    }

    public List<JsonObject>? SetEnabledByNameAndWarning(string name, string warningName)
    {
        List<JsonObject>? result;

        var warningValue = GetValueByWarning(warningName);

        if (warningValue != null && warningValue != String.Empty)
            result = SetPropertyByName(name, propertyEnableConst, true);
        else
            result = SetPropertyByName(name, propertyEnableConst, false);

        return result;
    }

    public void WriteInfoLog(string log)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));
        logger.Info(log);
    }

    public void WriteErrorLog(string log)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));
        logger.Error(log);
    }

    public List<JsonObject> SetPropertyExecutableByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyExecutableConst, propertyValue);
    }

    public List<JsonObject> SetPropertyExecutable(dynamic propertyValue)
    {
        return SetProperty(propertyExecutableConst, propertyValue);
    }

    public List<JsonObject> SetPropertyErrorByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyErrorConst, propertyValue);
    }

    public List<JsonObject> SetPropertyEnableByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyEnableConst, propertyValue);
    }

    public List<JsonObject> SetPropertyColorByName(string name, dynamic propertyValue)
    {
        return SetPropertyByName(name, propertyColorConst, propertyValue);
    }
}