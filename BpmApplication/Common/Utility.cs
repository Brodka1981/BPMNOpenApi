using System.Text.Json.Nodes;

namespace BpmApplication.Common
{
    public static class Utility
    {
        /// <summary>
        /// To Get Context Result
        /// </summary>
        /// <param name="getContextResult"></param>
        /// <returns></returns>
        public static BpmApplication.Results.GetContextResult ToGetContextResult(this BpmDomain.Results.GetContextResult getContextResult)
        {
            List<Models.Action>? actions = null;
            List<Models.Section>? sections = null;

            if (getContextResult?.Actions != null)
            {
                actions = [];
                foreach (var action in getContextResult.Actions)
                    actions.Add(new Models.Action() { IdAction = action.IdAction, Description = action.Description });
            }

            if (getContextResult?.Form?.Sections != null)
            {
                sections = new List<Models.Section>();
                foreach (var section in getContextResult.Form.Sections)
                    sections.Add(new Models.Section() { Title = section.Title, Type = section.Type, Fields = section.Fields });
            }

            if (getContextResult == null)
                return new BpmApplication.Results.GetContextResult();

            return new BpmApplication.Results.GetContextResult()
            {
                ProcessId = getContextResult.ProcessId,
                Name = getContextResult.Name,
                ProcessType = getContextResult.ProcessType,
                ContextMode = getContextResult.ContextMode,
                Actions = actions,
                State = new Models.State() { IdState = getContextResult?.State?.IdState, Description = getContextResult?.State?.Description },
                Variables = getContextResult?.Variables,
                Form = new Models.Form() { Sections = sections }
            };
        }
    }
}