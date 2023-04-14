using System;
using System.Linq;
using Chat.HRB.Common.Interfaces;
using Chat.HRB.Interface;
using Chat.HRB.Models;
using Whetstone.ChatGPT;
using Whetstone.ChatGPT.Models;

namespace Chat.HRB.Repository
{
    public class ChatHRBRepository : BaseRepository, IChatHRBRepository
    {
        protected IServiceProvider _serviceProvider;
        protected IChatGPTClient _chatHRB;

        public ChatHRBRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _chatHRB = _serviceProvider.GetService(typeof(IChatGPTClient)) as ChatGPTClient;
        }

        private IDocumentDbRepository ChatHistoryDocumentDbRepository => this.DocumentDbRepositoryFactory.GetDocumentDbRepository(DocumentDbType.ChatHistory);
        private IDocumentDbRepository PromtDocumentDbRepository => this.DocumentDbRepositoryFactory.GetDocumentDbRepository(DocumentDbType.Prompt);

        public async Task<string> Chat(string input, string appId, string userId, int year)
        {
            //generate message to persist, in list form for cosmosDB
            List<string> messagesToPersist = new List<string>();
            messagesToPersist.Add(input);

            //first, persist the question history from the client
            await UpdateOrInsertChatHistory(userId, appId, year, messagesToPersist);

            //TODO take app id, search cosmos DB for baseline prompt/intent by app id
            //if prompt model is null, use the default hardcoded value
            var promptModel = await GetPromptAsync(appId);
            if (promptModel != null)
            {
                var prompt = promptModel.Prompts.FirstOrDefault();
                if (prompt != null)
                {
                    this._baseLineIntent = prompt.Message;
                }
            }

            string userInput = $"You: {input}\nMax Refund: ";
            string userPrompt = string.Concat(this._baseLineIntent, userInput);
            var chatHRBRequest = new ChatGPTCompletionRequest
            {
                Model = ChatGPTCompletionModels.Davinci,
                Prompt = userPrompt,
                MaxTokens = 2000
            };

            var result = await _chatHRB.CreateCompletionAsync(chatHRBRequest);
            var response = result.GetCompletionText();

            if (response != null)
            {
                return response;
            }
            return "";
        }

        public async Task<PromptModel> GetPromptAsync(string appId)
        {
            try
            {
                var result = await this.PromtDocumentDbRepository.GetItemAsync<PromptModel>(appId, appId);
                return result;
            }
            catch(Exception e)
            {

            }
            //return null if cant get data from cosmos db
            return null;
        }

        public async Task<List<string>> GetChatHistoryAsync(string userId, string appId, int year)
        {
            try
            {
                var chatData = await this.ChatHistoryDocumentDbRepository.GetItemAsync<ChatHistoryModel>(userId, userId);
                if (chatData != null)
                {
                    var filteredChatHistory = chatData.Messages?.Where(m => m.AppId == appId && m.Year == year).ToList();
                    if (filteredChatHistory != null && filteredChatHistory.Any())
                    {
                        return filteredChatHistory[0].Messages;
                    }
                }
            }
            catch (Exception e)
            {

            }
            
            return new List<string>();
        }
        public async Task<ChatHistoryModel> UpdateOrInsertChatHistory(string userId, string appId, int year, List<string> messages)
        {
            try
            {
                var chatDataModel = await this.ChatHistoryDocumentDbRepository.GetItemAsync<ChatHistoryModel>(userId, userId);
                if (chatDataModel == null)
                {
                    chatDataModel = new ChatHistoryModel() { UserId = userId };
                    chatDataModel.Messages = new List<ChatMessageData>();
                    var chatMessage = new ChatMessageData() { AppId = appId, UserId = userId, Year = year };
                    chatMessage.Messages.AddRange(messages);
                    chatDataModel.Messages.Add(chatMessage);

                    chatDataModel = await ChatHistoryDocumentDbRepository.CreateItemAsync<ChatHistoryModel>(chatDataModel).ConfigureAwait(false);
                }
                else
                {
                    var chatMessage = chatDataModel.Messages?.FirstOrDefault<ChatMessageData>(m => m.AppId == appId && m.Year == year);
                    if (chatMessage == null)
                    {
                        chatDataModel.Messages = new List<ChatMessageData>();
                        var newChatMessage = new ChatMessageData() { AppId = appId, UserId = userId, Year = year };
                        newChatMessage.Messages.AddRange(messages);
                        chatDataModel.Messages.Add(newChatMessage);
                    }
                    else
                    {
                        chatMessage.Messages.AddRange(messages);
                    }
                    chatDataModel = await ChatHistoryDocumentDbRepository.UpdateItemAsync<ChatHistoryModel>(userId, chatDataModel).ConfigureAwait(false);
                }
                return chatDataModel;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public async Task<PromptModel> UpdateOrInsertPrompt(string appId, PromptModel model)
        {
            try
            {
                var prompt = await PromtDocumentDbRepository.GetItemAsync<PromptModel>(appId, appId).ConfigureAwait(false) == null
                           ? await PromtDocumentDbRepository.CreateItemAsync<PromptModel>(model).ConfigureAwait(false)
                           : await PromtDocumentDbRepository.UpdateItemAsync<PromptModel>(appId, model).ConfigureAwait(false);

                return prompt;
            }
            catch (Exception e)
            {

            }
            return null;
        }
    }
}

