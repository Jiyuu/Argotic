﻿/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
02/19/2008	brian.kuhn	Created TrackbackClient Class
04/10/2008  brian.kuhn  Implemented fix for work item 9959.
04/10/2008  brian.kuhn  Implemented fix for work item 9961.
****************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading;

using Argotic.Common;
using Argotic.Configuration;

namespace Argotic.Net
{
    /// <summary>
    /// Allows applications to send and received notification pings by using the Trackback peer-to-peer notification protocol.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This implementation of Trackback is based on the Trackback 1.2 specification which can be found 
    ///         at <a href="http://www.sixapart.com/pronet/docs/trackback_spec">http://www.sixapart.com/pronet/docs/trackback_spec</a>.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the TrackbackClient class.">
    ///         <code 
    ///             source="..\..\Documentation\Microsoft .NET 3.5\CodeExamplesLibrary\Core\Net\TrackbackClientExample.cs" 
    ///             region="TrackbackClient" 
    ///         />
    ///     </code>
    /// </example>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Trackback")]
    public class TrackbackClient
    {
        //============================================================
        //	PUBLIC/PRIVATE/PROTECTED MEMBERS
        //============================================================
        #region PRIVATE/PROTECTED/PUBLIC MEMBERS
        /// <summary>
        /// Private member to hold the location of the host computer that client Trackback pings will be sent to.
        /// </summary>
        private Uri clientHost;
        /// <summary>
        /// Private member to hold information such as the application name, version, host operating system, and language. 
        /// </summary>
        private string clientUserAgent  = String.Format(null, "Argotic-Syndication-Framework/{0}", System.Reflection.Assembly.GetAssembly(typeof(TrackbackClient)).GetName().Version.ToString(4));
        /// <summary>
        /// Private member to hold the web request options.
        /// </summary>
        private WebRequestOptions clientOptions = new WebRequestOptions();
        /// <summary>
        /// Private member to hold a value that specifies the amount of time after which an asynchronous send operation times out.
        /// </summary>
        private TimeSpan clientTimeout  = TimeSpan.FromSeconds(15);
        /// <summary>
        /// Private member to hold a value that indictaes if the client sends default credentials when making an Trackback ping request.
        /// </summary>
        private bool clientUsesDefaultCredentials;
        /// <summary>
        /// Private member to hold a value indicating if the client is in the process of sending an Trackback ping request.
        /// </summary>
        private bool clientIsSending;
        /// <summary>
        /// Private member to hold a value indicating if the client asynchronous send operation was cancelled.
        /// </summary>
        private bool clientAsyncSendCancelled;
        /// <summary>
        /// Private member to hold Trackback web request used by asynchronous send operations.
        /// </summary>
        private static WebRequest asyncHttpWebRequest;
        #endregion

        //============================================================
        //	CONSTRUCTORS
        //============================================================
        #region TrackbackClient()
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClient"/> class.
        /// </summary>
        public TrackbackClient()
        {
            //------------------------------------------------------------
            //	Attempt to initialize class state using configuration settings
            //------------------------------------------------------------
            this.Initialize();
        }
        #endregion

        #region TrackbackClient(Uri host)
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified Trackback server.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        public TrackbackClient(Uri host)
        {
            //------------------------------------------------------------
            //	Attempt to initialize class state using configuration settings
            //------------------------------------------------------------
            this.Initialize();

            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.Host   = host;
        }
        #endregion

        #region TrackbackClient(Uri host, string userAgent)
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackbackClient"/> class that sends Trackback pings using the specified Trackback server and user agent.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
        /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is an empty string.</exception>
        public TrackbackClient(Uri host, string userAgent) : this(host)
        {
            //------------------------------------------------------------
            //	Initialize class state using guarded properties
            //------------------------------------------------------------
            this.UserAgent  = userAgent;
        }
        #endregion

