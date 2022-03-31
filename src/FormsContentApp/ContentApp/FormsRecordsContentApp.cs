using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace FormsContentApp.ContentApp
{
    public class FormsRecordsContentApp : IContentAppFactory
    {
        private readonly IContentTypeService _contentTypeService;

        public FormsRecordsContentApp(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public Umbraco.Core.Models.ContentEditing.ContentApp GetContentAppFor(object source, IEnumerable<IReadOnlyUserGroup> userGroups)
        {   
            if (source is IContent content)
            {
                foreach (var property in content.Properties)
                {
                    switch (property.PropertyType.PropertyEditorAlias)
                    {
                        // leave the prop alias strings for backwards compatibility, constants of newer prop editors will not exist in older versions
                        case "UmbracoForms.FormPicker":
                            if (HasFormPicker(property))
                                return FormsApp();
                            break;
                        case "Umbraco.TinyMCE":
                            if (HasRte(property))
                                return FormsApp();
                            break;
                        case "Umbraco.Grid":
                            if (HasGrid(property))
                                return FormsApp();
                            break;
                        case "Umbraco.NestedContent":
                            if (HasNc(property))
                                return FormsApp();
                            break;
                        case "Umbraco.BlockList":
                            if (HasBlock(property))
                                return FormsApp();
                            break;
                        default:
                            break;
                    }
                }
            }

            return null;
        }

        private bool HasBlock(Property property)
        {
            throw new NotImplementedException();
        }

        private bool HasNc(Property property)
        {
            if (property.Values.Count == 0)
                return false;

            var value = property.Values.FirstOrDefault().PublishedValue.ToString();

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.DetectIsJson() == false)
                return false;

            var nestedContent = JsonConvert.DeserializeObject<NestedContentValue[]>(value);

            if (nestedContent == null)
                return false;

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.Get(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                return false;
            }

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    var rowValue = row.PropertyValues[key];

                    if(rowValue != null)
                    {

                    }
                }
            }

            return false;
        }

        private bool HasGrid(Property property)
        {
            if (property.Values.Count == 0)
                return false;

            var value = property.Values.FirstOrDefault().PublishedValue.ToString();

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.DetectIsJson() == false)
                return false;

            var grid = JsonConvert.DeserializeObject<GridValue>(value);

            if (grid == null)
                return false;

            foreach (var section in grid.Sections)
            {
                foreach (var row in section.Rows)
                {                    
                    foreach (var area in row.Areas)
                    {                        
                        foreach (var control in area.Controls)
                        {
                            if (control?.Editor?.Alias != null)
                            {
                                if (control.Editor.Alias == "umbraco_form_picker" || control.Editor.Alias == "macro")
                                {
                                    // TODO: Check if the picker or macro contains a form - can currently be null if you add the macro but don't select a form. Will also give a big error on each pageload though so can expect it to be a bug..
                                    if (control.Value.Value<string>("macroAlias") == "renderUmbracoForm")
                                        return true;
                                }

                                if (control.Editor.Alias == "rte")
                                {
                                    string macroString = "UMBRACO_MACRO macroAlias=\"renderUmbracoForm\"";

                                    if (control.Value.ToString().Contains(macroString))
                                        return true;
                                }
                            }                                
                        }
                    }
                }
            }

            return false;
        }

        private bool HasRte(Property property)
        {
            string macroString = "UMBRACO_MACRO macroAlias=\"renderUmbracoForm\"";

            if (property.Values.Count > 0)
            {
                if (property.Values.FirstOrDefault().PublishedValue == null)
                    return false;
                if (property.Values.FirstOrDefault().PublishedValue.ToString().Contains(macroString))
                    return true;
                return false;
            }

            return false;
        }

        private bool HasFormPicker(Property property)
        {
            if (property.Values.Count > 0)
                return true;

            return false;
        }

        private Umbraco.Core.Models.ContentEditing.ContentApp FormsApp()
        {
            var formsApp = new Umbraco.Core.Models.ContentEditing.ContentApp
            {
                Alias = "formsRecords",
                Name = "Forms Records",
                Icon = "icon-chat",
                View = "/App_Plugins/FormsContentApp/formscontentapp.html",
                Weight = 0
            };

            return formsApp;
        }
    }

    public class NestedContentValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ncContentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        /// <summary>
        /// The remaining properties will be serialized to a dictionary
        /// </summary>
        /// <remarks>
        /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
        /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
        /// NestedContent serializes to string, int, whatever eg
        ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
        /// </remarks>
        [JsonExtensionData]
        public IDictionary<string, object> PropertyValues { get; set; }
    }
}
