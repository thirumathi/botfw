using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace VoiceCommandService
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        private VoiceCommandServiceConnection voiceServiceConnection;
        private BackgroundTaskDeferral serviceDeferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Create the deferral by requesting it from the task instance
            serviceDeferral = taskInstance.GetDeferral();

            AppServiceTriggerDetails triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name.Equals("VoiceCommandService"))
            {
                voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                string output;

                // Perform the appropriate command depending on the operation defined in VCD
                switch (voiceCommand.CommandName)
                {
                    case "sayHiToBot":
                        output = await BotConnectorHelper.SendAndReceive("hi");
                        break;
                    default:
                        output = await BotConnectorHelper.SendAndReceive(voiceCommand.SpeechRecognitionResult.Text);
                        break;
                }

                var userMessage = new VoiceCommandUserMessage();
                var destinationsContentTiles = new List<VoiceCommandContentTile>();

                userMessage.DisplayMessage = "Bot Response";
                userMessage.SpokenMessage = "Bot Response";

                var destinationTile = new VoiceCommandContentTile();

                destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;

                destinationTile.Title = "Reply";
                destinationTile.TextLine1 = output;

                destinationsContentTiles.Add(destinationTile);

                var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

                await voiceServiceConnection.ReportSuccessAsync(response);
            }

            // Once the asynchronous method(s) are done, close the deferral
            serviceDeferral.Complete();
        }
    }
}