        //============================================================
        //	PUBLIC EVENTS
        //============================================================
        #region SendCompleted
        /// <summary>
        /// Occurs when an asynchronous Trackback ping request send operation completes.
        /// </summary>
        /// <seealso cref="SendAsync(TrackbackMessage, Object)"/>
        public event EventHandler<TrackbackMessageSentEventArgs> SendCompleted;
        #endregion

        //============================================================
        //	EVENT HANDLER DELEGATE METHODS
        //============================================================
        #region OnMessageSent(TrackbackMessageSentEventArgs e)
        /// <summary>
        /// Raises the <see cref="SendCompleted"/> event.
        /// </summary>
        /// <param name="e">A <see cref="TrackbackMessageSentEventArgs"/> that contains the event data.</param>
        /// <remarks>
        ///     <para>
        ///         Classes that inherit from the <see cref="TrackbackClient"/> class can override the <see cref="OnMessageSent(TrackbackMessageSentEventArgs)"/> method 
        ///         to perform additional tasks when the <see cref="SendCompleted"/> event occurs.
        ///     </para>
        ///     <para>
        ///         <see cref="OnMessageSent(TrackbackMessageSentEventArgs)"/> also allows derived classes to handle <see cref="SendCompleted"/> without attaching a delegate. 
        ///         This is the preferred technique for handling <see cref="SendCompleted"/> in a derived class.
        ///     </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnMessageSent(TrackbackMessageSentEventArgs e)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            EventHandler<TrackbackMessageSentEventArgs> handler    = null;

            //------------------------------------------------------------
            //	Raise event on registered handler(s)
            //------------------------------------------------------------
            handler = this.SendCompleted;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        //============================================================
        //	PUBLIC PROPERTIES
        //============================================================
        #region Credentials
        /// <summary>
        /// Gets or sets the authentication credentials utilized by this client when making Trackback pings.
        /// </summary>
        /// <value>
        ///     A <see cref="ICredentials"/> object that represents the authentication credentials provided by this client when making Trackback pings. 
        ///     The default is a null reference (Nothing in Visual Basic), which indicates no authentication information will be supplied to identify the maker of the request.
        /// </value>
        public ICredentials Credentials
        {
            get
            {
                return clientOptions.Credentials;
            }

            set
            {
                clientOptions.Credentials = value;
            }
        }
        #endregion

        #region Host
        /// <summary>
        /// Gets or sets the location of the host computer that client Trackback pings will be sent to.
        /// </summary>
        /// <value>A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</value>
        /// <remarks>
        ///     If <see cref="Host"/> is a null reference (Nothing in Visual Basic), <see cref="Host"/> is initialized using the settings in the application or machine configuration files.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        public Uri Host
        {
            get
            {
                return clientHost;
            }

            set
            {
                Guard.ArgumentNotNull(value, "value");
                clientHost = value;
            }
        }
        #endregion

        #region Proxy
        /// <summary>
        /// Gets or sets the web proxy utilized by this client to proxy Trackback pings.
        /// </summary>
        /// <value>
        ///     A <see cref="IWebProxy"/> object that represents the web proxy utilized by this client to proxy Trackback pings. 
        ///     The default is a null reference (Nothing in Visual Basic), which indicates no proxy will be used to proxy the request.
        /// </value>
        public IWebProxy Proxy
        {
            get
            {
                return clientOptions.Proxy;
            }

            set
            {
                clientOptions.Proxy = value;
            }
        }
        #endregion

