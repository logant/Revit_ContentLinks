using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Elk.UpdateableLinks
{
    /// <summary>
    /// Interaction logic for ManageLinksForm.xaml
    /// </summary>
    public partial class ManageLinksForm : Window
    {
        LinearGradientBrush eBrush = new LinearGradientBrush(Color.FromArgb(255, 195, 195, 195), Color.FromArgb(255, 245, 245, 245), new Point(0, 0), new Point(0, 1));
        SolidColorBrush lBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

        string filePath = null;
        List<string> panelNames;
        ObservableCollection<PanelInfo> panels;

        PanelInfo selectedPanel;
        ObservableCollection<ButtonInfo> buttonList;
        ButtonInfo selectedButton;
        ObservableCollection<CommandInfo> linkList;
        
        CommandInfo selectedLink;
        
        TextRange textRange = null;
        string xmlStr = string.Empty;

        public ManageLinksForm(string xmlFilePath, List<PanelInfo> xmlPanels)
        {
            filePath = xmlFilePath;
            panels = new ObservableCollection<PanelInfo>();
            foreach (PanelInfo pnl in xmlPanels)
            {
                panels.Add(pnl);
            }
            InitializeComponent();

            panelNames = new List<string>();
            foreach(PanelInfo pnl in panels)
            {
                panelNames.Add(pnl.TabName + " - " + pnl.PanelName);
            }

            panelsListView.ItemsSource = panels;
            panelsListView.DisplayMemberPath = "VisibleName";

            BuildXML();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void panelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            int index = panelsListView.SelectedIndex;
            
            try
            {
                if (index >= 0 && selectedPanel != null)
                {
                    List<PanelInfo> tempList = new List<PanelInfo>();
                    foreach (PanelInfo pnl in panels)
                    {
                        tempList.Add(pnl);
                    }
                   
                   
                    tempList.RemoveAt(index);
                   
                    selectedButton = null;
                    buttonList = new ObservableCollection<ButtonInfo>();
                    linkList = new ObservableCollection<CommandInfo>();
                    buttonsListView.ItemsSource = buttonList;
                    selectedPanel = null;
                    panelNames.RemoveAt(index);
                   
                    panels = new ObservableCollection<PanelInfo>();
                    foreach (PanelInfo pnl in tempList)
                    {
                        panels.Add(pnl);
                    }
                   
                    BuildXML();
                   
                    panelsListView.ItemsSource = panels;
                    panelsListView.DisplayMemberPath = "VisibleName";
                   
                }

            }
            catch { }
        }

        private void panelRemoveButton_MouseEnter(object sender, MouseEventArgs e)
        {
            panelRemoveRect.Fill = eBrush;
        }

        private void panelRemoveButton_MouseLeave(object sender, MouseEventArgs e)
        {
            panelRemoveRect.Fill = lBrush;
        }

        private void panelAddButton_Click(object sender, RoutedEventArgs e)
        {
            if(tabnameTextBox.Text != null && tabnameTextBox.Text.Length > 0 && panelnameTextBox.Text != null && panelnameTextBox.Text.Length > 0 && !panelNames.Contains(tabnameTextBox.Text + " - " + panelnameTextBox.Text))
            {
                try
                {
                    PanelInfo pnl = new PanelInfo();
                    pnl.Buttons = new List<ButtonInfo>();
                    pnl.TabName = tabnameTextBox.Text;
                    pnl.PanelName = panelnameTextBox.Text;
                    
                    panelNames.Add(pnl.TabName + " - " + pnl.PanelName);
                    panels.Add(pnl);
                    selectedLink = null;
                    selectedButton = null;
                    
                    panelsListView.ItemsSource = panels;
                    
                    // Clear the links
                    linkList = new ObservableCollection<CommandInfo>();
                    linksListView.ItemsSource = linkList;

                    // clear the buttons
                    buttonList = new ObservableCollection<ButtonInfo>();
                    buttonsListView.ItemsSource = buttonList;

                    tabnameTextBox.Text = string.Empty;
                    panelnameTextBox.Text = string.Empty;
                    BuildXML();
                }
                catch { }
            }
        }

        private void panelAddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            panelAddRect.Fill = eBrush;
        }

        private void panelAddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            panelAddRect.Fill = lBrush;
        }

        private void buttonRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = buttonsListView.SelectedIndex;

                if (index >= 0 && selectedPanel != null)
                {
                    List<ButtonInfo> tempList = selectedPanel.Buttons;

                    tempList.RemoveAt(index);

                    selectedButton = null;
                    buttonList = new ObservableCollection<ButtonInfo>();
                    foreach(ButtonInfo btn in tempList)
                    {
                        buttonList.Add(btn);
                    }

                    selectedPanel.Buttons = tempList;
                    linkList.Clear();
                    linkList = new ObservableCollection<CommandInfo>();
                    BuildXML();

                    buttonsListView.ItemsSource = buttonList;
                    buttonsListView.DisplayMemberPath = "Name";
                    linksListView.ItemsSource = linkList;
                    linksListView.DisplayMemberPath = "CommandName";
                }
            }
            catch { }
        }

        private void buttonRemoveButton_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonRemoveRect.Fill = eBrush;
        }

        private void buttonRemoveButton_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonRemoveRect.Fill = lBrush;
        }

        private void buttonAddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPanel != null)
                {
                    ButtonInfo bi = new ButtonInfo();
                    if (pushRadioButton.IsChecked.HasValue && pushRadioButton.IsChecked.Value)
                    {
                        List<ButtonInfo> tempList = selectedPanel.Buttons;

                        bi.ButtonType = ButtonType.PushButton;
                        if (buttonNameTextBox.Text != null && buttonNameTextBox.Text.Length > 0)
                            bi.Name = buttonNameTextBox.Text;
                        else
                            bi.Name = "Button";
                        bi.Commands = new List<CommandInfo>();
                        tempList.Add(bi);

                        buttonList = new ObservableCollection<ButtonInfo>();
                        foreach (ButtonInfo btn in tempList)
                        {
                            buttonList.Add(btn);
                        }

                        selectedPanel.Buttons = tempList;

                        BuildXML();

                        buttonNameTextBox.Text = string.Empty;

                        buttonsListView.ItemsSource = buttonList;
                        buttonsListView.DisplayMemberPath = "Name";


                        buttonsListView.SelectedIndex = buttonList.Count - 1;
                    }
                    else if (splitRadioButton.IsChecked.HasValue && splitRadioButton.IsChecked.Value)
                    {
                        List<ButtonInfo> tempList = selectedPanel.Buttons;

                        bi.ButtonType = ButtonType.SplitButton;
                        if (buttonNameTextBox.Text != null && buttonNameTextBox.Text.Length > 0)
                            bi.Name = buttonNameTextBox.Text;
                        else
                            bi.Name = "Button";
                        bi.Commands = new List<CommandInfo>();
                        tempList.Add(bi);
                        buttonList = new ObservableCollection<ButtonInfo>();
                        foreach (ButtonInfo btn in tempList)
                        {
                            buttonList.Add(btn);
                        }

                        selectedPanel.Buttons = tempList;

                        BuildXML();

                        buttonNameTextBox.Text = string.Empty;
                        buttonsListView.ItemsSource = buttonList;
                        buttonsListView.DisplayMemberPath = "Name";
                        buttonsListView.SelectedIndex = buttonList.Count - 1;
                    }
                }
                else
                    MessageBox.Show("Select a PanelSet before trying to add a button.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + ex.Message);
            }
        }

        private void buttonAddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonAddRect.Fill = eBrush;
        }

        private void buttonAddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonAddRect.Fill = lBrush;
        }

        private void linkRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = linksListView.SelectedIndex;

                if (index >= 0 && selectedButton != null)
                {
                    try
                    {
                        List<CommandInfo> tempList = linkList.ToList();
                        tempList.RemoveAt(index);
                        linkList = new ObservableCollection<CommandInfo>();
                        foreach (CommandInfo cmd in tempList)
                        {
                            linkList.Add(cmd);
                        }
                    }
                    catch { }
                    selectedButton.Commands = linkList.ToList();

                    try
                    {
                        selectedPanel.Buttons[buttonsListView.SelectedIndex] = selectedButton;
                        panels[panelsListView.SelectedIndex] = selectedPanel;
                    }
                    catch { }
                    BuildXML();

                    linksListView.ItemsSource = linkList;
                    linksListView.DisplayMemberPath = "CommandName";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + ex.ToString());
            }
        }

        private void linkRemoveButton_MouseEnter(object sender, MouseEventArgs e)
        {
            linkRemoveRect.Fill = eBrush;
        }

        private void linkRemoveButton_MouseLeave(object sender, MouseEventArgs e)
        {
            linkRemoveRect.Fill = lBrush;
        }

        private void linkAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPanel != null && selectedButton != null)
            {
                if (addressTextBox.Text != null && addressTextBox.Text.Length > 0 && linkNameTextBox.Text != null && linkNameTextBox.Text.Length > 0)
                {
                    // Create a new command
                    CommandInfo cmd = new CommandInfo();
                    cmd.Address = addressTextBox.Text;
                    cmd.CommandName = linkNameTextBox.Text;
                    if (iconTextBox.Text != null && iconTextBox.Text.Length > 0)
                        cmd.Icon = iconTextBox.Text;
                    if (tooltipTextBox.Text != null && tooltipTextBox.Text.Length > 0)
                        cmd.Tooltip = tooltipTextBox.Text;

                    if ((selectedButton.ButtonType == ButtonType.SplitButton && selectedButton.Commands.Count < 1) || selectedButton.ButtonType == ButtonType.PushButton)
                        cmd.Primary = true;

                    cmd.Button = selectedButton;
                    cmd.Panel = selectedPanel;

                    linkList.Add(cmd);
                    selectedButton.Commands = linkList.ToList();

                    BuildXML();

                    linksListView.ItemsSource = linkList;
                    linksListView.DisplayMemberPath = "CommandName";
                    linksListView.SelectedIndex = 0;
                    linksListView.SelectedIndex = linkList.Count - 1;

                    addressTextBox.Text = string.Empty;
                    linkNameTextBox.Text = string.Empty;
                    iconTextBox.Text = string.Empty;
                    tooltipTextBox.Text = string.Empty;
                }
            }
            else
                MessageBox.Show("Select a panel and button before adding a command.");
        }

        private void linkAddButton_MouseEnter(object sender, MouseEventArgs e)
        {
            linkAddRect.Fill = eBrush;
        }

        private void linkAddButton_MouseLeave(object sender, MouseEventArgs e)
        {
            linkAddRect.Fill = lBrush;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void cancelButton_MouseEnter(object sender, MouseEventArgs e)
        {
            cancelRect.Fill = eBrush;
        }

        private void cancelButton_MouseLeave(object sender, MouseEventArgs e)
        {
            cancelRect.Fill = lBrush;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool compiled = LinkCommon.CompileButtons(panels.ToList());

                Close();
                if (compiled)
                    MessageBox.Show("NOTE:\nChanges to the Links will appear after restarting Revit.");
                else
                    MessageBox.Show("Was not able to create new buttons.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error\n" + ex.ToString());
            }
        }

        private void okButton_MouseEnter(object sender, MouseEventArgs e)
        {
            okRect.Fill = eBrush;
        }

        private void okButton_MouseLeave(object sender, MouseEventArgs e)
        {
            okRect.Fill = lBrush;
        }

        public void BuildXML()
        {
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<ContentLinks>");
            foreach (PanelInfo panel in panels)
            {
                sb.AppendLine("  <PanelSet TabName=\"" + panel.TabName + "\" PanelName=\"" + panel.PanelName + "\">");
                foreach(ButtonInfo btn in panel.Buttons)
                {
                    sb.AppendLine("    <Button name=\"" + btn.Name + "\" type=\"" + btn.ButtonType.ToString() + "\">");
                    foreach(CommandInfo cmd in btn.Commands)
                    {
                        sb.AppendLine("      <Link>");
                        sb.AppendLine("        <Name>" + cmd.CommandName + "</Name>");
                        sb.AppendLine("        <Address>" + cmd.Address + "</Address>");
                        if(cmd.Icon != null && cmd.Icon.Length > 0)
                            sb.AppendLine("        <Icon>" + cmd.Icon + "</Icon>");
                        if(cmd.Tooltip != null && cmd.Tooltip.Length > 0)
                            sb.AppendLine("        <ToolTip>" + cmd.Tooltip + "</ToolTip>");
                        sb.AppendLine("      </Link>");
                    }
                    sb.AppendLine("    </Button>");
                }
                sb.AppendLine("  </PanelSet>");
            }
            sb.AppendLine("</ContentLinks>");
            xmlStr = sb.ToString();
            FlowDocument flowDoc = new FlowDocument();
            flowDoc.Blocks.Add(new Paragraph(new Run(xmlStr)));
            dataRichTextBox.Document = flowDoc;
            dataRichTextBox.IsReadOnly = true;
        }

        private void panelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPanel = panels[panelsListView.SelectedIndex];

            if (selectedPanel.Buttons == null || selectedPanel.Buttons.Count == 0)
                buttonList = new ObservableCollection<ButtonInfo>();
            else
            {
                buttonList = new ObservableCollection<ButtonInfo>();
                foreach(ButtonInfo btn in selectedPanel.Buttons)
                {
                    buttonList.Add(btn);
                }
            }
            buttonsListView.ItemsSource = buttonList;
            buttonsListView.DisplayMemberPath = "Name";

            selectedButton = null;
            linkList = new ObservableCollection<CommandInfo>();
            linksListView.ItemsSource = linkList;
            
        }

        private void buttonsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (buttonList.Count > 0 && buttonList.Count >= buttonsListView.SelectedIndex)
            {
                selectedButton = buttonList[buttonsListView.SelectedIndex];

                linkList = new ObservableCollection<CommandInfo>();
                if (selectedButton.Commands != null)
                {
                    linkList = new ObservableCollection<CommandInfo>();
                    foreach (CommandInfo cmd in selectedButton.Commands)
                    {
                        linkList.Add(cmd);
                    }
                }

                linksListView.ItemsSource = linkList;
                linksListView.DisplayMemberPath = "CommandName";

                // Reset highlighting.
                if (textRange != null)
                    textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
            }
        }

        private void linksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FlowDocument flowDoc = dataRichTextBox.Document;
                // Reset highlighting.
                if (textRange != null)
                    textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
                
                // Get the active link
                CommandInfo cmd = selectedButton.Commands[linksListView.SelectedIndex];
                selectedLink = cmd;

                // Try to highlight the link
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("      <Link>");
                sb.AppendLine("        <Name>" + cmd.CommandName + "</Name>");
                sb.AppendLine("        <Address>" + cmd.Address + "</Address>");
                if (cmd.Icon != null && cmd.Icon.Length > 0)
                    sb.AppendLine("        <Icon>" + cmd.Icon + "</Icon>");
                if (cmd.Tooltip != null && cmd.Tooltip.Length > 0)
                    sb.AppendLine("        <ToolTip>" + cmd.Tooltip + "</ToolTip>");
                sb.AppendLine("      </Link>");
                string linkStr = sb.ToString();

                textRange = FindWordFromPosition(flowDoc.ContentStart, linkStr);
                textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            }
            catch { }
        }

        TextRange FindWordFromPosition(TextPointer position, string word)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word);
                    if (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun);
                        TextPointer end = start.GetPositionAtOffset(word.Length);
                        return new TextRange(start, end);
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "word" is not found.
            return null;
        }

        private void linkEditButton_Click(object sender, RoutedEventArgs e)
        {
            if(selectedLink  != null)
            {
                try
                {
                    int index = linksListView.SelectedIndex;

                    LinkEditForm editForm = new LinkEditForm(selectedLink);
                    System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();
                    IntPtr handle = proc.MainWindowHandle;
                    System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(editForm);
                    helper.Owner = handle;
                    editForm.ShowDialog();

                    if(editForm.Command != null)
                    {
                        selectedLink = editForm.Command;
                        linkList[index] = selectedLink;

                        selectedButton.Commands = linkList.ToList();

                        BuildXML();
                        linksListView.SelectedIndex = index;

                    }
                }
                catch { }
            }
        }

        private void linkEditButton_MouseEnter(object sender, MouseEventArgs e)
        {
            linkEditRect.Fill = eBrush;
        }

        private void linkEditButton_MouseLeave(object sender, MouseEventArgs e)
        {
            linkEditRect.Fill = lBrush;
        }
    }
}
