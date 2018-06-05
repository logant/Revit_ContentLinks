using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Xml;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

using RevitCommon.Attributes;

namespace Elk.UpdateableLinks
{
    public static class LinkCommon
    {
        public static void GetPanels(string xmlPath, out List<PanelInfo> panels, out List<CommandInfo> allCommands)
        {
            panels = new List<PanelInfo>();
            allCommands = new List<CommandInfo>();

            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                string xmlStr = File.ReadAllText(xmlPath);
                xmlStr = EscapeXMLValue(xmlStr);
                xmlDoc.LoadXml(xmlStr);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                xmlDoc = null;
            }

            if (xmlDoc != null)
            {
                // Get the panelsets
                XmlNodeList panelSets = xmlDoc.SelectNodes("ContentLinks/PanelSet");
                int buttonIdCounter = 0;
                int commandIdCounter = 0;
                
                foreach (XmlNode panelSetNode in panelSets)
                {
                    PanelInfo panel = new PanelInfo();
                    string tab = null;
                    try
                    {
                        tab = panelSetNode.Attributes["TabName"].Value;
                    }
                    catch
                    {
                        tab = null;
                    }

                    string pnlName = null;
                    try
                    {
                        pnlName = panelSetNode.Attributes["PanelName"].Value;
                    }
                    catch
                    {
                        pnlName = null;
                    }
                    if (tab == null)
                        tab = panelSetNode.Attributes["tabName"].Value;
                    if (pnlName == null || pnlName.Length <= 0)
                        pnlName = panelSetNode.Attributes["panelName"].Value;
                    
                    panel.TabName = tab;
                    panel.PanelName = pnlName;

                    List<ButtonInfo> buttons = new List<ButtonInfo>();
                    XmlNodeList buttonNodes = panelSetNode.SelectNodes("Button");
                    if (buttonNodes.Count == 0)
                        continue;
                    foreach (XmlNode buttonNode in buttonNodes)
                    {
                        string button_Style = buttonNode.Attributes["type"].Value;
                        ButtonType bt = ButtonType.PushButton;
                        if (button_Style.ToLower() == "splitbutton")
                            bt = ButtonType.SplitButton;
                        else if (button_Style.ToLower() == "stackbutton")
                            bt = ButtonType.StackButton;
                        else
                            bt = ButtonType.PushButton;

                        string buttonName = buttonNode.Attributes["name"].Value;
                        if (buttonName == null || buttonName.Length == 0)
                            buttonName = "Button";

                        ButtonInfo button = new ButtonInfo();
                        button.ButtonType = bt;
                        button.ButtonId = "Button_" + buttonIdCounter.ToString();
                        button.Name = buttonName;
                        List<CommandInfo> commands = new List<CommandInfo>();
                        XmlNodeList linkNodes = buttonNode.SelectNodes("Link");
                        if (linkNodes.Count == 0)
                            continue;
                        foreach (XmlNode linkNode in linkNodes)
                        {
                            // Get the properties of the link
                            XmlNode nameNode = linkNode.SelectSingleNode("Name");
                            XmlNode iconNode = linkNode.SelectSingleNode("Icon");
                            XmlNode addrNode = linkNode.SelectSingleNode("Address");
                            XmlNode toolTipNode = linkNode.SelectSingleNode("ToolTip");
                            CommandInfo cmd = new CommandInfo();
                            cmd.CommandId = "LinkCmd_" + commandIdCounter.ToString();
                            cmd.CommandName = nameNode.InnerText;
                            if (iconNode != null)
                                cmd.Icon = iconNode.InnerText;
                            if (bt == ButtonType.SplitButton && commands.Count == 0)
                                cmd.Primary = true;
                            else if (bt == ButtonType.PushButton)
                                cmd.Primary = true;
                            else
                                cmd.Primary = false;
                            if(toolTipNode != null)
                                cmd.Tooltip = toolTipNode.InnerText;
                            cmd.Address = UnescapeXMLValue(addrNode.InnerText);
                            cmd.Button = button;
                            cmd.Panel = panel;
                            commands.Add(cmd);
                            allCommands.Add(cmd);
                            commandIdCounter++;
                        }
                        button.Commands = commands;
                        buttons.Add(button);
                        buttonIdCounter++;
                    }
                    panel.Buttons = buttons;
                    panels.Add(panel);
                }
            }
            
        }

