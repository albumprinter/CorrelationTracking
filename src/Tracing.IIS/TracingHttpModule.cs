using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;

namespace Tracing.IIS
{
    public sealed class TracingHttpModule : IHttpModule
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _initialized = false;
        private bool _disposed = false;
        private HttpApplication _application;

        public void Init(HttpApplication application)
        {
            if (!_initialized)
            {
                _initialized = true;
                _application = application;
                _application.BeginRequest += Application_BeginRequest;
                _application.EndRequest += Application_EndRequest;
                _application.Error += Application_Error;
            }
        }

        public void Dispose()
        {
            if (_initialized && !_disposed)
            {
                _disposed = true;
                _application.BeginRequest -= Application_BeginRequest;
                _application.EndRequest -= Application_EndRequest;
                _application.Error -= Application_Error;
            }
        }

        private void Application_BeginRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;

            //var content = request.InputStream != null ? new StreamReader(request.InputStream).ReadToEnd() : null;
            //Log.Info($"{request.HttpMethod} {request.Url.OriginalString} {content}");

            var state = TrackingHttpModuleState.AttachTo(context);
            var headers = state.GetInputHeaders();
            var content = state.GetInputContent();
            if (string.IsNullOrWhiteSpace(content))
            {
                Log.Info($"{request.HttpMethod} {request.Url.OriginalString}{Environment.NewLine}{headers}");
            }
            else
            {
                Log.Info($"{request.HttpMethod} {request.Url.OriginalString}{Environment.NewLine}{headers}{Environment.NewLine}{content}");
            }
        }

        private void Application_EndRequest(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            //Log.Info($"{response.Status} {request.Url.OriginalString}");

            var state = TrackingHttpModuleState.DetachFrom(context);
            if (state != null)
            {
                var content = state.GetOutputContent();
                if (string.IsNullOrWhiteSpace(content))
                {
                    Log.Info($"{response.StatusCode} {request.RawUrl} {state.Stopwatch.Elapsed.TotalMilliseconds}ms");
                }
                else
                {
                    Log.Info(
                        $"{response.StatusCode} {request.RawUrl} {state.Stopwatch.Elapsed.TotalMilliseconds}ms{Environment.NewLine}{content}");
                }
            }
            else
            {
                Log.Info($"{response.StatusCode} {request.Url.OriginalString}");
            }
        }

        private void Application_Error(object sender, EventArgs args)
        {
            var application = (HttpApplication) sender;
            var context = application.Context;
            var request = context.Request;
            var response = context.Response;

            var exception = context.Server.GetLastError();
            Log.Error($"{request.HttpMethod} {request.Url.OriginalString} {response.Status}", exception);
        }

        private sealed class TrackingHttpModuleState
        {
            private TrackingHttpModuleState(HttpContextBase context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }
                Context = context;
                InputStream = Context.Request.InputStream;
                OutputStream = InterceptOutputStream(Context);
                Stopwatch = Stopwatch.StartNew();
            }

            public HttpContextBase Context { get; }
            public Stream InputStream { get; }
            public Stream OutputStream { get; }
            public Stopwatch Stopwatch { get; }

            public static TrackingHttpModuleState AttachTo(HttpContext context)
            {
                return AttachTo(new HttpContextWrapper(context));
            }

            public static TrackingHttpModuleState AttachTo(HttpContextBase context)
            {
                var state = new TrackingHttpModuleState(context);
                context.Items[typeof (TrackingHttpModuleState).FullName] = state;
                return state;
            }

            public static TrackingHttpModuleState DetachFrom(HttpContext context)
            {
                return DetachFrom(new HttpContextWrapper(context));
            }

            public static TrackingHttpModuleState DetachFrom(HttpContextBase context)
            {
                return context.Items[typeof (TrackingHttpModuleState).FullName] as TrackingHttpModuleState;
            }

            public string GetInputContent()
            {
                return GetStreamContent(InputStream, Context.Request.ContentEncoding);
            }

            public string GetInputHeaders()
            {
                var headers = Context.Request.Headers;
                return string.Join(Environment.NewLine, headers.AllKeys.Select(key => $"{key}={headers[key]}"));
            }

            public string GetOutputContent()
            {
                return GetStreamContent(OutputStream, Context.Response.ContentEncoding);
            }

            private static string GetStreamContent(Stream stream, Encoding encoding)
            {
                if (stream.CanRead && stream.CanSeek && stream.Length > 0)
                {
                    var position = stream.Position;
                    try
                    {
                        stream.Position = 0;
                        return new StreamReader(stream, encoding).ReadToEnd();
                    }
                    finally
                    {
                        stream.Position = position;
                    }
                }
                return null;
            }

            private static Stream InterceptOutputStream(HttpContextBase context)
            {
                var spyStream = new SpyStream(context.Response.Filter);
                context.Response.Filter = spyStream;
                return spyStream.Buffer;
            }

            private sealed class SpyStream : Stream
            {
                private readonly Stream origin;

                public SpyStream(Stream origin)
                {
                    this.origin = origin;
                    Buffer = new MemoryStream();
                }

                public MemoryStream Buffer { get; }

                public override bool CanRead => origin.CanRead;
                public override bool CanSeek => origin.CanSeek;
                public override bool CanWrite => origin.CanWrite;
                public override long Length => origin.Length;

                public override long Position
                {
                    get { return origin.Position; }
                    set { origin.Position = value; }
                }

                public override bool CanTimeout => origin.CanTimeout;

                public override int ReadTimeout
                {
                    get { return origin.ReadTimeout; }
                    set { origin.ReadTimeout = value; }
                }

                public override int WriteTimeout
                {
                    get { return origin.WriteTimeout; }
                    set { origin.WriteTimeout = value; }
                }

                public override void SetLength(long value)
                {
                    origin.SetLength(value);
                }

                public override long Seek(long offset, SeekOrigin origin)
                {
                    return this.origin.Seek(offset, origin);
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    return origin.Read(buffer, offset, count);
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    origin.Write(buffer, offset, count);
                    Buffer.Write(buffer, offset, count);
                }

                public override void Flush()
                {
                    origin.Flush();
                }

                public override void Close()
                {
                    origin.Close();
                    base.Close();
                }
            }
        }
    }
}