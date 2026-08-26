using System;
using System.Drawing;
using System.Windows.Forms;
using TimeTracker.Classic.Application;

namespace TimeTracker.Classic.Presentation
{
    internal sealed class RunningApplicationForm : Form
    {
        private readonly ListBox _applications;

        internal RunningApplicationForm(IApplicationCatalog catalog)
        {
            Text = LocalizedText.RunningApplications;
            ClientSize = new Size(600, 330);
            StartPosition = FormStartPosition.CenterParent;
            _applications = new ListBox { Left = 15, Top = 15, Width = 570, Height = 260 };
            foreach (ApplicationChoice application in catalog.GetRunningApplications()) _applications.Items.Add(application);
            Button add = new Button { Left = 405, Top = 290, Width = 85, Text = LocalizedText.Add, DialogResult = DialogResult.OK };
            Button cancel = new Button { Left = 500, Top = 290, Width = 85, Text = LocalizedText.Cancel, DialogResult = DialogResult.Cancel };
            Controls.AddRange(new Control[] { _applications, add, cancel });
            AcceptButton = add;
            CancelButton = cancel;
        }

        internal ApplicationChoice SelectedApplication { get { return _applications.SelectedItem as ApplicationChoice; } }
    }
}
