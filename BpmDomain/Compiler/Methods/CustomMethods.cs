using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Methods;

public class CustomMethods(List<JsonObject> fields, JsonObject field, List<JsonObject>? warnings)
{
    private readonly List<JsonObject> _fields = fields;
    private readonly JsonObject _field = field;
    private readonly List<JsonObject>? _warnings = warnings;


    public List<JsonObject> AlternativeViewControl(bool showAlternativeView, object? _object = null)
    {
        var baseMethods = new BaseMethods(_fields, _field, _warnings);

        var name = baseMethods.GetPropertyByName("ZVEwsAccantonamenti", "name");

        if (name.Equals("ZVEwsAccantonamenti"))
        {
            baseMethods.SetPropertyByName(name, "AccantonamentiNonPrevisti", showAlternativeView);
        }

        name = baseMethods.GetPropertyByName("ZVEwsFidi", "name");

        if (name.Equals("ZVEwsFidi"))
        {
            baseMethods.SetPropertyByName(name, "FidiNonPrevisti", showAlternativeView);
        }

        name = baseMethods.GetPropertyByName("ZVEwsFidiDiRinnovo", "name");

        if (name.Equals("ZVEwsFidiDiRinnovo"))
        {
            baseMethods.SetPropertyByName(name, "FidiNonPrevisti", showAlternativeView);
        }

        return _fields;
    }

    public List<JsonObject> SetIRRPrecedente(bool usaOOIRP, object? _object = null)
    {
        var baseMethods = new BaseMethods(_fields, _field, _warnings);

        var name = baseMethods.GetPropertyByName("ZVEwsAccantonamenti", "name");

        if (name.Equals("ZVEwsAccantonamenti"))
        {
            var actualState = baseMethods.GetPropertyByName(name, "UsaOOIRP");
            baseMethods.SetPropertyByName(name, "UsaOOIRP", usaOOIRP);
            baseMethods.SetPropertyByName(name, "Modificata", actualState.ToString() != String.Empty && actualState != usaOOIRP);
        }

        return _fields;
    }

    public List<JsonObject> SetSalvaStrategiaNPL(bool salvaStrategiaNPL, object? _object = null)
    {
        var baseMethods = new BaseMethods(_fields, _field, _warnings);

        var name = baseMethods.GetPropertyByName("ZVEwsStrategie", "name");

        if (name.Equals("ZVEwsStrategie"))
        {
            var actualState = baseMethods.GetPropertyByName(name, "SalvaStrategiaNPL");
            baseMethods.SetPropertyByName(name, "SalvaStrategiaNPL", salvaStrategiaNPL);
            baseMethods.SetPropertyByName(name, "Modificata", actualState.ToString() != String.Empty && actualState != salvaStrategiaNPL);
        }

        return _fields;
    }
}