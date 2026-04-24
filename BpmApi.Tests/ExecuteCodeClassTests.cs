using BpmDomain.Compiler;
using BpmInfrastructure.Common;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BpmApi.Tests
{
    [TestFixture]
    public class ExecuteCodeClassTests
    {
        private ExecuteCodeClass? _executeCodeClass;
        private List<JsonObject> _fields = [];
        private List<JsonObject> _variables = [];
        private string? _code;

        [SetUp]
        public Task SetUp()
        {
            PopulateData();

            return Task.CompletedTask;
        }

        [Test]
        public void BaseMethods_GetPropertyValue_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().GetPropertyValue();

            var response = result ?? String.Empty;

            //Assert
            Assert.That(_field["value"]?.ToString() == response?.ToString());
        }

        [Test]
        public void BaseMethods_GetPropertyValueInString_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().GetPropertyValueInString();

            var response = result ?? String.Empty;

            //Assert
            Assert.That(_field["value"]?.ToString() == response);
        }

        [Test]
        public void BaseMethods_GetPropertyValueByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().GetPropertyValueByName("field1");

            var response = result ?? String.Empty;

            //Assert
            Assert.That(_fields[1]["value"]?.ToString() == response?.ToString());
        }

        [Test]
        public void BaseMethods_GetPropertyValueNullableByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().GetPropertyValueNullableByName("field1");

            var response = result ?? String.Empty;

            //Assert
            Assert.That(_fields[1]["value"]?.ToString() == response?.ToString());
        }

        [Test]
        public void BaseMethods_SetPropertyVisibleByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyVisibleByName("field1", true);

            var response = result ?? [];

            //Assert
            Assert.That((response?[1]?["visible"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void BaseMethods_SetPropertyColorByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyColorByName("field1", "red");

            var response = result ?? [];

            //Assert
            Assert.That(response?[1]["color"]?.ToString(), Is.EqualTo("red"));
        }

        public void BaseMethods_SetPropertyExecutableByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyExecutableByName("field1", true);

            var response = result ?? [];

            //Assert
            Assert.That((response?[1]?["executable"].ToBoolFromObject()), Is.True);
        }

        public void BaseMethods_SetPropertyErrorByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyErrorByName("field1", "errore di test");

            var response = result ?? [];

            //Assert
            Assert.That(response?[1]["value"]?.ToString(), Is.EqualTo("errore di test"));
        }

        public void BaseMethods_SetPropertyEnableByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyEnableByName("field1", true);

            var response = result ?? [];

            //Assert
            Assert.That((response?[1]?["enable"].ToBoolFromObject()), Is.True);
        }

        public void BaseMethods_SetPropertyExecutable_InputIsValid_ReturnTrue()
        {
            var _field = _fields[1];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyExecutable(true);

            var response = result ?? [];

            //Assert
            Assert.That((response?[1]?["executable"].ToBoolFromObject()), Is.True);
        }


        [Test]
        public void BaseMethods_SetPropertyValueByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyValueByName("field1" ,"new value");

            var response = result ?? [];

            //Assert
            Assert.That(response[1]["value"]?.ToString(), Is.EqualTo("new value"));
        }

        [Test]
        public void BaseMethods_SetPropertyCssClassByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetPropertyCssClassByName("field1", "new value");

            var response = result ?? [];

            //Assert
            Assert.That(response[1]["cssClass"]?.ToString(), Is.EqualTo("new value"));
        }

        [Test]
        public void BaseMethods_SetEnabledByNameAndWarning_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().SetEnabledByNameAndWarning("field1", "warningName2");

            var response = result ?? [];

            //Assert
            Assert.That((response?[1]?["enable"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void BaseMethods_WriteInfoLog_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //Assert and execute test
            Assert.DoesNotThrow(() => _executeCodeClass.BaseMethods().WriteInfoLog("ExecuteCodeClass WriteInfoLog UnitTest test"));
        }

        [Test]
        public void BaseMethods_WriteErrorLog_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //Assert and execute test
            Assert.DoesNotThrow(() => _executeCodeClass.BaseMethods().WriteErrorLog("ExecuteCodeClass WriteErrorLog UnitTest  test"));
        }

        [Test]
        public void CustomMethods_AlternativeViewControl_InputIsValid_ReturnTrue()
        {
            var _field = _fields[4];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.CustomMethods().AlternativeViewControl(true);

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That((response?[4]?["AccantonamentiNonPrevisti"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void CustomMethods_SetIRRPrecedente_InputIsValid_ReturnTrue()
        {
            var _field = _fields[4];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.CustomMethods().SetIRRPrecedente(true);

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That((response?[4]?["UsaOOIRP"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void CustomMethods_SetIRRPrecedenteByName_InputIsValid_ReturnTrue()
        {
            var _field = _fields[4];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.CustomMethods().SetIRRPrecedenteByName("ZVEwsAccantonamenti", true);

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That((response?[4]?["UsaOOIRP"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void CustomMethods_SetSalvaStrategiaNPL_InputIsValid_ReturnTrue()
        {
            var _field = _fields[5];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.CustomMethods().SetSalvaStrategiaNPL(true);

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That((response?[5]?["SalvaStrategiaNPL"].ToBoolFromObject()), Is.True);
        }

        [Test]
        public void GetFields_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.GetFields();

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That(response?[0]["type"].ToStringFromObject(), Is.EqualTo("combobox"));
        }

        [Test]
        public void GetField_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.GetField();

            var response = result ?? [];

            //Assert
            Assert.That(response?["type"].ToStringFromObject(), Is.EqualTo("combobox"));
        }

        [Test]
        public void Code_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.Code();

            var response = result ?? String.Empty;

            //Assert
            Assert.That(response.ToString(), Is.EqualTo(_code));
        }

        [Test]
        public void GetWarnings_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.GetWarnings();

            var response = result ?? [];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That(response?[0]["field1"].ToStringFromObject(), Is.EqualTo("valorefield1"));
        }

        [Test]
        public void BaseMethods_GetValueByWarning_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.BaseMethods().GetValueByWarning("field1");

            var response = result ?? String.Empty;

            //Assert
            Assert.That(_variables[0]["field1"]?.ToString(), Is.EqualTo(response?.ToString()));
        }

        [Test]
        public void CurrentState_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.CurrentState();

            var response = result ?? String.Empty;

            //Assert
            Assert.That(response.ToString(), Is.EqualTo("BOZZA"));
        }

        [Test]
        public void GetDateTime_InputIsValid_ReturnTrue()
        {
            var _field = _fields[0];

            _executeCodeClass = new ExecuteCodeClass(new BpmDomain.Compiler.Models.MethodsParameters() { Fields = _fields, Field = _field, Code = _code, Warnings = _variables, CurrentState = "BOZZA" });

            //execute test
            var result = _executeCodeClass.GetDateTime(DateTime.Now.ToString());

            var response = result;

            //Assert
            Assert.That(response.Day, Is.EqualTo(DateTime.Now.Day));
        }

        private void PopulateData()
        {
            #region populate variables
            _code = "if(x == 1) y++;";
            #endregion

            #region populate Json
            var fieldsJson = @"[{ ""type"": ""combobox"", ""name"": ""combobox1"", ""label"": ""label combobox1"", ""value"":""V""},{ ""type"": ""text"", ""name"": ""field1"", ""label"": ""label field1"", ""value"": ""valorefield1""},{ ""type"": ""text"", ""name"": ""field2"", ""label"": ""label field2"", ""code"": ""if (BaseMethods().GetPropertyValue().Equals(\""V\"")) { if (BaseMethods().GetPropertyByName(\""field1\"", \""value\"").Contains(\""valorefield1\"")) { BaseMethods().SetPropertyByName(\""field1\"", \""visibile\"", true); } else { BaseMethods().SetPropertyByName(\""field1\"", \""visibile\"", false); } } else { BaseMethods().SetPropertyByName(\""field1\"", \""value\"", \""\""); BaseMethods().SetPropertyByName(\""field1\"", \""visibile\"", false); }""},{ ""type"": ""text"", ""name"": ""field3"", ""label"": ""label field3""},{ ""type"": ""text"", ""name"": ""ZVEwsAccantonamenti"", ""label"": ""label ZVEwsAccantonamenti"", ""code"":""CustomMethods().AlternativeViewControl(true); CustomMethods().SetIRRPrecedente(true);""},{ ""type"": ""text"", ""name"": ""ZVEwsStrategie"", ""label"": ""label ZVEwsStrategie""}]";
            var variablesJson = @"[{ ""field1"": ""valorefield1"" },{ ""warningName2"": ""warning Value 1"" }]";
            #endregion

            #region populate Deserialize
            var fields = JsonSerializer.Deserialize<List<JsonObject>>(fieldsJson);

            if (fields != null)
                _fields = fields;

            var variables = JsonSerializer.Deserialize<List<JsonObject>>(variablesJson);

            if (variables != null)
                _variables = variables;
            #endregion
        }
    }
}