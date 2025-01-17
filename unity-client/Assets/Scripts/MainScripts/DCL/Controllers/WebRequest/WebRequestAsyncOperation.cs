using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    /// <summary>
    /// Our custom request async operation to be used with the WebRequestController.
    /// </summary>
    public class WebRequestAsyncOperation : CustomYieldInstruction
    {
        /// <summary>
        /// Event that will be invoked when the request has been completed.
        /// </summary>
        public event Action<WebRequestAsyncOperation> completed;

        /// <summary>
        /// WebRequest that is being managed.
        /// </summary>
        public UnityWebRequest webRequest { get; private set; }

        /// <summary>
        /// Returns true after the request has finished communicating with the remote server.
        /// </summary>
        public bool isDone { get; private set; }

        /// <summary>
        /// Returns true if webRequest has been disposed (webRequest = null).
        /// </summary>
        public bool isDisposed { get { return webRequest == null; } }

        /// <summary>
        /// Set to true for disposing the request just after it has been completed.
        /// </summary>
        public bool disposeOnCompleted { get; set; } = false;

        public override bool keepWaiting { get { return webRequest != null && !webRequest.isDone; } }

        public WebRequestAsyncOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
            isDone = false;
        }

        /// <summary>
        /// Mark the request as completed and throw the corresponding event.
        /// </summary>
        internal void SetAsCompleted()
        {
            isDone = true;
            completed?.Invoke(this);

            if (disposeOnCompleted)
                Dispose();
        }

        /// <summary>
        /// If in progress, halts the request as soon as possible.
        /// </summary>
        public void Abort()
        {
            if (webRequest == null || isDone)
                return;

            webRequest.Abort();
        }

        /// <summary>
        /// Signals that this request is no longer being used, and should clean up any resources it is using (it aborts the request before disposing).
        /// </summary>
        public void Dispose()
        {
            Abort();

            if (webRequest == null)
                return;

            webRequest.Dispose();
            webRequest = null;
        }
    }
}