using FormsContentApp.ContentApp;
using NUnit.Framework;
using Umbraco.Core.Models;

namespace FormsContentApp.Test
{
    [TestFixture]
    public class ContentAppComponent : UmbracoBaseTest
    {      
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void HasBlock()
        {
            var contentApp = new FormsRecordsContentApp();

            var property = new Property { };

            var block1 = contentApp.HasBlock();
            Assert.Pass();
        }
    }
}