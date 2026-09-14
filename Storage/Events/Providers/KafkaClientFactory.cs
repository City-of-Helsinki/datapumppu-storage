using Confluent.Kafka;

namespace Storage.Events.Providers
{
    /// <summary>
    /// Defines the contract for creating Kafka clients for producing and consuming messages.
    /// </summary>
    public interface IKafkaClientFactory
    {
        /// <summary>
        /// Creates a new Kafka producer configured for the current environment.
        /// </summary>
        /// <returns>A configured Kafka producer instance.</returns>
        public IProducer<Null, string> CreateProducer();

        /// <summary>
        /// Creates a new Kafka consumer configured for the current environment.
        /// </summary>
        /// <returns>A configured Kafka consumer instance.</returns>
        public IConsumer<Null, string> CreateConsumer();
    }

    /// <summary>
    /// Factory for creating Kafka producers and consumers with environment-specific configuration.
    /// In development, uses simple authentication. In production, uses SASL/SCRAM-SHA-512 with SSL/TLS.
    /// </summary>
    public class KafkaClientFactory : IKafkaClientFactory
    {
        private readonly IConfiguration _configuration;
        private IHostEnvironment _hostEnvironment;

        /// <summary>
        /// Initializes a new instance of the KafkaClientFactory with the required dependencies.
        /// </summary>
        /// <param name="hostEnvironment">Host environment for determining development vs. production configuration.</param>
        /// <param name="configuration">Application configuration containing Kafka connection details.</param>
        public KafkaClientFactory(IHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// Creates a new Kafka producer with environment-specific configuration.
        /// </summary>
        /// <returns>A configured producer instance ready to send messages.</returns>
        public IProducer<Null, string> CreateProducer()
        {
            var config = CreateProducerConfiguration();

            return new ProducerBuilder<Null, string>(config).Build();
        }

        /// <summary>
        /// Creates a new Kafka consumer with environment-specific configuration.
        /// </summary>
        /// <returns>A configured consumer instance ready to receive messages.</returns>
        public IConsumer<Null, string> CreateConsumer()
        {
            var config = CreateConsumerConfiguration();

            return new ConsumerBuilder<Null, string>(config).Build();
        }

        /// <summary>
        /// Creates the consumer configuration based on the current environment.
        /// Development uses basic authentication, production uses SASL/SCRAM-SHA-512 with SSL/TLS.
        /// Production consumer disables auto-commit for manual offset management.
        /// </summary>
        /// <returns>A configured ConsumerConfig instance.</returns>
        private ConsumerConfig CreateConsumerConfiguration()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return new ConsumerConfig
                {
                    BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                    GroupId = _configuration["KAFKA_GROUP_ID"],
                    AllowAutoCreateTopics = true,
                };
            }

            var cert = ParseCert(_configuration["SSL_CERT_PEM"]);

            return new ConsumerConfig
            {
                BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                GroupId = _configuration["KAFKA_GROUP_ID"],
                SaslMechanism = SaslMechanism.ScramSha512,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = _configuration["KAFKA_USER_USERNAME"],
                SaslPassword = _configuration["KAFKA_USER_PASSWORD"],
                SslCaPem = cert,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };
        }

        /// <summary>
        /// Creates the producer configuration based on the current environment.
        /// Development uses basic authentication, production uses SASL/SCRAM-SHA-512 with SSL/TLS.
        /// </summary>
        /// <returns>A configured ProducerConfig instance.</returns>
        private ProducerConfig CreateProducerConfiguration()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return new ProducerConfig
                {
                    BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                };
            }

            var cert = ParseCert(_configuration["SSL_CERT_PEM"]);

            return new ProducerConfig
            {
                BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                SaslMechanism = SaslMechanism.ScramSha512,
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslUsername = _configuration["KAFKA_USER_USERNAME"],
                SaslPassword = _configuration["KAFKA_USER_PASSWORD"],
                SslCaPem = cert
            };
        }

        /// <summary>
        /// Parses and formats SSL certificate from configuration.
        /// The certificate is stored in Key Vault without quotes and BEGIN/END tags for compatibility.
        /// This method reconstructs the proper PEM format.
        /// </summary>
        /// <param name="cert">The base64-encoded certificate content from configuration.</param>
        /// <returns>A properly formatted PEM certificate string.</returns>
        private string ParseCert(string cert)
        {
            // To prevent pipeline errors the keyvault ca.crt is in quotes and without the begin/end tags. 
            cert = cert.Replace("\"", "");

            var certBegin = "-----BEGIN CERTIFICATE-----\n";
            var certEnd = "\n-----END CERTIFICATE-----";

            return certBegin + cert + certEnd;
        }

    }
}
