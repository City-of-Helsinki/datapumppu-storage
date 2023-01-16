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
                };
            }
            else
            {
                return new ConsumerConfig
                {
                    BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                    GroupId = _configuration["KAFKA_GROUP_ID"],
                    SaslMechanism = SaslMechanism.ScramSha512,
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslUsername = _configuration["KAFKA_USER_USERNAME"],
                    SaslPassword = _configuration["KAFKA_USER_PASSWORD"],
                    SslCertificatePem = _configuration["SSL_CERT_PEM"],
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
            }
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
            else
            {
                return new ProducerConfig
                {
                    BootstrapServers = _configuration["KAFKA_BOOTSTRAP_SERVER"],
                    SaslMechanism = SaslMechanism.ScramSha512,
                    SecurityProtocol = SecurityProtocol.SaslSsl,
                    SaslUsername = _configuration["KAFKA_USER_USERNAME"],
                    SaslPassword = _configuration["KAFKA_USER_PASSWORD"],
                    SslCertificatePem = _configuration["SSL_CERT_PEM"],
                };
            }
        }

    }
}
