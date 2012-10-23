using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace ChangePersona
{
    public class EditProfileKey : WebEditCommand
    {
        /// <summary>
        /// Webeditcommand execute command, checks the context isn't null and hands over to the run method.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            //TODO: Could contents of Run now be in here?
            Assert.ArgumentNotNull(context, "context");
            Context.ClientPage.Start(this, "Run", context.Parameters);
        }

        /// <summary>
        /// If postback: sets profile key to supplied value
        /// Otherwise: prompt user for new value
        /// </summary>
        /// <param name="args">The args.</param>
        protected void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.Parameters["profilename"], "profilename");
            Assert.ArgumentNotNull(args.Parameters["profilekey"], "profilekey");

            if (args.IsPostBack)
            {
                if (args.HasResult)
                {
                    int profileKeyValue;
                    if(int.TryParse(args.Result,out profileKeyValue))
                    {
                        PersonaHelper personaHelper = PersonaHelper.GetHelper();
                        personaHelper.UpdateProfileKey(args.Parameters["profilename"], args.Parameters["profilekey"], profileKeyValue);
                        SheerResponse.Redraw();
                    }
                }
            }
            else
            {
                Context.ClientPage.ClientResponse.Input("Please enter new profile key value.", "0");
                args.WaitForPostBack();
            }
        }
    }
}