        #region Timeout
        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which asynchronous send operations will time out.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> that specifies the time-out period. The default value is 15 seconds.</value>
        /// <remarks>
        ///     If <see cref="Timeout"/> is equal to <see cref="TimeSpan.MinValue"/>, <see cref="Timeout"/> is initialized using the settings in the application or machine configuration files.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The time out period is less than zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The time out period is greater than a year.</exception>
        public TimeSpan Timeout
        {
            get
            {
                return clientTimeout;
            }

            set
            {
                if (value.TotalMilliseconds < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else if (value > TimeSpan.FromDays(365))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                else
                {
                    clientTimeout   = value;
                }
            }
        }
        #endregion

        #region UseDefaultCredentials
        /// <summary>
        /// Gets or sets a <see cref="Boolean"/> value that controls whether the <see cref="CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent when making Trackback pings.
        /// </summary>
        /// <value><b>true</b> if the default credentials are used; otherwise <b>false</b>. The default value is <b>false</b>.</value>
        /// <remarks>
        ///     <para>
        ///         Some Trackback servers require that the client be authenticated before the server executes Trackback pings on its behalf. 
        ///         Set this property to <b>true</b> when this <see cref="TrackbackClient"/> object should, if requested by the server, authenticate using the 
        ///         default credentials of the currently logged on user. For client applications, this is the desired behavior in most scenarios.
        ///     </para>
        ///     <para>
        ///         Credentials information can also be specified using the application and machine configuration files. 
        ///         For more information, see <see cref="Argotic.Configuration.XmlRpcClientNetworkElement"/> Element (Network Settings).
        ///     </para>
        ///     <para>
        ///         If the UseDefaultCredentials property is set to <b>false</b>, then the value set in the <see cref="Credentials"/> property 
        ///         will be used for the credentials when connecting to the server. If the UseDefaultCredentials property is set to <b>false</b> 
        ///         and the <see cref="Credentials"/> property has not been set, then Trackback pings are sent to the server anonymously.
        ///     </para>
        /// </remarks>
        public bool UseDefaultCredentials
        {
            get
            {
                return clientUsesDefaultCredentials;
            }

            set
            {
                clientUsesDefaultCredentials = value;
            }
        }
        #endregion

        #region UserAgent
        /// <summary>
        /// Gets or sets information such as the client application name, version, host operating system, and language. 
        /// </summary>
        /// <value>Information such as the client application name, version, host operating system, and language. The default value is an agent that describes this syndication framework.</value>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
        public string UserAgent
        {
            get
            {
                return clientUserAgent;
            }

            set
            {
                Guard.ArgumentNotNullOrEmptyString(value, "value");
                clientUserAgent = value.Trim();
            }
        }
        #endregion

        //============================================================
        //	INTERNAL PROPERTIES
        //============================================================
        #region AsyncSendHasBeenCancelled
        /// <summary>
        /// Gets or sets a value indicating if the client asynchronous send operation was cancelled.
        /// </summary>
        /// <value><b>true</b> if client asynchronous send operation has been cancelled, otherwise <b>false</b>.</value>
        internal bool AsyncSendHasBeenCancelled
        {
            get
            {
                return clientAsyncSendCancelled;
            }

            set
            {
                clientAsyncSendCancelled = value;
            }
        }
        #endregion

        #region SendOperationInProgress
        /// <summary>
        /// Gets or sets a value indicating if the client is in the process of sending an Trackback ping request.
        /// </summary>
        /// <value><b>true</b> if client is in the process of sending an Trackback ping request, otherwise <b>false</b>.</value>
        internal bool SendOperationInProgress
        {
            get
            {
                return clientIsSending;
            }

            set
            {
                clientIsSending = value;
            }
        }
        #endregion

        //============================================================
        //	CALLBACK DELEGATE METHODS
        //============================================================
        #region AsyncSendCallback(IAsyncResult result)
        /// <summary>
        /// Called when a corresponding asynchronous send operation completes.
        /// </summary>
        /// <param name="result">The result of the asynchronous operation.</param>
        private static void AsyncSendCallback(IAsyncResult result)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            TrackbackResponse response      = null;
            WebRequest httpWebRequest       = null;
            TrackbackClient client          = null;
            Uri host                        = null;
            TrackbackMessage message        = null;
            WebRequestOptions options       = null;
            object userToken                = null;     

            //------------------------------------------------------------
            //	Determine if the async send operation completed
            //------------------------------------------------------------
            if (result.IsCompleted)
            {
                //------------------------------------------------------------
                //	Extract the send operations parameters from the user state
                //------------------------------------------------------------
                object[] parameters = (object[])result.AsyncState;
                httpWebRequest      = parameters[0] as WebRequest;
                client              = parameters[1] as TrackbackClient;
                host                = parameters[2] as Uri;
                message             = parameters[3] as TrackbackMessage;
                options             = parameters[4] as WebRequestOptions;
                userToken           = parameters[5];

                //------------------------------------------------------------
                //	Verify expected parameters were found
                //------------------------------------------------------------
                if (client != null)
                {
                    //------------------------------------------------------------
                    //	Get the Trackback response to the Trackback ping request
                    //------------------------------------------------------------
                    WebResponse httpWebResponse = (WebResponse)httpWebRequest.EndGetResponse(result);

                    //------------------------------------------------------------
                    //	Extract the Trackback response to the Trackback ping request
                    //------------------------------------------------------------
                    response    = new TrackbackResponse(httpWebResponse);

                    //------------------------------------------------------------
                    //	Raise SendCompleted event to notify registered handlers of state change
                    //------------------------------------------------------------
                    client.OnMessageSent(new TrackbackMessageSentEventArgs(host, message, response, options, userToken));

                    //------------------------------------------------------------
                    //	Reset async operation in progress indicator
                    //------------------------------------------------------------
                    client.SendOperationInProgress  = false;
                }
            }
        }
        #endregion

