using System;

namespace SBTopic.Model
{
    public class SBSubscriptionConnectionData
    {
        public string Endpoint { get; set; }

        public string Topic { get; set; }

        public string Subscription { get; set; }

        public string SharedAccessSignatureToken { get; set; }
    }
}