        public static void GetPanels(string libPath, out List<PanelInfo> panels)
        {
            panels = new List<PanelInfo>();
            PanelInfo pi = null;

            // Use Reflection to iterate through the buttons.dll
            Assembly assembly = Assembly.LoadFile(libPath);
            foreach (Type t in assembly.GetTypes())
            {
                // This is the command class, we need to get an instance of it and
                // iterate through it's properties to determine what to build
                var obj = Activator.CreateInstance(t);
                string name = string.Empty;
                string tooltip = string.Empty;
                string icon = string.Empty;
                string address = string.Empty;
                string buttonId = string.Empty;
                string buttonType = string.Empty;
                string buttonName = "Button";
                string tabName = string.Empty;
                string panelName = string.Empty;
                string fullAddress = t.Namespace + "." + t.Name;

                foreach (PropertyInfo propInfo in t.GetProperties())
                {
                    switch (propInfo.Name)
                    {
                        case "Name":
                            name = propInfo.GetValue(obj).ToString();
                            if (name.Contains(Environment.NewLine))
                                System.Windows.MessageBox.Show("New Line found?");
                            name.Replace(Environment.NewLine, "\\n");
                            break;
                        case "ToolTip":
                            tooltip = propInfo.GetValue(obj).ToString();
                            break;
                        case "Icon":
                            icon = propInfo.GetValue(obj).ToString();
                            break;
                        case "Address":
                            address = propInfo.GetValue(obj).ToString();
                            break;
                        case "ButtonId":
                            buttonId = propInfo.GetValue(obj).ToString();
                            break;
                        case "ButtonType":
                            buttonType = propInfo.GetValue(obj).ToString();
                            break;
                        case "ButtonName":
                            buttonName = propInfo.GetValue(obj).ToString();
                            break;
                        case "TabName":
                            tabName = propInfo.GetValue(obj).ToString();
                            break;
                        case "PanelName":
                            panelName = propInfo.GetValue(obj).ToString();
                            break;
                    }
                }

                if (pi == null) // construct the pi panel.  This should only occur once for the initial panel
                {
                    pi = new PanelInfo();
                    pi.PanelName = panelName;
                    pi.TabName = tabName;
                    
                    List<ButtonInfo> buttons = new List<ButtonInfo>();
                    ButtonInfo button = new ButtonInfo();
                    button.ButtonId = buttonId;
                    button.Name = buttonName;
                    button.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), buttonType);

                    List<CommandInfo> commands = new List<CommandInfo>();
                    CommandInfo ci = new CommandInfo();

                    ci.CommandName = name;
                    ci.CommandId = t.Name;
                    ci.FullAssemblyPath = fullAddress;
                    ci.Icon = icon;
                    ci.Address = address;
                    ci.Primary = true;
                    ci.Tooltip = tooltip;
                    ci.Button = button;
                    ci.Panel = pi;
                    commands.Add(ci);
                    button.Commands = commands;
                    buttons.Add(button);
                    pi.Buttons = buttons;
                }
                else if (pi.TabName == tabName && pi.PanelName == panelName)
                {
                    // Add the new command to the existing panel
                    List<ButtonInfo> buttons = pi.Buttons;
                    ButtonInfo button = buttons.Last();
                    if (button.ButtonId == buttonId)
                    {
                        // Add the command to this button
                        List<CommandInfo> commands = button.Commands;
                        CommandInfo cmd = new CommandInfo();
                        cmd.CommandName = name;
                        cmd.CommandId = t.Name;
                        cmd.Address = address;
                        cmd.FullAssemblyPath = fullAddress;
                        if (button.ButtonType == ButtonType.SplitButton)
                        {
                            if (icon.Length <= 0 || icon == null)
                                cmd.Icon = null;
                            else
                                cmd.Icon = icon;
                            cmd.Primary = false;
                        }
                        else
                        {
                            cmd.Icon = icon;
                            cmd.Primary = true;
                        }
                        cmd.Tooltip = tooltip;
                        cmd.Button = button;
                        cmd.Panel = pi;
                        commands.Add(cmd);
                        button.Commands = commands;
                        buttons.RemoveAt(buttons.Count - 1);
                        buttons.Add(button);
                        pi.Buttons = buttons;
                    }
                    else
                    {
                        // Command is on a new button
                        List<CommandInfo> commands = new List<CommandInfo>();
                        CommandInfo cmd = new CommandInfo();
                        cmd.CommandName = name;
                        cmd.CommandId = t.Name;
                        cmd.Address = address;
                        cmd.FullAssemblyPath = fullAddress;
                        cmd.Icon = icon;
                        cmd.Primary = true;
                        cmd.Tooltip = tooltip;
                        cmd.Button = button;
                        cmd.Panel = pi;
                        commands.Add(cmd);
                        button = new ButtonInfo();
                        button.ButtonId = buttonId;
                        button.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), buttonType);
                        button.Name = buttonName;
                        button.Commands = commands;
                        buttons.Add(button);
                        pi.Buttons = buttons;
                    }
                }
                else
                {
                    // Add the existing panel to the panels
                    panels.Add(pi);

                    // Create a new PanelSet
                    pi = new PanelInfo();
                    pi.PanelName = panelName;
                    pi.TabName = tabName;

                    List<ButtonInfo> buttons = new List<ButtonInfo>();
                    ButtonInfo button = new ButtonInfo();
                    button.ButtonId = buttonId;
                    button.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), buttonType);

                    List<CommandInfo> commands = new List<CommandInfo>();
                    CommandInfo ci = new CommandInfo();

                    ci.CommandName = name;
                    ci.CommandId = t.Name;
                    ci.FullAssemblyPath = fullAddress;
                    ci.Address = address;
                    ci.Icon = icon;
                    ci.Primary = true;
                    ci.Tooltip = tooltip;
                    ci.Button = button;
                    ci.Panel = pi;
                    commands.Add(ci);
                    button.Commands = commands;
                    buttons.Add(button);
                    pi.Buttons = buttons;
                }
            }
            // Add the remaining panel set
            panels.Add(pi);
        }

        public static void InstantiateButtons(UIControlledApplication application, List<PanelInfo> panels, string buttonsPath, string directory)
        {
            // Add the commands to the Ribbon UI
            foreach (PanelInfo panel in panels)
            {
                // Get the panel information
                string tabName = panel.TabName;
                string panelName = panel.PanelName;

                // go through the buttons we'll be creating
                foreach (ButtonInfo button in panel.Buttons)
                {
                    List<PushButtonData> pushButtons = new List<PushButtonData>();
                    string cmdName = "Name";
                    string cmdText = "Name";
                    foreach (CommandInfo ci in button.Commands)
                    {
                        cmdText = ci.CommandName.Replace("\\n", Environment.NewLine);
                        cmdName = ci.CommandName;
                        // Create the push button
                        PushButtonData pbd = new PushButtonData(ci.CommandName, cmdText, buttonsPath, "Elk.ContentLinks." + ci.CommandId);
                        if (ci.Icon != null && ci.Icon != string.Empty && ci.Primary && File.Exists(Path.Combine(directory, ci.Icon)))
                        {
                            if(File.Exists(ci.Icon))
                                pbd.LargeImage = new BitmapImage(new Uri(ci.Icon));
                            else
                                pbd.LargeImage = new BitmapImage(new Uri(directory + "\\" + ci.Icon));
                        }
                        else if (ci.Primary)
                            pbd.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.DefaultButton.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        if (ci.Tooltip != null && ci.Tooltip != string.Empty)
                            pbd.ToolTip = ci.Tooltip;

                        // Add the button to the list.
                        pushButtons.Add(pbd);

                        // If it's a push button just add it to the panel
                        if (button.ButtonType == ButtonType.PushButton)
                            RevitCommon.UI.AddToRibbon(application, tabName, panelName, pbd);
                    }
                    if (button.ButtonType == ButtonType.SplitButton)
                    {
                        // Create the split button and then add the push buttons to it.
                        SplitButtonData sbd = new SplitButtonData(cmdName, cmdText);
                        SplitButton sb = RevitCommon.UI.AddToRibbon(application, tabName, panelName, sbd);
                        foreach (PushButtonData pbd in pushButtons)
                        {
                            sb.AddPushButton(pbd);
                        }
                        sb.IsSynchronizedWithCurrentItem = false;
                    }
                }
            }
        }

        public static bool CompileButtons(List<PanelInfo> panels)
        {
            // Get the current directory
            string directory = new FileInfo(typeof(LinkCommon).Assembly.Location).DirectoryName;
            
            // Get a list of all commands.
            List<CommandInfo> allCommands = new List<CommandInfo>();
            int commandCount = 0;
            int buttonCount = 0;
            foreach(PanelInfo pnl in panels)
            {
                foreach(ButtonInfo btn in pnl.Buttons)
                {
                    string buttonId = "Button_" + buttonCount.ToString();
                    btn.ButtonId = buttonId;
                    foreach(CommandInfo cmd in btn.Commands)
                    {
                        cmd.CommandId = "LinkCommand_" + commandCount.ToString();
                        cmd.Button = btn;
                        allCommands.Add(cmd);
                        commandCount++;
                    }
                    buttonCount++;
                }
            }
            
            // Create the new code string of the commands
            if (panels != null && panels.Count > 0 && allCommands != null && allCommands.Count > 0)
            {
                // Create the string that will be compiled into the buttons.dll file
                string pluginCode = Properties.Resources.LinkStarter.ToString();
                string commandstr = string.Empty;
                foreach (CommandInfo cmd in allCommands)
                {
                    if (commandstr != string.Empty)
                        commandstr += "\n";
                    string cmdCode = Properties.Resources.CommandStarter.ToString();
                    string cmdCode1 = cmdCode.Replace(@"[#LINKCLASSNAME#]", cmd.CommandId);
                    string cmdCode2 = cmdCode1.Replace(@"[#LINKADDRESS#]", cmd.Address);
                    string cmdCode3 = cmdCode2.Replace(@"[#LINKNAME#]", cmd.CommandName);
                    string cmdCode4 = cmdCode3.Replace(@"[#LINKTOOLTIP#]", cmd.Tooltip);
                    string cmdCode5 = string.Empty;
                    if (cmd.Icon != null && cmd.Icon.Length > 0)
                    {
                        if (cmd.Icon.Contains("\\"))
                        {
                            string iconPath = cmd.Icon.Replace("\\", "\\\\");
                            cmdCode5 = cmdCode4.Replace(@"[#LINKICON#]", iconPath);
                        }
                        else
                            cmdCode5 = cmdCode4.Replace(@"[#LINKICON#]", cmd.Icon);
                    }
                    else
                        cmdCode5 = cmdCode4.Replace(@"[#LINKICON#]", string.Empty);
                    string cmdCode6 = cmdCode5.Replace(@"[#BUTTONID#]", cmd.Button.ButtonId);
                    string cmdCode7 = cmdCode6.Replace(@"[#BUTTONTYPE#]", cmd.Button.ButtonType.ToString());
                    string cmdCode8 = cmdCode7.Replace(@"[#TABNAME#]", cmd.Panel.TabName);
                    string cmdCode9 = cmdCode8.Replace(@"[#PANELNAME#]", cmd.Panel.PanelName);
                    string cmdCode10 = cmdCode9.Replace(@"[#BUTTONNAME#]", cmd.Button.Name);
                    commandstr += cmdCode10;
                }
                string code = pluginCode.Replace(@"[#COMMANDS#]", commandstr);
                // Write out the texty
                //File.WriteAllText(directory + "\\buttons.cs", code);
                
                // Compile the results
                CompilerResults compilerResult = CompileLinks(code, directory);
                foreach (CompilerError error in compilerResult.Errors)
                {
                    TaskDialog.Show("Compiler Errors", error.ErrorText);
                }

                if (compilerResult.Errors.Count > 0)
                    return false;
                else
                    return true;
                //runtimeAssembly = compilerResult.CompiledAssembly;
                //buttonCommandPath = runtimeAssembly.Location;

                //LinkCommon.InstantiateButtons(application, panels, buttonCommandPath, directory);
            }

            return false;
        }

        internal static CompilerResults CompileLinks(string source, string directory)
        {
            string installPath = null;
            foreach (var reference in typeof(ManageLinkApp).Assembly.GetReferencedAssemblies())
            {
                string refPath = Assembly.ReflectionOnlyLoad(reference.FullName).Location.ToLower();
                if (refPath.Contains("revitapi.dll") || refPath.Contains("revitapiui.dll"))
                {
                    installPath = new FileInfo(refPath).DirectoryName;
                    break;
                }
            }

            if (installPath != null)
            {
                CompilerParameters cParams = new CompilerParameters();
                cParams.ReferencedAssemblies.Add("System.dll");
                cParams.ReferencedAssemblies.Add("System.Core.dll");
                cParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                cParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                cParams.ReferencedAssemblies.Add(Path.Combine(installPath, "RevitAPI.dll"));
                cParams.ReferencedAssemblies.Add(Path.Combine(installPath, "RevitAPIUI.dll"));
                cParams.ReferencedAssemblies.Add(Path.Combine(directory, "RevitCommon.dll"));
                cParams.ReferencedAssemblies.Add(typeof(RevitCommon.HKS).Assembly.Location);
                cParams.GenerateExecutable = false;
                cParams.GenerateInMemory = false;
                cParams.IncludeDebugInformation = false;
                if(!File.Exists(directory + "\\Elk.Buttons.dll"))
                    cParams.OutputAssembly = directory + "\\Elk.Buttons.dll";
                else
                    cParams.OutputAssembly = directory + "\\Elk.Buttons_New.dll";


                //CodeDomProvider compiler = CodeDomProvider.CreateProvider("CSharp");
                var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
                CodeDomProvider compiler = new CSharpCodeProvider(options);
                CompilerResults cr = compiler.CompileAssemblyFromSource(cParams, source);
                return cr;
            }
            else
            {
                TaskDialog.Show("Error", "Could not find the Revit install path.");
                return null;
            }
        }

        private static string UnescapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                return null;

            return xmlString.Replace("&apos;", "'").Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
        }

        private static string EscapeXMLValue(string xmlString)
        {
            if (xmlString == null)
                return null;

            return xmlString.Replace("&", "&amp;");
        }

        private static bool IsValidXmlString(string text)
        {
            try
            {
                XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    [ExtApp(Name = "Manage Info Links", Description = "Manage Links to Useful Information", 
        Guid = "b82cbf5e-5cd8-4389-8991-bb39cc7eabf1", Vendor = "HKSL", VendorDescription = "HKS LINE, www.hksline.com",
        ForceEnabled = false, Commands = new [] {"Manage Links"})]
    public class ManageLinkApp : IExternalApplication
    {
        static string appPath;

        public string AppPath
        {
            get { return appPath; }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            appPath = new FileInfo(typeof(ManageLinkApp).Assembly.Location).DirectoryName;
            try
            {
                if (File.Exists(appPath + "\\Elk.Buttons_New.dll"))
                {
                    File.Delete(appPath + "\\Elk.Buttons.dll");
                    File.Move(appPath + "\\Elk.Buttons_New.dll", appPath + "\\Elk.Buttons.dll");
                }

                // Add manage form button
                PushButtonData managePBD = new PushButtonData("Manage Link Buttons", "Manage\nLinks", typeof(ManageLinkApp).Assembly.Location, typeof(ManageLinksCmd).FullName)
                {
                    LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.manageButton.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                    ToolTip = "Manage the Updateable Links",
                };
                managePBD.AvailabilityClassName = "Elk.UpdateableLinks.ZeroDocAvailability";
                RevitCommon.UI.AddToRibbon(application, "Add-Ins", "Links", managePBD);
                

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }

    [ExtApp(Name = "Information Links", Description = "Links to HKS Standards and Useful Information",
        Guid = "4595c82b-efee-405a-bb13-3cd8852bbcf8", Vendor = "HKSL", VendorDescription = "HKS LINE",
        ForceEnabled = true, Commands = new[] { "Auto-Populated" })]
    public class LinkApp : IExternalApplication
    {
        static string appPath;
        string directory;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                appPath = new FileInfo(typeof(LinkApp).Assembly.Location).DirectoryName;
                directory = appPath;

                string buttonLibPath = appPath + "\\Elk.Buttons.dll";

                if(File.Exists(buttonLibPath))
                {
                    List<PanelInfo> panels = new List<PanelInfo>();
                    LinkCommon.GetPanels(buttonLibPath, out panels);

                    //intantiate the buttons
                    LinkCommon.InstantiateButtons(application, panels, buttonLibPath, directory);
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ManageLinksCmd : IExternalCommand
    {
        static string appPath;
        string directory;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                appPath = new FileInfo(typeof(ManageLinkApp).Assembly.Location).DirectoryName;
                directory = appPath;
                string buttonLibPath = directory + "\\Elk.Buttons.dll";

                List<PanelInfo> panels = new List<PanelInfo>();
                
                if (File.Exists(buttonLibPath))
                    LinkCommon.GetPanels(buttonLibPath, out panels);

                System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
                IntPtr handle = proc.MainWindowHandle;
                ManageLinksForm form = new ManageLinksForm(buttonLibPath, panels);
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(form);
                helper.Owner = handle;
                form.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
