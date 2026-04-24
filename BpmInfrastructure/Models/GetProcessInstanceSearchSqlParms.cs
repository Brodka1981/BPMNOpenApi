namespace BpmInfrastructure.Models
{
    public class GetProcessInstanceSearchSqlParms
    {
        public List<string> Categories { get; set; } = [];
        public List<string> IdStates { get; set; } = [];
        public List<string> DefinitionTypes { get; set; } = [];
        public List<string> Columns { get; set; } = [];

        public List<VariableFilterItemDto>? VariableFilters;
    }
    public class VariableFilterItemDto
    {
        private object? _value;
        private string _condition = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Condition
        {
            get => _condition;
            set
            {
                _condition = value;
                ApplyLikeIfNeeded();
            }
        }

        public object? Value
        {
            get => _value;
            set
            {
                _value = value;
                ApplyLikeIfNeeded();
            }
        }

        private void ApplyLikeIfNeeded()
        {
            if (_condition == "LIKE" && _value is string str && !str.StartsWith("%"))
            {
                _value = $"%{str}%";
            }
        }
    }

}
