using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;

namespace ChangePersona
{
    public class ActivatePersona : WebEditCommand
    {
        /// <summary>
        /// Activate the selected persona.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            PersonaHelper personaHelper = PersonaHelper.GetHelper();
            personaHelper.ResetProfiles();
            Item persona = Client.ContentDatabase.Items[context.Parameters["id"]];
            PersonaHelper.ActivePersona = persona.ID;
            
            var profileValues = XDocument.Parse(persona["ProfileValues"]);
            var tracking = profileValues.Element("tracking");
            if(tracking == null)
            {
                return;
            }
            IEnumerable<XElement> profileElements = tracking.Elements("profile");
            if (profileElements != null)
            {
                foreach (XElement profileElement in profileElements)
                {
                    string profileName = profileElement.GetAttributeValue("name");
                    if (!string.IsNullOrEmpty(profileName))
                    {
                        IEnumerable<XElement> profileKeys = profileElement.Elements("key");
                        if (profileKeys != null)
                        {
                            foreach (XElement profileKey in profileKeys)
                            {
                                int profileKeyValue;
                                string profileKeyName = profileKey.GetAttributeValue("name");
                                string profileValueString = profileKey.GetAttributeValue("value");
                                if (int.TryParse(profileValueString, out profileKeyValue))
                                {
                                    personaHelper.UpdateProfileKey(profileName, profileKeyName, profileKeyValue * 10);
                                }
                            }
                        }
                    }
                }
            }

            Reload(GetUrl());
        }
    }
}
