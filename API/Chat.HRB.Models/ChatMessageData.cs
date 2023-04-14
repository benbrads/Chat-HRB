namespace Chat.HRB.Models
{
    public class ChatMessageData
    {

        #region Constructor

        public ChatMessageData()
        {
            Messages = new List<string>();
        }
        #endregion

        #region Properties

        public string UserId { get; set; }
        public string AppId { get; set; }
        public int Year { get; set; }
        public List<string> Messages { get; set; }

        #endregion
    }

}
