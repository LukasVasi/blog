namespace Blog.Infrastructure.Options
{
    internal class SmtpOptions
    {
        /// <summary>
        /// The host address of the smtp server.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The port of the smtp server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The username used to authenticate with the smtp server.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// The password used to authenticate with the smtp server.
        /// </summary>
        public string Password { get; set; }
    }
}
