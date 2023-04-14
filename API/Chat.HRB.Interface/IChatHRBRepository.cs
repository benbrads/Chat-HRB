using System;
using Chat.HRB.Models;

namespace Chat.HRB.Interface
{
    public interface IChatHRBRepository
    {
        Task<string> Chat(string input, string appId, string userId, int year);
        Task<PromptModel> GetPromptAsync(string appId);
        Task<PromptModel> UpdateOrInsertPrompt(string appId, PromptModel model);
        Task<List<string>> GetChatHistoryAsync(string userId, string appId, int year);
        Task<ChatHistoryModel> UpdateOrInsertChatHistory(string userId, string appId, int year, List<string> messages);
    }
}

