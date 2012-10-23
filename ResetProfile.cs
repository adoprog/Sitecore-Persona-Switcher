using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace ChangePersona
{
    /// <summary>
    /// WebEdit Command to respond to the reset profile scores button
    /// </summary>
    public class ResetProfile : WebEditCommand
    {
        public override void Execute(CommandContext context)
        {
            PersonaHelper personaHelper = PersonaHelper.GetHelper();
            personaHelper.ResetProfiles();

            SheerResponse.Redraw();
        }
    }
}
