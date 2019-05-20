using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SBTopic.API.Services;
using SBTopic.Model;

namespace SBTopic.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SBSubscriptionController : ControllerBase
    {
        // GET api/values
        //[HttpGet]
        //public ActionResult<SBSubscriptionConnectionData> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        [HttpGet("{Destinatary}")]
        public async Task<ActionResult<SBSubscriptionConnectionData>> Get(string Destinatary)
        {
            SBSubscription sbSubscription = new SBSubscription();

            await sbSubscription.CreateServiceBusConnectionStringBuilders();

            SBSubscriptionConnectionData sbSubscriptionConnectionData = new SBSubscriptionConnectionData()
            {
                Endpoint = sbSubscription._ListenServiceBusConnectionStringBuilder.Endpoint,
                Topic = sbSubscription._Topic
            };

            sbSubscriptionConnectionData.Subscription = await sbSubscription.CreateSubscription(Destinatary);

            sbSubscriptionConnectionData.SharedAccessSignatureToken = await sbSubscription.CreateListenSasTokenAsync();

            return sbSubscriptionConnectionData;
        }

        [HttpGet("{Destinatary}/{Subscription}")]
        public async Task<ActionResult<SBSubscriptionConnectionData>> Get(string Destinatary, string Subscription)
        {
            SBSubscription sbSubscription = new SBSubscription();

            await sbSubscription.CreateServiceBusConnectionStringBuilders();

            SBSubscriptionConnectionData sbSubscriptionConnectionData = new SBSubscriptionConnectionData()
            {
                Endpoint = sbSubscription._ListenServiceBusConnectionStringBuilder.Endpoint,
                Topic = sbSubscription._Topic
            };

            sbSubscriptionConnectionData.Subscription = Subscription;

            sbSubscriptionConnectionData.SharedAccessSignatureToken = await sbSubscription.CreateListenSasTokenAsync();

            return sbSubscriptionConnectionData;
        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
