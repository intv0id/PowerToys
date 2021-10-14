// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Common;
using Jupyscope.Contracts;
using Jupyscope.Extensions;
using Jupyscope.Helpers;
using Jupyscope.Serializers;
using Microsoft.PowerToys.PreviewHandler.JupyterNotebook.Properties;
using Microsoft.PowerToys.PreviewHandler.JupyterNotebook.Telemetry.Events;
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
        /// Extended Browser Control to display markdown html.
        /// </summary>
        private WebBrowserExt _browser;

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

            try
            {
                if (!(dataSource is string filePath))
                {
                    throw new ArgumentException($"{nameof(dataSource)} for {nameof(JupyterNotebookPreviewHandler)} must be a string but was a '{typeof(T)}'");
                }

                string fileText = File.ReadAllText(filePath);
                Regex imageTagRegex = new Regex(@"<[ ]*img.*>");
                if (imageTagRegex.IsMatch(fileText))
                {
                    _infoBarDisplayed = true;
                }

                var notebook = TelescopeConverter.Deserialize<Notebook>(fileText);
                var notebookContentHTML = notebook.ToHtml();
                var notebookHTML = $"{HTMLHeaderHelper.Header}{notebookContentHTML}{HTMLHeaderHelper.Footer}";

                InvokeOnControlThread(() =>
                {
                    _browser = new WebBrowserExt
                    {
                        DocumentText = notebookHTML,
                        Dock = DockStyle.Fill,
                        IsWebBrowserContextMenuEnabled = false,
                        ScriptErrorsSuppressed = true,
                        ScrollBarsEnabled = true,
                        AllowNavigation = false,
                    };
                    Controls.Add(_browser);

                    if (_infoBarDisplayed)
                    {
                        _infoBar = GetTextBoxControl(Resources.BlockedImageInfoText);
                        Resize += FormResized;
                        Controls.Add(_infoBar);
                    }
                });

                PowerToysTelemetry.Log.WriteEvent(new JupyterNotebookFilePreviewed());
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                PowerToysTelemetry.Log.WriteEvent(new JupyterNotebookFilePreviewError { Message = ex.Message });

                InvokeOnControlThread(() =>
                {
                    Controls.Clear();
                    _infoBarDisplayed = true;
                    _infoBar = GetTextBoxControl(Resources.NotebookNotPreviewedError);
                    Resize += FormResized;
                    Controls.Add(_infoBar);
                });
            }
            finally
            {
                base.DoPreview(dataSource);
            }
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
