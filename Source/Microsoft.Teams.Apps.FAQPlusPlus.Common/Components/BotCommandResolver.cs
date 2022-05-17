// <copyright file="BotCommandResolver.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that resolves bot commands in personal and teams chat.
    /// </summary>
    public class BotCommandResolver : IBotCommandResolver
    {
        /// <summary>
        /// ChangeStatus - text that triggers change status action by SME.
        /// </summary>
        private const string ChangeStatus = "change status";

        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly IActivityStorageProvider activityStorageProvider;
        private readonly ILogger<BotCommandResolver> logger;
        private readonly IQnAPairServiceFacade qnaPairService;
        private readonly string appBaseUri;
        private readonly IConversationService conversationService;
        private readonly BotGraphQlClient botGraphQlClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotCommandResolver"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="activityStorageProvider">Activity storage provider.</param>
        /// <param name="qnaServiceProvider">QnA service provider.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="qnaPairService">Instance of QnA pair service class to call add/update/get QnA pair.</param>
        /// <param name="botSettings">Represents a set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="conversationService">Conversation service to send adaptive card in personal and teams chat.</param>
        /// <param name="botGraphQlClient">GraphQl client...</param>
        public BotCommandResolver(
            Common.Providers.IConfigurationDataProvider configurationProvider,
            IQnaServiceProvider qnaServiceProvider,
            IActivityStorageProvider activityStorageProvider,
            IQnAPairServiceFacade qnaPairService,
            IOptionsMonitor<BotSettings> botSettings,
            ILogger<BotCommandResolver> logger,
            IConversationService conversationService,
            BotGraphQlClient botGraphQlClient)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.qnaServiceProvider = qnaServiceProvider ?? throw new ArgumentNullException(nameof(qnaServiceProvider));
            this.activityStorageProvider = activityStorageProvider ?? throw new ArgumentNullException(nameof(activityStorageProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.qnaPairService = qnaPairService ?? throw new ArgumentNullException(nameof(qnaPairService));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            this.botGraphQlClient = botGraphQlClient;
            if (botSettings == null)
            {
                throw new ArgumentNullException(nameof(botSettings));
            }

            var options = botSettings.CurrentValue;
            this.appBaseUri = options.AppBaseUri;
        }

        /// <summary>
        /// Resolve bot command in 1:1 chat.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ResolveBotCommandInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            string text = (message.Text ?? string.Empty).Trim();
            string commandType;
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
            {

                commandType = "Card submit in 1:1 chat";
                this.logger.LogInformation(commandType);
                await this.botGraphQlClient.SaveMessage(message.From?.Id ?? "", message.From?.Name ?? "", commandType, text, message.ReplyToId ?? "", String.Empty);
                await this.conversationService.SendAdaptiveCardInPersonalChatAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
                return;
            }

            if (text.Equals(Strings.BotCommandAskExpert, StringComparison.CurrentCultureIgnoreCase) ||
                text.Equals(Constants.AskAnExpert, StringComparison.InvariantCultureIgnoreCase))
            {
                commandType = "Sending user ask an expert card";
                this.logger.LogInformation(commandType);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard())).ConfigureAwait(false);
            }
            else if (text.Equals(Strings.BotCommandFeedback, StringComparison.CurrentCultureIgnoreCase) ||
                text.Equals(Constants.ShareFeedback, StringComparison.InvariantCultureIgnoreCase))
            {
                commandType = "Sending user feedback card";
                this.logger.LogInformation(commandType);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard())).ConfigureAwait(false);
            }
            else if (text.Equals(Strings.BotCommandTour, StringComparison.CurrentCultureIgnoreCase) ||
                text.Equals(Constants.TakeATour, StringComparison.InvariantCultureIgnoreCase))
            {
                commandType = "Sending user tour card";
                this.logger.LogInformation(commandType);
                var userTourCards = TourCarousel.GetUserTourCards(this.appBaseUri);
                await turnContext.SendActivityAsync(MessageFactory.Carousel(userTourCards)).ConfigureAwait(false);
            }
            else
            {
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                commandType = "Sending input to QnAMaker";
                this.logger.LogInformation(commandType);
                await this.qnaPairService.GetReplyToQnAAsync(turnContext, message).ConfigureAwait(false);
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                this.logger.LogInformation($"Time taken to answer a question: {elapsedTime}");
            }

            await this.botGraphQlClient.SaveMessage(message.From?.Id ?? "", message.From?.Name ?? "", commandType, text, message.ReplyToId ?? "", String.Empty);
        }

        /// <summary>
        /// Resolves bot commands in channel.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task ResolveBotCommandInTeamChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            string text;
            string type;

            // Check if the incoming request is from SME for updating the ticket status.
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues && !string.IsNullOrEmpty(((JObject)message.Value)["ticketId"]?.ToString()))
            {
                text = ChangeStatus ?? throw new ArgumentNullException(nameof(ChangeStatus));
            }
            else
            {
                text = message.Text?.ToLower()?.Trim() ?? string.Empty;
            }

            try
            {
                switch (text)
                {
                    case Constants.TeamTour:
                        type = "Sending team tour card";
                        this.logger.LogInformation("Sending team tour card");
                        var teamTourCards = TourCarousel.GetTeamTourCards(this.appBaseUri);
                        await turnContext.SendActivityAsync(MessageFactory.Carousel(teamTourCards)).ConfigureAwait(false);
                        break;

                    case ChangeStatus:
                        type = $"Card submit in channel {message.Value?.ToString()}";
                        this.logger.LogInformation($"Card submit in channel {message.Value?.ToString()}");
                        await this.conversationService.SendAdaptiveCardInTeamChatAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
                        return;

                    case Constants.DeleteCommand:
                        type = $"Delete card submit in channel {message.Value?.ToString()}";
                        this.logger.LogInformation($"Delete card submit in channel {message.Value?.ToString()}");
                        await QnaHelper.DeleteQnaPair(turnContext, this.qnaServiceProvider, this.activityStorageProvider, this.logger, cancellationToken).ConfigureAwait(false);
                        break;

                    case Constants.NoCommand:
                        return;

                    default:
                        type = "Unrecognized input in channel";
                        this.logger.LogInformation("Unrecognized input in channel");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedTeamInputCard.GetCard())).ConfigureAwait(false);
                        break;
                }

                await this.botGraphQlClient.SaveMessage(message.From?.Id ?? "", message.From?.Name ?? "", type, text, message.ReplyToId ?? "", String.Empty);
            }
            catch (Exception ex)
            {
                // Check if expert user is trying to delete the question and knowledge base has not published yet.
                if (((ErrorResponseException)ex).Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this.qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    if (!hasPublished)
                    {
                        var activity = (Activity) turnContext.Activity;
                        var activityValue = ((JObject)activity.Value).ToObject<AdaptiveSubmitActionData>();
                        await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.WaitMessage, activityValue?.OriginalQuestion))).ConfigureAwait(false);
                        this.logger.LogError(ex, $"Error processing message: {ex.Message}", SeverityLevel.Error);
                        return;
                    }
                }

                // Throw the error at calling place, if there is any generic exception which is not caught by above conditon.
                throw;
            }
        }
    }
}
