// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Common;
using Microsoft.PowerToys.Telemetry;

namespace Microsoft.PowerToys.PreviewHandler.JupyterNotebook
{
    /// <summary>
    /// Implementation of preview handler for Jupyter notebook files.
    /// </summary>
    [Guid("465f84b1-5910-4611-b79a-2577c309e9bb")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class JupyterNotebookPreviewHandler : FileBasedPreviewHandler, IDisposable
    {
        private JupyterNotebookPreviewHandlerControl _jupyterNotebookPreviewHandlerControl;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="JupyterNotebookPreviewHandler"/> class.
        /// </summary>
        public JupyterNotebookPreviewHandler()
        {
            Initialize();
        }

        /// <inheritdoc/>
        public override void DoPreview()
        {
            _jupyterNotebookPreviewHandlerControl.DoPreview(FilePath);
        }

        /// <inheritdoc/>
        protected override IPreviewHandlerControl CreatePreviewHandlerControl()
        {
            PowerToysTelemetry.Log.WriteEvent(new Telemetry.Events.JupyterNotebookFileHandlerLoaded());
            _jupyterNotebookPreviewHandlerControl = new JupyterNotebookPreviewHandlerControl();

            return _jupyterNotebookPreviewHandlerControl;
        }

        /// <summary>
        /// Disposes objects
        /// </summary>
        /// <param name="disposing">Is Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _jupyterNotebookPreviewHandlerControl.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
