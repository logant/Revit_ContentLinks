using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elk.UpdateableLinks
{
    public enum ButtonType { SplitButton, PushButton, StackButton };
    public class CommandInfo
    {
        public string CommandId { get; set; }
        public string CommandName { get; set; }
        public string Icon { get; set; }
        public string Address { get; set; }
        public string Tooltip { get; set; }
        public bool Primary { get; set; }
        public ButtonInfo Button { get; set; }
        public PanelInfo Panel { get; set; }

        public string FullAssemblyPath { get; set; }
    }

    public class ButtonInfo
    {
        public string ButtonId { get; set; }
        public List<CommandInfo> Commands { get; set; }
        public ButtonType ButtonType { get; set; }
        public string Name { get; set; }
    }

    public class PanelInfo
    {
        public string TabName { get; set; }
        public string PanelName { get; set; }
        public List<ButtonInfo> Buttons { get; set; }

        public string VisibleName
        {
            get { return TabName + " : " + PanelName; }
        }
    }
}
