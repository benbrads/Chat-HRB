#region Using Directives

using Newtonsoft.Json;

#endregion

namespace Chat.HRB.Models
{
    public class ChatHistoryModel
    {
        #region Constructor

        public ChatHistoryModel()
        {
            Messages = new List<ChatMessageData>();
        }

        #endregion

        #region Properties

        [JsonProperty("id")]
        public string UserId { get; set; }
        public List<ChatMessageData> Messages { get; set; }

        #endregion
    }
}
