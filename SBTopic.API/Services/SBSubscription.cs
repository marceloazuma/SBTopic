using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Azure.ServiceBus.Primitives;

namespace SBTopic.API.Services
{
    public class SBSubscription
    {
        public string _ManagementConnectionString = "Endpoint=sb://xp-sbteste.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Dq698VqvxAJKSWPsOOuEuYVKXQjjnGtpvM7j1EaCIZU=";

        public string _ListenConnectionString;

        public string _Topic = "teste";

        public string _ListenSharedAccessAuthorizationRule = "ListenSharedAccessAuthorizationRule";

        public ServiceBusConnectionStringBuilder _ListenServiceBusConnectionStringBuilder = null;

        public ServiceBusConnectionStringBuilder _ManagementServiceBusConnectionStringBuilder = null;

        public SBSubscription()
        {
        }

        public SBSubscription(string ManagementConnectionString, string ListenConnectionString, string Topic)
        {
            _ManagementConnectionString = ManagementConnectionString;
            _ListenConnectionString = ListenConnectionString;
            _Topic = Topic;
        }

        public async Task CreateServiceBusConnectionStringBuilders()
        {
            _ManagementServiceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_ManagementConnectionString);

            TopicDescription topicDescription = await GetTopic();
            SharedAccessAuthorizationRule authorizationRule = null;

            if (topicDescription == null)
            {
                topicDescription = await CreateTopic();
            }

            if (topicDescription != null)
            {
                authorizationRule = topicDescription.AuthorizationRules.Find(r => r.KeyName == _ListenSharedAccessAuthorizationRule) as SharedAccessAuthorizationRule;

                if (authorizationRule == null)
                {
                    topicDescription = await CreateSharedAccessAuthorizationRule(topicDescription);
                }
            }

            authorizationRule = topicDescription.AuthorizationRules.Find(r => r.KeyName == _ListenSharedAccessAuthorizationRule) as SharedAccessAuthorizationRule;

            _ListenServiceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(_ManagementServiceBusConnectionStringBuilder.Endpoint, _Topic, _ListenSharedAccessAuthorizationRule, authorizationRule.PrimaryKey);
        }

        private async Task<TopicDescription> GetTopic()
        {
            ManagementClient managementClient = new ManagementClient(_ManagementConnectionString);

            try
            {
                return await managementClient.GetTopicAsync(_Topic);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<TopicDescription> CreateTopic()
        {
            ManagementClient managementClient = new ManagementClient(_ManagementConnectionString);

            try
            {
                TopicDescription topicDescription = new TopicDescription(_Topic)
                {
                    AutoDeleteOnIdle = TimeSpan.FromMinutes(30),
                    DefaultMessageTimeToLive = TimeSpan.FromMinutes(5),
                    MaxSizeInMB = 1024
                };

                return await managementClient.CreateTopicAsync(topicDescription);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<TopicDescription> CreateSharedAccessAuthorizationRule(TopicDescription topicDescription)
        {
            ManagementClient managementClient = new ManagementClient(_ManagementConnectionString);

            try
            {
                List<AccessRights> accessRights = new List<AccessRights>();
                accessRights.Add(AccessRights.Listen);

                AuthorizationRule authorizationRule = new SharedAccessAuthorizationRule(_ListenSharedAccessAuthorizationRule, accessRights);

                topicDescription.AuthorizationRules.Add(authorizationRule);
                return await managementClient.UpdateTopicAsync(topicDescription);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> CreateSubscription(string Destinatary)
        {
            string subscription = Guid.NewGuid().ToString();
            SubscriptionDescription subscriptionDescription = new SubscriptionDescription(_Topic, subscription)
            {
                AutoDeleteOnIdle = TimeSpan.FromHours(1),
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(5),
                LockDuration = TimeSpan.FromSeconds(30),
                MaxDeliveryCount = 50
            };

            Filter filter = new CorrelationFilter()
            {
                To = Destinatary
            };

            RuleDescription ruleDescription = new RuleDescription("ToFilter", filter);

            ManagementClient managementClient = new ManagementClient(_ManagementConnectionString);

            await managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);

            return subscription;
        }

        public async Task<string> CreateListenSasTokenAsync()
        {
            /// Criação do SAS Token para envio ao cliente, com validade de 1 hora em securityToken.TokenValue
            /// Este código deverá rodar no servidor
            TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_ListenServiceBusConnectionStringBuilder.SasKeyName, _ListenServiceBusConnectionStringBuilder.SasKey, TimeSpan.FromMinutes(1));
            SecurityToken securityToken = await tokenProvider.GetTokenAsync(_ManagementServiceBusConnectionStringBuilder.Endpoint, TimeSpan.FromMinutes(1));

            return securityToken.TokenValue;
        }
    }
}
