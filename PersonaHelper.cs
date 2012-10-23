using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Data;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace ChangePersona
{
    /// <summary>
    /// Used to persist the OMS context data between the menus etc
    /// Also includes various helper methods
    /// </summary>
    public class PersonaHelper
    {
        private readonly AnalyticsTracker personaTracker;

        /// <summary>
        /// Gets a value indicating whether to disable other rules or not.
        /// </summary>
        /// <value><c>true</c> if disable other rules otherwise, <c>false</c>.</value>
        public static bool DisableOtherRules
        {
            get
            {
                if (!ID.IsNullOrEmpty(ActivePersona))
                {
                    var personaItem = Sitecore.Data.Database.GetDatabase("master").GetItem(ActivePersona);

                    if (personaItem != null)
                    {
                        return MainUtil.GetBool(personaItem["Disable Other Rules"], false);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Defines is rule is active.
        /// </summary>
        /// <param name="conditionItem">The condition item.</param>
        /// <returns>Is rule active</returns>
        public static bool RuleIsActive(Item conditionItem)
        {
            if (!ID.IsNullOrEmpty(ActivePersona))
            {
                var personaItem = Sitecore.Data.Database.GetDatabase("master").GetItem(ActivePersona);

                if (personaItem != null)
                {
                    var forcedRulesField = (MultilistField) personaItem.Fields["Active Rules"];
                    return forcedRulesField.GetItems().Where(x => x.ID == conditionItem.ID).Count() > 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonaHelper"/> class.
        /// </summary>
        /// <param name="newContext">The analytics context.</param>
        private PersonaHelper(AnalyticsTracker newContext)
        {
            personaTracker = newContext;
        }

        /// <summary>
        /// Gets the context helper.
        /// </summary>
        /// <returns></returns>
        public static PersonaHelper GetHelper()
        {
            return HttpContext.Current.Application["PersonaHelper"] as PersonaHelper;
        }

        /// <summary>
        /// Initializes the helper using the specified new analytics tracker.
        /// </summary>
        /// <param name="newContext">The analytics tracker.</param>
        public static void Initialize(AnalyticsTracker newContext)
        {
            HttpContext.Current.Application["PersonaHelper"] = new PersonaHelper(newContext);
        }

        /// <summary>
        /// The currently active persona.
        /// </summary>
        public static ID ActivePersona;

        /// <summary>
        /// Resets the profiles.
        /// </summary>
        public void ResetProfiles()
        {
            ActivePersona = ID.Null;

            foreach (var profileKey in personaTracker.Data.Profiles.SelectMany(profile => profile))
            {
                profileKey.Value = 0;
            }
            
            personaTracker.Submit();
            AnalyticsTracker.IsActive = false;
        }

        /// <summary>
        /// Activates the context stored in the helper.
        /// </summary>
        public void SetAnalyticsContext()
        {
            Context.Items["SC_ANALYTICS_TRACKER"] = personaTracker;
        }

        /// <summary>
        /// Updates the profile key.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="value">The value.</param>
        public void UpdateProfileKey(string profileName, string keyName, int value)
        {
            var profile = GetProfile(profileName);
            if(profile == null)
            {
                profile = personaTracker.Data.Profiles.AddProfile(profileName);
            }

            var profileKeyData = profile.GetProfileKey(keyName);
            if(profileKeyData == null)
            {
                profile.AddProfileKeyValue(keyName, value); 
            }
            else
            {
                profileKeyData.Value = value;
            }

            AnalyticsManager.Submit(personaTracker, false);
        }


        /// <summary>
        /// Gets a profile by name.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <returns></returns>
        public ProfileData GetProfile(string profileName)
        {
            return personaTracker.Data.Profiles.GetProfile(profileName);
        }

        /// <summary>
        /// Gets the profile key value.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <returns></returns>
        public int GetProfileKeyValue(string profileName, string keyName)
        {
            var profile = GetProfile(profileName);
            if(profile != null)
            {
                var profileKeyData = profile.GetProfileKey(keyName);
                if(profileKeyData != null)
                {
                    return profileKeyData.Value;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets all profiles.
        /// </summary>
        /// <returns></returns>
        public ProfilesDataCollection GetProfiles()
        {
            return personaTracker.Data.Profiles;
        }
    }
}
