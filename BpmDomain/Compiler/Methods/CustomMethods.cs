using BpmDomain.Compiler.Models;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Methods;

public class CustomMethods(MethodsParameters methodsParameters)
{
    private readonly MethodsParameters _methodsParameters = methodsParameters;

    public List<JsonObject>? AlternativeViewControl(bool showAlternativeView)
    {
        var baseMethods = new BaseMethods(_methodsParameters);

        var name = baseMethods.GetProperty("name").ToString();

        if (name.Equals("ZVEwsAccantonamenti"))
        {
            baseMethods.SetProperty("AccantonamentiNonPrevisti", showAlternativeView);
        }

        if (name.Equals("ZVEwsFidi"))
        {
            baseMethods.SetProperty("FidiNonPrevisti", showAlternativeView);
        }

        if (name.Equals("ZVEwsFidiDiRinnovo"))
        {
            baseMethods.SetProperty("FidiNonPrevisti", showAlternativeView);
        }

        return _methodsParameters.Fields;
    }

    public List<JsonObject>? SetIRRPrecedente(bool usaOOIRP)
    {
        var baseMethods = new BaseMethods(_methodsParameters);

        var name = baseMethods.GetProperty("name").ToString();

        if (name.Equals("ZVEwsAccantonamenti"))
        {
            var usaOOIRPProperty = baseMethods.GetProperty("UsaOOIRP")?.ToString();
            bool.TryParse(usaOOIRPProperty, out bool actualState);
            baseMethods.SetProperty("UsaOOIRP", usaOOIRP);
            baseMethods.SetProperty("Modificata", usaOOIRPProperty != String.Empty && actualState != usaOOIRP);
        }

        return _methodsParameters.Fields;
    }

    public List<JsonObject>? SetIRRPrecedenteByName(string name, bool usaOOIRP)
    {
        var baseMethods = new BaseMethods(_methodsParameters);

        if (name.Equals("ZVEwsAccantonamenti"))
        {
            var usaOOIRPProperty = baseMethods.GetProperty("UsaOOIRP")?.ToString();
            bool.TryParse(usaOOIRPProperty, out bool actualState);
            baseMethods.SetPropertyByName(name, "UsaOOIRP", usaOOIRP);
            baseMethods.SetPropertyByName(name, "Modificata", usaOOIRPProperty != String.Empty && actualState != usaOOIRP);
        }

        return _methodsParameters.Fields;
    }

    public List<JsonObject>? SetSalvaStrategiaNPL(bool salvaStrategiaNPL)
    {
        var baseMethods = new BaseMethods(_methodsParameters);

        var name = baseMethods.GetProperty("name").ToString();

        if (name.Equals("ZVEwsStrategie"))
        {
            var salvaStrategiaNPLProperty = baseMethods.GetProperty("SalvaStrategiaNPL")?.ToString();
            bool.TryParse(salvaStrategiaNPLProperty, out bool actualState);
            baseMethods.SetProperty("SalvaStrategiaNPL", salvaStrategiaNPL);
            baseMethods.SetProperty("Modificata", salvaStrategiaNPLProperty != String.Empty && actualState != salvaStrategiaNPL);
        }

        return _methodsParameters.Fields;
    }
}