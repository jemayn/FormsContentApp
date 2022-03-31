using Umbraco.Core.Composing;
using Umbraco.Web;

namespace FormsContentApp.ContentApp
{
    public class ContentAppComponent : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.ContentApps().Append<FormsRecordsContentApp>();
        }
    }
}
