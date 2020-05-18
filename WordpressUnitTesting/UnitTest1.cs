using NUnit.Framework;
using RabbitMqReceiver;
using RabbitMqReceiver.Services;
using WordPressPCL.Models;

namespace WordpressUnitTesting
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public async System.Threading.Tasks.Task Test1Async()
        {
           
                Services services = new Services();
                string xml = @"<?xml version = '1.0' encoding = 'utf-8'?><patch_user><application_name>str1234</application_name><name>str1234</name><uuid>0e6a633d-516f-48f8-a365-a9e15b6c658c</uuid><email>str1234@mail.be</email ><street>str1234</street><municipal>str1234</municipal><postalCode>str1234</postalCode><vat>str1234</vat></patch_user>";
                var response = await services.ReceivingPatchUserAsync(xml);
            Assert.AreEqual(typeof(User), response);
            var test = 0;

            
        }

        [Test]
        public async System.Threading.Tasks.Task Test2Async()
        {

            Services services = new Services();
            var response = await services.TestUsersConnectionAsync();
            Assert.IsNotNull(response);
            var test = 0;


        }
    }
}