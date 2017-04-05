using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Autofac;

namespace HelloWorldBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        string botName = "ThirusHelloWorldBot";
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                if (activity.Text.Equals("who", StringComparison.OrdinalIgnoreCase))
                {
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                    {
                        var client = scope.Resolve<IConnectorClient>();
                        var activityMembers = await client.Conversations.GetConversationMembersAsync(activity.Conversation.Id);

                        string members = string.Join(
                            "\n ",
                            activityMembers.Select(
                                member => (member.Id != activity.Recipient.Id) ? $"* {member.Name} (Id: {member.Id})"
                                            : $"* {activity.Recipient.Name} (Id: {activity.Recipient.Id})"));

                        Activity reply = activity.CreateReply($"These are the members of this conversation: \n {members}");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                else
                {
                    // calculate something for us to return
                    int length = (activity.Text ?? string.Empty).Length;

                    // return our reply to the user
                    Activity reply = activity.CreateReply($"You sent **{activity.Text.ToUpper()}** which was {length} characters");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                if (message.MembersAdded != null && message.MembersAdded.Any())
                {
                    string membersAdded = string.Join(
                        ", ",
                        message.MembersAdded.Select(
                            newMember => (newMember.Id != message.Recipient.Id && !newMember.Name.Equals(botName, StringComparison.OrdinalIgnoreCase) && !newMember.Name.Equals("bot", StringComparison.OrdinalIgnoreCase)) ? $"{newMember.Name} (Id: {newMember.Id})" : string.Empty));

                    if (!string.IsNullOrEmpty(membersAdded))
                    {
                        Activity reply = message.CreateReply($"Welcome {membersAdded}");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }

                if (message.MembersRemoved != null && message.MembersRemoved.Any())
                {
                    string membersRemoved = string.Join(
                        ", ",
                        message.MembersRemoved.Select(
                            removedMember => (removedMember.Id != message.Recipient.Id) ? $"{removedMember.Name} (Id: {removedMember.Id})" : string.Empty));

                    Activity reply = message.CreateReply($"The following members {membersRemoved} were removed or left the conversation :(");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}