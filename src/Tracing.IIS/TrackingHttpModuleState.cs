using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Albumprinter.CorrelationTracking.Tracing.IIS
{
    internal sealed class TrackingHttpModuleState
    {
        private TrackingHttpModuleState(HttpContextBase context, TrackingHttpModuleConfiguration configuration)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            Context = context;
            Configuration = configuration;
            InputStream = Context.Request.InputStream;
            OutputStream = InterceptOutputStream(Context);
            Stopwatch = Stopwatch.StartNew();
        }

        public HttpContextBase Context { get; }
        public TrackingHttpModuleConfiguration Configuration { get; }
        public Stream InputStream { get; }
        public Stream OutputStream { get; }
        public Stopwatch Stopwatch { get; }

        public static bool IsTrackable(HttpContextBase context,  TrackingHttpModuleConfiguration configuration)
        {
            var requestUrl = context.Request.Url?.OriginalString;
            return configuration.AllowedUrls.IsMatch(requestUrl) && !configuration.DeniedUrls.IsMatch(requestUrl);
        }

        public static TrackingHttpModuleState AttachTo(HttpContextBase context, TrackingHttpModuleConfiguration configuration)
        {
            var state = new TrackingHttpModuleState(context, configuration);
            context.Items[typeof(TrackingHttpModuleState).FullName] = state;
            return state;
        }

        public static TrackingHttpModuleState DetachFrom(HttpContextBase context)
        {
            return context.Items[typeof(TrackingHttpModuleState).FullName] as TrackingHttpModuleState;
        }

        public string GetInputHeaders()
        {
            return GetHeaders(Context.Request.Headers, Configuration.AllowedHeaders);
        }

        public string GetInputContent()
        {
            return GetStreamContent(InputStream, Context.Request.ContentEncoding);
        }

        public string GetOutputHeaders()
        {
            return GetHeaders(Context.Response.Headers, Configuration.AllowedHeaders);
        }

        public string GetOutputContent()
        {
            return GetStreamContent(OutputStream, Context.Response.ContentEncoding);
        }

        public static string GetHeaders(NameValueCollection headers, HashSet<string> allowedHeaders)
        {
            var allowed = headers.AllKeys.Where(allowedHeaders.Contains);
            return string.Join(Environment.NewLine, allowed.Select(key => $"{key}: {headers[key]}").ToArray());
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