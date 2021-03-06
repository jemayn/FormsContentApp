using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace FormsContentApp.Test
{
    public abstract class UmbracoBaseTest
    {
        public ServiceContext ServiceContext;
        public MembershipHelper MembershipHelper;
        public UmbracoHelper UmbracoHelper;
        public UmbracoMapper UmbracoMapper;

        public Mock<ICultureDictionary> CultureDictionary;
        public Mock<ICultureDictionaryFactory> CultureDictionaryFactory;
        public Mock<IPublishedContentQuery> PublishedContentQuery;

        public Mock<HttpContextBase> HttpContext;
        public Mock<IMemberService> memberService;
        public Mock<IPublishedMemberCache> memberCache;

        [SetUp]
        public virtual void SetUp()
        {
            this.SetupHttpContext();
            this.SetupCultureDictionaries();
            this.SetupPublishedContentQuerying();

            this.ServiceContext = ServiceContext.CreatePartial();
            this.UmbracoHelper = new UmbracoHelper(Mock.Of<IPublishedContent>(), Mock.Of<ITagQuery>(), this.CultureDictionaryFactory.Object, Mock.Of<IUmbracoComponentRenderer>(), this.PublishedContentQuery.Object, this.MembershipHelper);
            this.UmbracoMapper = new UmbracoMapper(new MapDefinitionCollection(new List<IMapDefinition>()));
        }

        public virtual void SetupHttpContext()
        {
            this.HttpContext = new Mock<HttpContextBase>();
        }

        public virtual void SetupCultureDictionaries()
        {
            this.CultureDictionary = new Mock<ICultureDictionary>();
            this.CultureDictionaryFactory = new Mock<ICultureDictionaryFactory>();
            this.CultureDictionaryFactory.Setup(x => x.CreateDictionary()).Returns(this.CultureDictionary.Object);
        }

        public virtual void SetupPublishedContentQuerying()
        {
            this.PublishedContentQuery = new Mock<IPublishedContentQuery>();
        }

        public void SetupPropertyValue(Mock<IPublishedContent> publishedContentMock, string alias, object value, string culture = null, string segment = null)
        {
            var property = new Mock<IPublishedProperty>();
            property.Setup(x => x.Alias).Returns(alias);
            property.Setup(x => x.GetValue(culture, segment)).Returns(value);
            property.Setup(x => x.HasValue(culture, segment)).Returns(value != null);
            publishedContentMock.Setup(x => x.GetProperty(alias)).Returns(property.Object);
        }
    }
}
