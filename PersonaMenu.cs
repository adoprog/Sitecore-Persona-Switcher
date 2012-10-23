using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using Sitecore;
using Sitecore.Analytics.Rules.Conditions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Rules;
using Sitecore.Rules.ConditionalRenderings;
using Sitecore.Shell.Applications.Rules;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Control = System.Web.UI.Control;

namespace ChangePersona
{
    /// <summary>
    /// WebEdit Command that runs the PersonaSwitcher menu
    /// </summary>
    [Serializable]
    public class PersonaMenu : WebEditCommand
    {
        /// <summary>
        /// Shows the profile keys for a given profile group name.
        /// </summary>
        /// <param name="profileName">Name of the profile.</param>
        private void ShowKeys(string profileName)
        {
            SheerResponse.DisableOutput();
            Item profileKeys = Client.ContentDatabase.Items["/sitecore/system/Marketing Center/Profiles/" + profileName];
            if(profileKeys !=null)
            {
                var subMenu = new Menu();
                foreach (Item profileKey in profileKeys.Children)
                {
                    PersonaHelper personaHelper = PersonaHelper.GetHelper();
                    int keyData = personaHelper.GetProfileKeyValue(profileName, profileKey.Name);
                    string displayName = string.Format("{0}: {1}", profileKey.Name, keyData);

                    subMenu.Add(profileKey.ID.ToString(), displayName, profileKey.Appearance.Icon, string.Empty, 
                        string.Format("webedit:editprofilekey(profilename={0},profilekey={1})", profileName, profileKey.Name), false, string.Empty, MenuItemType.Normal);
                }
                SheerResponse.EnableOutput();
                SheerResponse.ShowPopup(profileName, "right", subMenu);
            }
        }

        /// <summary>
        /// Shows a list of all profile groups.
        /// </summary>
        /// <param name="menuId">The menu id, used when determining where the menu should appear.</param>
        private void ShowProfiles(string menuId)
        {
            SheerResponse.DisableOutput();
            Menu subMenu = new Menu();
            Item profiles = Client.ContentDatabase.Items["/sitecore/system/Marketing Center/Profiles"];

            foreach (Item profile in profiles.Children)
            {
                string profileName = profile.Name;
                subMenu.Add(profileName, profileName, profile.Appearance.Icon, string.Empty, string.Format("webedit:personamenu(showkeys={0})", profileName), false, string.Empty, MenuItemType.Submenu);
            }
            SheerResponse.EnableOutput();
            SheerResponse.ShowPopup(menuId, "right", subMenu);
        }

        /// <summary>
        /// Shows the personas.
        /// </summary>
        /// <param name="menuId">The menu id, used when determining where the menu should appear.</param>
        private void ShowPersonas(string menuId)
        {
            SheerResponse.DisableOutput();
            Menu subMenu = new Menu();

            Item personas = Client.ContentDatabase.Items["/sitecore/system/Marketing Center/Personas/"];
            foreach (Item persona in personas.Children)
            {
                string id = "L" + ShortID.NewId();
                string displayName = persona.DisplayName;
                if (persona.ID == PersonaHelper.ActivePersona)
                {
                    displayName += " - Active";
                }

                if (displayName.Length > 0)
                {
                    subMenu.Add(id, displayName, persona.Appearance.Icon, string.Empty, string.Format("webedit:activatepersona(id={0})", persona.ID), false, string.Empty, MenuItemType.Normal);
                }
            }

            SheerResponse.EnableOutput();
            SheerResponse.ShowPopup(menuId, "right", subMenu);
        }

        private static string GetRuleDescription(Item item)
        {
            var stringOutput = new StringWriter();
            var output = new HtmlTextWriter(stringOutput);
            var renderer2 = new RulesRenderer(item.Fields["Rule"].Value);
            renderer2.Render(output);
            return stringOutput.ToString();
        }

