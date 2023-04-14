using Newtonsoft.Json;

namespace Chat.HRB.Models
{
    public class PromptModel
    {
        #region Constructor

        public PromptModel()
        {
            Prompts = new List<PromptMessage>();
        }
        #endregion

        #region Properties

        [JsonProperty("id")]
        public string AppId { get; set; }
        [JsonProperty("prompts")]
        public List<PromptMessage> Prompts { get; set; }

        #endregion
    }
}