        #region AsyncTimeoutCallback(object state, bool timedOut)
        /// <summary>
        /// Represents a method to be called when a <see cref="WaitHandle"/> is signaled or times out.
        /// </summary>
        /// <param name="state">An object containing information to be used by the callback method each time it executes.</param>
        /// <param name="timedOut"><b>true</b> if the <see cref="WaitHandle"/> timed out; <b>false</b> if it was signaled.</param>
        private void AsyncTimeoutCallback(object state, bool timedOut)
        {
            //------------------------------------------------------------
            //	Determine if asynchronous send operation timed out
            //------------------------------------------------------------
            if (timedOut)
            {
                //------------------------------------------------------------
                //	Abort asynchronous send operation
                //------------------------------------------------------------
                if (asyncHttpWebRequest != null)
                {
                    asyncHttpWebRequest.Abort();
                }
            }

            //------------------------------------------------------------
            //	Reset async operation in progress indicator
            //------------------------------------------------------------
            this.SendOperationInProgress    = false;
        }
        #endregion

        //============================================================
        //	PUBLIC METHODS
        //============================================================
        #region Send(TrackbackMessage message)
        /// <summary>
        /// Sends the specified message to a Trackback server to execute an Trackback ping request.
        /// </summary>
        /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
        /// <returns>A <see cref="TrackbackResponse"/> that represents the server's response to the Trackback ping request.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">This <see cref="TrackbackClient"/> has a <see cref="SendAsync(TrackbackMessage, Object)"/> call in progress.</exception>
        public TrackbackResponse Send(TrackbackMessage message)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            TrackbackResponse response   = null;

            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(message, "message");

