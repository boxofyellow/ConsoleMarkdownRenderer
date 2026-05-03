using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// A test-only <see cref="HttpMessageHandler"/> that delegates to a caller-supplied function,
    /// enabling full control over HTTP responses without making real network calls.
    /// </summary>
    internal sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        /// <param name="handler">Function invoked for every request; returns the desired <see cref="HttpResponseMessage"/>.</param>
        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_handler(request));
    }

    /// <summary>
    /// A test-only <see cref="IHttpClientFactory"/> that always returns the same pre-built <see cref="HttpClient"/>.
    /// </summary>
    internal sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        /// <param name="client">The <see cref="HttpClient"/> to return from every <see cref="CreateClient"/> call.</param>
        public FakeHttpClientFactory(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc/>
        public HttpClient CreateClient(string name) => _client;
    }
}
