using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderLayout;
using Sitecore.Rules;
using Sitecore.Rules.ConditionalRenderings;
using Sitecore.SecurityModel;
using Sitecore.Text;

namespace ChangePersona
{
    /// <summary>
    /// Processes renderings with custom rules, when persona is defined.
    /// </summary>
    public class CustomInsertRenderings : InsertRenderings
    {
        /// <summary>
        /// Adds the renderings.
        /// </summary>
        /// <returns>List of renderings, with personalization processed</returns>
        private static IEnumerable<RenderingReference> AddRenderings()
        {
            Item item = Context.Item;
            if (item == null)
            {
                return new RenderingReference[0];
            }

            DeviceItem device = Context.Device;
            if (device == null)
            {
                return new RenderingReference[0];
            }

            RuleList<ConditionalRenderingsRuleContext> globalRules = GetGlobalRules(item);
            List<RenderingReference> collection = new List<RenderingReference>(item.Visualization.GetRenderings(device, true));
            List<RenderingReference> list3 = new List<RenderingReference>(collection);
            foreach (RenderingReference reference in list3)
            {
                string conditions = reference.Settings.Conditions;
                if (!string.IsNullOrEmpty(conditions))
                {
                    List<Item> conditionItems = new ListString(conditions).Select(item.Database.GetItem).Where(x => x != null).ToList();
                    if (conditionItems.Count > 0)
                    {
                        var rules = RuleFactory.GetRules<ConditionalRenderingsRuleContext>(conditionItems, "Rule");
                        var ruleContext = new ConditionalRenderingsRuleContext(collection, reference) { Item = item };

                        if (!PersonaHelper.DisableOtherRules)
                        {
                            rules.Run(ruleContext);
                        }

                        foreach (var conditionItem in conditionItems)
                        {
                            if (!PersonaHelper.RuleIsActive(conditionItem))
                            {
                                continue;
                            }

                            var conditionRules = RuleFactory.GetRules<ConditionalRenderingsRuleContext>(new[] {conditionItem}, "Rule");
                            foreach (var action in conditionRules.Rules.SelectMany(rule => rule.Actions))
                            {
                                action.Apply(ruleContext);
                            }
                        }
                    }
                }

                if (globalRules != null)
                {
                    ConditionalRenderingsRuleContext context4 = new ConditionalRenderingsRuleContext(collection, reference);
                    context4.Item = item;
                    ConditionalRenderingsRuleContext context3 = context4;
                    globalRules.Run(context3);
                }
            }

            return collection.ToArray();
        }

        /// <summary>
        /// Gets the global rules.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private static RuleList<ConditionalRenderingsRuleContext> GetGlobalRules(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            RuleList<ConditionalRenderingsRuleContext> rules = null;
            using (new SecurityDisabler())
            {
                Item parentItem = item.Database.GetItem(ItemIDs.ConditionalRenderingsGlobalRules);
                if (parentItem != null)
                {
                    rules = RuleFactory.GetRules<ConditionalRenderingsRuleContext>(parentItem, "Rule");
                }
            }
            return rules;
        }

        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public override void Process(RenderLayoutArgs args)
        {
            if (!Context.PageMode.IsPreview || ID.IsNullOrEmpty(PersonaHelper.ActivePersona))
            {
                base.Process(args);
            }
            else
            {
                CustomProcess(args);
            }
        }

        /// <summary>
        /// Process the renderings, enabling "Active Rules"
        /// </summary>
        /// <param name="args">The args.</param>
        private static void CustomProcess(RenderLayoutArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            using (new ProfileSection("Insert renderings into page."))
            {
                if (Context.Item != null)
                {
                    IEnumerable<RenderingReference> referenceArray = AddRenderings();
                 
                    if (referenceArray != null)
                    {
                        foreach (RenderingReference reference in referenceArray)
                        {
                            Context.Page.AddRendering(reference);
                        }
                    }
                }
            }
        }
    }
}
