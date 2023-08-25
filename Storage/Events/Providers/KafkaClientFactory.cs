using Confluent.Kafka;

namespace Storage.Events.Providers
{
    public interface IKafkaClientFactory
    {
        public IProducer<Null, string> CreateProducer();

        public IConsumer<Null, string> CreateConsumer();
    }

    public class KafkaClientFactory : IKafkaClientFactory
    {
        private readonly IConfiguration _configuration;
        private IHostEnvironment _hostEnvironment;

        public KafkaClientFactory(IHostEnvironment hostEnvironment, IConfiguration configuration)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        public IProducer<Null, string> CreateProducer()
        {
            var config = CreateProducerConfiguration();

            return new ProducerBuilder<Null, string>(config).Build();
        }

        public IConsumer<Null, string> CreateConsumer()
        {
            var config = CreateConsumerConfiguration();

            return new ConsumerBuilder<Null, string>(config).Build();
        }

        private ConsumerConfig CreateConsumerConfiguration()
        {
            if (_hostEnvironment.IsDevelopment())
            {
                return new ConsumerConfig
                {
                    BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                    GroupId = _configuration["KAFKA_GROUP_ID"],
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
