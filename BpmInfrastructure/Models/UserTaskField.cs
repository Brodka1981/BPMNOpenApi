namespace BpmInfrastructure.Models
{
    public sealed class UserTaskField
    {
        public string Json { get; }

        public UserTaskField(string json)
        {
            Json = json;
        }
    }
}