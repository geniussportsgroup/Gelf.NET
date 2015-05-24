namespace Gelf
{
    public interface IGelfPublisher
    {
        /// <summary>
        /// Gets Graylog Server Hostname.
        /// </summary>
        string RemoteHostname { get; }

        /// <summary>
        /// Gets Graylog Server Host Port.
        /// </summary>
        int RemoteHostPort { get; }

        /// <summary>
        /// Publishes a Gelf message to the specified Graylog server.
        /// </summary>
        void Publish(GelfMessage message);
    }
}