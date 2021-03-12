// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.IO.Abstractions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Common;
using Markdig;
using Microsoft.PowerToys.PreviewHandler.JupyterNotebook.Properties;
using Microsoft.PowerToys.Telemetry;
using PreviewHandlerCommon;

namespace Microsoft.PowerToys.PreviewHandler.JupyterNotebook
{
    /// <summary>
    /// Win Form Implementation for JupyterNotebook Preview Handler.
    /// </summary>
    public class JupyterNotebookPreviewHandlerControl : FormHandlerControl
    {
        /// <summary>
        /// RichTextBox control to display if external images are blocked.
        /// </summary>
        private RichTextBox _infoBar;

        /// <summary>
        /// True if external image is blocked, false otherwise.
        /// </summary>
        private bool _infoBarDisplayed;

        /// <summary>
        /// Initializes a new instance of the <see cref="JupyterNotebookPreviewHandlerControl"/> class.
        /// </summary>
        public JupyterNotebookPreviewHandlerControl()
        {
        }

        /// <summary>
        /// Start the preview on the Control.
        /// </summary>
        /// <param name="dataSource">Path to the file.</param>
        public override void DoPreview<T>(T dataSource)
        {
            _infoBarDisplayed = false;

            InvokeOnControlThread(() =>
            {
                Controls.Clear();
                _infoBarDisplayed = true;
                _infoBar = GetTextBoxControl(Resources.NotebookNotPreviewedError);
                Resize += FormResized;
                Controls.Add(_infoBar);
            });

            base.DoPreview(dataSource);
        }

        /// <summary>
        /// Gets a textbox control.
        /// </summary>
        /// <param name="message">Message to be displayed in textbox.</param>
        /// <returns>An object of type <see cref="RichTextBox"/>.</returns>
        private RichTextBox GetTextBoxControl(string message)
        {
            RichTextBox richTextBox = new RichTextBox
            {
                Text = message,
                BackColor = Color.LightYellow,
                Multiline = true,
                Dock = DockStyle.Top,
                ReadOnly = true,
            };
            richTextBox.ContentsResized += RTBContentsResized;
            richTextBox.ScrollBars = RichTextBoxScrollBars.None;
            richTextBox.BorderStyle = BorderStyle.None;

            return richTextBox;
        }

        /// <summary>
        /// Callback when RichTextBox is resized.
        /// </summary>
        /// <param name="sender">Reference to resized control.</param>
        /// <param name="e">Provides data for the resize event.</param>
        private void RTBContentsResized(object sender, ContentsResizedEventArgs e)
        {
            RichTextBox richTextBox = (RichTextBox)sender;
            richTextBox.Height = e.NewRectangle.Height + 5;
        }

        /// <summary>
        /// Callback when form is resized.
        /// </summary>
        /// <param name="sender">Reference to resized control.</param>
        /// <param name="e">Provides data for the event.</param>
        private void FormResized(object sender, EventArgs e)
        {
            if (_infoBarDisplayed)
            {
                _infoBar.Width = Width;
            }
        }

        /// <summary>
        /// Callback when image is blocked by extension.
        /// </summary>
        private void ImagesBlockedCallBack()
        {
            _infoBarDisplayed = true;
        }
    }
}