        private void ShowRules(string menuId, Item contextItem)
        {
            SheerResponse.DisableOutput();
            Menu subMenu = new Menu();

            Item rules = Client.ContentDatabase.Items["/sitecore/system/Marketing Center/Personalization/"];
            foreach (Item ruleItem in rules.Axes.SelectItems(".//*[@@tid='{550B5CEF-242C-463F-8ED5-983922A39863}']"))
            {
                string id = "L" + ShortID.NewId();
                string displayName = ruleItem.DisplayName;
                string ruleIcon = ruleItem.Appearance.Icon;
                string ruleDescription = GetRuleDescription(ruleItem);

                if (RuleIsActive(ruleItem, contextItem))
                {
                    ruleIcon = "Applications/16x16/star_blue.png";
                }

                if (displayName.Length > 0)
                {
                    subMenu.Add(id, "<b>" + displayName + " </b>" + ruleDescription, ruleIcon, string.Empty, "", false, string.Empty, MenuItemType.Normal);
                }
            }

            SheerResponse.EnableOutput();
            SheerResponse.ShowPopup(menuId, "right", subMenu);
        }

        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            //Only works in preview mode
            if (WebUtil.GetQueryString("mode") != "preview")
            {
                return CommandState.Disabled;
            }
            //Disable if can't access the persona helper (likely means analytics is disabled)
            if (PersonaHelper.GetHelper() == null)
            {
                return CommandState.Disabled;
            }
            return base.QueryState(context);
        }

        // Methods
        /// <summary>
        /// Main menu method, controls submenu popups etc.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            //TODO: Bit messy, needs a better way or broken up into seperate webedit classes
            if (context.Parameters["showprofiles"] != null)
            {
                ShowProfiles(context.Parameters["showprofiles"]);
                return;
            }

            if (context.Parameters["showkeys"] != null)
            {
                ShowKeys(context.Parameters["showkeys"]);
                return;
            }

            if (context.Parameters["showpersonas"] != null)
            {
                ShowPersonas(context.Parameters["showpersonas"]);
                return;
            }

            if (context.Parameters["showrules"] != null)
            {
                ShowRules(context.Parameters["showrules"], context.Items[0]);
                return;
            }

            //If the request wasn't for a particular submenu then output the base menu.
            Assert.ArgumentNotNull(context, "context");
            if (context.Items.Length == 1)
            {
                SheerResponse.DisableOutput();
                Menu menuControl = new Menu();

                string profilesMenuId = ShortID.NewId().ToString();
                menuControl.Add(profilesMenuId, "Profile Scores", "business/32x32/radar-chart.png", string.Empty,
                                string.Format("webedit:personamenu(showprofiles={0})", profilesMenuId), false,
                                string.Empty, MenuItemType.Submenu);

                string personasMenuId = ShortID.NewId().ToString();
                menuControl.Add(personasMenuId, "Personas", "People/16x16/users2.png", string.Empty,
                                string.Format("webedit:personamenu(showpersonas={0})", personasMenuId), false,
                                string.Empty, MenuItemType.Submenu);

                string activeRulesMenuId = ShortID.NewId().ToString();
                menuControl.Add(activeRulesMenuId, "Active Rules", "Software/32x32/shape_circle.png", string.Empty,
                                string.Format("webedit:personamenu(showrules={0})", activeRulesMenuId), false,
                                string.Empty, MenuItemType.Submenu);

                menuControl.AddDivider();
                menuControl.Add(ShortID.NewId().ToString(), "Reset Profile", "Applications/32x32/recycle.png", string.Empty,
                                "webedit:resetprofile", false,
                                string.Empty, MenuItemType.Normal);

                SheerResponse.EnableOutput();
                SheerResponse.ShowPopup("ChangePersonaButton", "below", menuControl);
            }
        }

        /// <summary>
        /// Evaluates if rule is active.
        /// </summary>
        /// <param name="ruleItem">The rule item.</param>
        /// <param name="contextItem">The context item.</param>
        /// <returns></returns>
        private static bool RuleIsActive(Item ruleItem, Item contextItem)
        {
            var personaHelper = PersonaHelper.GetHelper();
            personaHelper.SetAnalyticsContext();

            foreach (Rule<ConditionalRenderingsRuleContext> rule in RuleFactory.GetRules<ConditionalRenderingsRuleContext>(new[] { ruleItem }, "Rule").Rules)
            {
                if (rule.Condition != null)
                {
                    var ruleContext = new ConditionalRenderingsRuleContext(new List<RenderingReference>(), new RenderingReference(new Control())) { Item = contextItem };
                    var stack = new RuleStack();
                    rule.Condition.Evaluate(ruleContext, stack);
                    if (ruleContext.IsAborted)
                    {
                        return false;
                    }

                    bool ruleOutcome = false;
                    if ((stack.Count != 0) && ((bool)stack.Pop()))
                    {
                        ruleOutcome = true;
                    }
                    if (rule.Condition.GetType() == typeof(VisitorIdentificationCondition<ConditionalRenderingsRuleContext>))
                    {
                        ruleOutcome = !ruleOutcome;
                    }
                    return ruleOutcome;
                }
            }
            return false;
        }
    }
}