            //------------------------------------------------------------
            //	Validate client state
            //------------------------------------------------------------
            if(this.Host == null)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send Trackback message. The Host property has not been initialized. \n\r Message payload: {0}", message));
            }
            else if (this.SendOperationInProgress)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send Trackback message. The TrackbackClient has a SendAsync call in progress. \n\r Message payload: {0}", message));
            }

            //------------------------------------------------------------
            //	Execute the Trackback ping request
            //------------------------------------------------------------
            WebRequest webRequest   = TrackbackClient.CreateWebRequest(this.Host, this.UserAgent, message, this.UseDefaultCredentials, this.clientOptions);

            using (WebResponse webResponse = (WebResponse)webRequest.GetResponse())
            {
                response    = new TrackbackResponse(webResponse);
            }

            return response;
        }
        #endregion

        #region SendAsync(TrackbackMessage message, Object userToken)
        /// <summary>
        /// Sends the specified message to an Trackback server to execute an Trackback ping request. 
        /// This method does not block the calling thread and allows the caller to pass an object to the method that is invoked when the operation completes.
        /// </summary>
        /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
        /// <param name="userToken">A user-defined object that is passed to the method invoked when the asynchronous operation completes.</param>
        /// <remarks>
        ///     <para>
        ///         To receive notification when the Trackback ping request has been sent or the operation has been cancelled, add an event handler to the <see cref="SendCompleted"/> event. 
        ///         You can cancel a <see cref="SendAsync(TrackbackMessage, Object)"/> operation by calling the <see cref="SendAsyncCancel()"/> method.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">The <see cref="Host"/> is a <b>null</b> reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidOperationException">This <see cref="TrackbackClient"/> has a <see cref="SendAsync(TrackbackMessage, Object)"/> call in progress.</exception>
        [HostProtectionAttribute(SecurityAction.LinkDemand, ExternalThreading = true)]
        public void SendAsync(TrackbackMessage message, Object userToken)
        {
            //------------------------------------------------------------
            //	Validate parameter
            //------------------------------------------------------------
            Guard.ArgumentNotNull(message, "message");

            //------------------------------------------------------------
            //	Validate client state
            //------------------------------------------------------------
            if (this.Host == null)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send Trackback message. The Host property has not been initialized. \n\r Message payload: {0}", message));
            }
            else if (this.SendOperationInProgress)
            {
                throw new InvalidOperationException(String.Format(null, "Unable to send Trackback message. The TrackbackClient has a SendAsync call in progress. \n\r Message payload: {0}", message));
            }

            //------------------------------------------------------------
            //	Set async operation tracking indicators
            //------------------------------------------------------------
            this.SendOperationInProgress    = true;
            this.AsyncSendHasBeenCancelled  = false;

            //------------------------------------------------------------
            //	Build Trackback web request used to send the Trackback ping request
            //------------------------------------------------------------
            asyncHttpWebRequest = TrackbackClient.CreateWebRequest(this.Host, this.UserAgent, message, this.UseDefaultCredentials, this.clientOptions);

            //------------------------------------------------------------
            //	Get the async response to the web request
            //------------------------------------------------------------
            object[] state      = new object[6] { asyncHttpWebRequest, this, this.Host, message, this.clientOptions, userToken };
            IAsyncResult result = asyncHttpWebRequest.BeginGetResponse(new AsyncCallback(AsyncSendCallback), state);

            //------------------------------------------------------------
            //  Register the timeout callback
            //------------------------------------------------------------
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(AsyncTimeoutCallback), state, this.Timeout, true);
        }
        #endregion

        #region SendAsyncCancel()
        /// <summary>
        /// Cancels an asynchronous operation to send an Trackback ping request.
        /// </summary>
        /// <remarks>
        ///     Use the <see cref="SendAsyncCancel()"/> method to cancel a pending <see cref="SendAsync(TrackbackMessage, Object)"/> operation. 
        ///     If there is an Trackback ping request waiting to be sent, this method releases resources used to execute the send operation and cancels the pending operation. 
        ///     If there is no send operation pending, this method does nothing.
        /// </remarks>
        public void SendAsyncCancel()
        {
            if (this.SendOperationInProgress && !this.AsyncSendHasBeenCancelled)
            {
                this.AsyncSendHasBeenCancelled  = true;
                asyncHttpWebRequest.Abort();
            }
        }
        #endregion

        //============================================================
        //	PRIVATE METHODS
        //============================================================
        #region CreateWebRequest(Uri host, string userAgent, TrackbackMessage message, bool useDefaultCredentials, WebRequestOptions options)
        /// <summary>
        /// Initializes a new <see cref="WebRequest"/> suitable for sending an Trackback ping request using the supplied host, user agent, message, credentials, and proxy.
        /// </summary>
        /// <param name="host">A <see cref="Uri"/> that represents the URL of the host computer used for Trackback transactions.</param>
        /// <param name="userAgent">Information such as the application name, version, host operating system, and language.</param>
        /// <param name="message">A <see cref="TrackbackMessage"/> that represents the information needed to execute the Trackback ping request.</param>
        /// <param name="useDefaultCredentials">
        ///     Controls whether the <see cref="CredentialCache.DefaultCredentials">DefaultCredentials</see> are sent when making Trackback pings.
        /// </param>
        /// <param name="options">A <see cref="WebRequestOptions"/> that holds options that should be applied to web requests.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="host"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="userAgent"/> is an empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="message"/> is a null reference (Nothing in Visual Basic).</exception>
        private static WebRequest CreateWebRequest(Uri host, string userAgent, TrackbackMessage message, bool useDefaultCredentials, WebRequestOptions options)
        {
            //------------------------------------------------------------
            //	Local members
            //------------------------------------------------------------
            HttpWebRequest httpRequest  = null;
            byte[] payloadData;

            //------------------------------------------------------------
            //	Validate parameters
            //------------------------------------------------------------
            Guard.ArgumentNotNull(host, "host");
            Guard.ArgumentNotNullOrEmptyString(userAgent, "userAgent");
            Guard.ArgumentNotNull(message, "message");

            //------------------------------------------------------------
            //	Build the Trackback payload data
            //------------------------------------------------------------
            using(MemoryStream stream = new MemoryStream())
            {
                using(StreamWriter writer = new StreamWriter(stream, message.Encoding))
                {
                    message.WriteTo(writer);
                    writer.Flush();

                    stream.Seek(0, SeekOrigin.Begin);
                    payloadData = message.Encoding.GetBytes((new StreamReader(stream)).ReadToEnd());
                }
            }

            //------------------------------------------------------------
            //	Build Trackback web request used to send an Trackback ping request
            //------------------------------------------------------------
            httpRequest                     = (HttpWebRequest)HttpWebRequest.Create(host);
            httpRequest.Method              = "POST";
            httpRequest.ContentLength       = payloadData.Length;
            httpRequest.ContentType         = String.Format(null, "application/x-www-form-urlencoded; charset={0}", message.Encoding.WebName);
            httpRequest.UserAgent           = userAgent;
            if (options != null) options.ApplyOptions(httpRequest);
            
            if(useDefaultCredentials)
            {
                httpRequest.Credentials     = CredentialCache.DefaultCredentials;
            }

            using (Stream stream = httpRequest.GetRequestStream())
            {
                stream.Write(payloadData, 0, payloadData.Length);
            }

            return httpRequest;
        }
        #endregion

        #region Initialize()
        /// <summary>
        /// Initializes the current instance using the application configuration settings.
        /// </summary>
        /// <seealso cref="XmlRpcClientSection"/>
        private void Initialize()
        {
            TrackbackClientSection clientConfiguration  = PrivilegedConfigurationManager.GetTracbackClientSection();

            if (clientConfiguration != null)
            {
                if(clientConfiguration.Timeout.TotalMilliseconds > 0 && clientConfiguration.Timeout < TimeSpan.FromDays(365))
                {
                    this.Timeout    = clientConfiguration.Timeout;
                }

                if (!String.IsNullOrEmpty(clientConfiguration.UserAgent))
                {
                    this.UserAgent  = clientConfiguration.UserAgent;
                }

                if (clientConfiguration.Network != null)
                {
                    this.UseDefaultCredentials  = clientConfiguration.Network.DefaultCredentials;

                    if (clientConfiguration.Network.Credential != null)
                    {
                        this.Credentials    = clientConfiguration.Network.Credential;
                    }

                    if (clientConfiguration.Network.Host != null)
                    {
                        this.Host   = clientConfiguration.Network.Host;
                    }
                }
            }
        }
        #endregion
    }
}
