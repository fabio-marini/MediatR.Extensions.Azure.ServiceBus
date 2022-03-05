// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.ServiceBus
{
    using System;

    static class Constants
    {
        public const int MaxMessageIdLength = 128;

        public const int MaxPartitionKeyLength = 128;

        public const int MaxSessionIdLength = 128;

        public const string PathDelimiter = @"/";

        public const int RuleNameMaximumLength = 50;

        public const int MaximumSqlFilterStatementLength = 1024;

        public const int MaximumSqlRuleActionStatementLength = 1024;

        public const int DefaultClientPrefetchCount = 0;

        public const int MaxDeadLetterReasonLength = 4096;

        public static readonly long DefaultLastPeekedSequenceNumber = 0;

        public static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromMinutes(1);

        public static readonly TimeSpan ClientPumpRenewLockTimeout = TimeSpan.FromMinutes(5);

        public static readonly TimeSpan MaximumRenewBufferDuration = TimeSpan.FromSeconds(10);

        public static readonly TimeSpan DefaultRetryDeltaBackoff = TimeSpan.FromSeconds(3);

        public static readonly TimeSpan NoMessageBackoffTimeSpan = TimeSpan.FromSeconds(5);

        public const string SasTokenType = "servicebus.windows.net:sastoken";

        public const string JsonWebTokenType = "jwt";

        public const string AadServiceBusAudience = "https://servicebus.azure.net/";

        /// Represents 00:00:00 UTC Thursday 1, January 1970.
        public static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public const int WellKnownPublicPortsLimit = 1023;
    }

    internal class ManagementClientConstants
    {
        public const int QueueNameMaximumLength = 260;
        public const int TopicNameMaximumLength = 260;
        public const int SubscriptionNameMaximumLength = 50;
        public const int RuleNameMaximumLength = 50;

        public const string AtomNamespace = "http://www.w3.org/2005/Atom";
        public const string ServiceBusNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
        public const string XmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        public const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
        public const string SerializationNamespace = "http://schemas.microsoft.com/2003/10/Serialization/";
        public const string AtomContentType = "application/atom+xml";
        public const string apiVersionQuery = "api-version=" + ApiVersion;
        public const string ApiVersion = "2017-04";

        public const string ServiceBusSupplementartyAuthorizationHeaderName = "ServiceBusSupplementaryAuthorization";
        public const string ServiceBusDlqSupplementaryAuthorizationHeaderName = "ServiceBusDlqSupplementaryAuthorization";
        public const string HttpErrorSubCodeFormatString = "SubCode={0}";
        public static string ConflictOperationInProgressSubCode =
            string.Format(HttpErrorSubCodeFormatString, ExceptionErrorCodes.ConflictOperationInProgress.ToString("D"));
        public static string ForbiddenInvalidOperationSubCode =
            string.Format(HttpErrorSubCodeFormatString, ExceptionErrorCodes.ForbiddenInvalidOperation.ToString("D"));

        public static readonly TimeSpan MinimumAllowedTimeToLive = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan MaximumAllowedTimeToLive = TimeSpan.MaxValue;
        public static readonly TimeSpan MinimumLockDuration = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan MaximumLockDuration = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MinimumAllowedAutoDeleteOnIdle = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan MaximumDuplicateDetectionHistoryTimeWindow = TimeSpan.FromDays(7);
        public static readonly TimeSpan MinimumDuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(20);
        public static readonly int MinAllowedMaxDeliveryCount = 1;
        public static readonly int MaxUserMetadataLength = 1024;

        public static char[] InvalidEntityPathCharacters = { '@', '?', '#', '*' };

        // Authorization constants
        public static readonly int SupportedClaimsCount = 3;

        /// <summary>Specifies the error codes of the exceptions.</summary>
        public enum ExceptionErrorCodes
        {
            /// <summary>A parse error encountered while processing a request.</summary>
            BadRequest = 40000,
            /// <summary>A generic unauthorized error.</summary>
            UnauthorizedGeneric = 40100,
            /// <summary>The service bus has no transport security.</summary>
            NoTransportSecurity = 40101,
            /// <summary>The token is missing.</summary>
            MissingToken = 40102,
            /// <summary>The signature is invalid.</summary>
            InvalidSignature = 40103,
            /// <summary>The audience is invalid.</summary>
            InvalidAudience = 40104,
            /// <summary>A malformed token.</summary>
            MalformedToken = 40105,
            /// <summary>The token had expired.</summary>
            ExpiredToken = 40106,
            /// <summary>The audience is not found.</summary>
            AudienceNotFound = 40107,
            /// <summary>The expiry date not found.</summary>
            ExpiresOnNotFound = 40108,
            /// <summary>The issuer cannot be found.</summary>
            IssuerNotFound = 40109,
            /// <summary>The signature cannot be found.</summary>
            SignatureNotFound = 40110,
            /// <summary>The incoming ip has been rejected by policy.</summary>
            IpRejected = 40111,
            /// <summary>The incoming ip is not in acled subnet.</summary>
            IpNotInAcledSubNet = 40112,
            /// <summary>A generic forbidden error.</summary>
            ForbiddenGeneric = 40300,
            /// <summary>Operation is not allowed.</summary>
            ForbiddenInvalidOperation = 40301,
            /// <summary>The endpoint is not found.</summary>
            EndpointNotFound = 40400,
            /// <summary>The destination is invalid.</summary>
            InvalidDestination = 40401,
            /// <summary>The namespace is not found.</summary>
            NamespaceNotFound = 40402,
            /// <summary>The store lock is lost.</summary>
            StoreLockLost = 40500,
            /// <summary>The SQL filters exceeded its allowable maximum number.</summary>
            SqlFiltersExceeded = 40501,
            /// <summary>The correlation filters exceeded its allowable maximum number.</summary>
            CorrelationFiltersExceeded = 40502,
            /// <summary>The subscriptions exceeded its allowable maximum number.</summary>
            SubscriptionsExceeded = 40503,
            /// <summary>A conflict during updating occurred.</summary>
            UpdateConflict = 40504,
            /// <summary>The Event Hub is at full capacity.</summary>
            EventHubAtFullCapacity = 40505,
            /// <summary>A generic conflict error.</summary>
            ConflictGeneric = 40900,
            /// <summary>An operation is in progress.</summary>
            ConflictOperationInProgress = 40901,
            /// <summary>The entity is not found.</summary>
            EntityGone = 41000,
            /// <summary>An internal error that is not specified.</summary>
            UnspecifiedInternalError = 50000,
            /// <summary>The error of data communication.</summary>
            DataCommunicationError = 50001,
            /// <summary>An internal error.</summary>
            InternalFailure = 50002,
            /// <summary>The provider is unreachable.</summary>
            ProviderUnreachable = 50003,
            /// <summary>The server is busy.</summary>
            ServerBusy = 50004,
            /// <summary> Archive Storage Account Server is busy. </summary>
            ArchiveStorageAccountServerBusy = 50005,
            /// <summary> Archive Storage Account ResourceId is invalid. </summary>
            InvalidArchiveStorageAccountResourceId = 50006,
            /// <summary>The error is caused by bad gateway.</summary>
            BadGatewayFailure = 50200,
            /// <summary>The gateway did not receive a timely response from the upstream server.</summary>
            GatewayTimeoutFailure = 50400,
            /// <summary>This exception detail will be used for those exceptions that are thrown without specific any explicit exception detail.</summary>
            UnknownExceptionDetail = 60000,
        }
    }

    /// <summary>
    /// This class can be used to format the path for different Service Bus entity types.
    /// </summary>
    public static class EntityNameHelper
    {
        private const string PathDelimiter = @"/";
        private const string SubscriptionsSubPath = "Subscriptions";
        private const string RulesSubPath = "Rules";
        private const string SubQueuePrefix = "$";
        private const string DeadLetterQueueSuffix = "DeadLetterQueue";
        private const string DeadLetterQueueName = SubQueuePrefix + DeadLetterQueueSuffix;
        private const string Transfer = "Transfer";
        private const string TransferDeadLetterQueueName = SubQueuePrefix + Transfer + PathDelimiter + DeadLetterQueueName;

        /// <summary>
        /// Formats the dead letter path for either a queue, or a subscription.
        /// </summary>
        /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
        /// <returns>The path as a string of the dead letter entity.</returns>
        public static string FormatDeadLetterPath(string entityPath)
        {
            return EntityNameHelper.FormatSubQueuePath(entityPath, EntityNameHelper.DeadLetterQueueName);
        }

        /// <summary>
        /// Formats the subqueue path for either a queue, or a subscription.
        /// </summary>
        /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
        /// <returns>The path as a string of the subqueue entity.</returns>
        public static string FormatSubQueuePath(string entityPath, string subQueueName)
        {
            return string.Concat(entityPath, EntityNameHelper.PathDelimiter, subQueueName);
        }

        /// <summary>
        /// Formats the subscription path, based on the topic path and subscription name.
        /// </summary>
        /// <param name="topicPath">The name of the topic, including slashes.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        public static string FormatSubscriptionPath(string topicPath, string subscriptionName)
        {
            return string.Concat(topicPath, PathDelimiter, SubscriptionsSubPath, PathDelimiter, subscriptionName);
        }

        /// <summary>
        /// Formats the rule path, based on the topic path, subscription name and rule name.
        /// </summary>
        /// <param name="topicPath">The name of the topic, including slashes.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="ruleName">The name of the rule</param>
        public static string FormatRulePath(string topicPath, string subscriptionName, string ruleName)
        {
            return string.Concat(
                topicPath, PathDelimiter,
                SubscriptionsSubPath, PathDelimiter,
                subscriptionName, PathDelimiter,
                RulesSubPath, PathDelimiter, ruleName);
        }

        /// <summary>
        /// Utility method that creates the name for the transfer dead letter receiver, specified by <paramref name="entityPath"/>
        /// </summary>
        public static string Format​Transfer​Dead​Letter​Path(string entityPath)
        {
            return string.Concat(entityPath, PathDelimiter, TransferDeadLetterQueueName);
        }

        internal static void CheckValidQueueName(string queueName, string paramName = "queuePath")
        {
            CheckValidEntityName(GetPathWithoutBaseUri(queueName), ManagementClientConstants.QueueNameMaximumLength, true, paramName);
        }

        internal static void CheckValidTopicName(string topicName, string paramName = "topicPath")
        {
            CheckValidEntityName(topicName, ManagementClientConstants.TopicNameMaximumLength, true, paramName);
        }

        internal static void CheckValidSubscriptionName(string subscriptionName, string paramName = "subscriptionName")
        {
            CheckValidEntityName(subscriptionName, ManagementClientConstants.SubscriptionNameMaximumLength, false, paramName);
        }

        internal static void CheckValidRuleName(string ruleName, string paramName = "ruleName")
        {
            CheckValidEntityName(ruleName, ManagementClientConstants.RuleNameMaximumLength, false, paramName);
        }

        private static void CheckValidEntityName(string entityName, int maxEntityNameLength, bool allowSeparator, string paramName)
        {
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException(paramName);
            }

            // and "\" will be converted to "/" on the REST path anyway. Gateway/REST do not
            // have to worry about the begin/end slash problem, so this is purely a client side check.
            var tmpName = entityName.Replace(@"\", Constants.PathDelimiter);
            if (tmpName.Length > maxEntityNameLength)
            {
                throw new ArgumentOutOfRangeException(paramName, $@"Entity path '{entityName}' exceeds the '{maxEntityNameLength}' character limit.");
            }

            if (tmpName.StartsWith(Constants.PathDelimiter, StringComparison.OrdinalIgnoreCase) ||
                tmpName.EndsWith(Constants.PathDelimiter, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($@"The entity name/path cannot contain '/' as prefix or suffix. The supplied value is '{entityName}'", paramName);
            }

            if (!allowSeparator && tmpName.Contains(Constants.PathDelimiter))
            {
                throw new ArgumentException($@"The entity name/path contains an invalid character '{Constants.PathDelimiter}'", paramName);
            }

            foreach (var uriSchemeKey in ManagementClientConstants.InvalidEntityPathCharacters)
            {
                if (entityName.IndexOf(uriSchemeKey) >= 0)
                {
                    throw new ArgumentException($@"'{entityName}' contains character '{uriSchemeKey}' which is not allowed because it is reserved in the Uri scheme.", paramName);
                }
            }
        }

        private static string GetPathWithoutBaseUri(string entityName)
        {
            // Note: on Linux/macOS, "/path" URLs are treated as valid absolute file URLs.
            // To ensure relative queue paths are correctly rejected on these platforms,
            // an additional check using IsWellFormedOriginalString() is made here.
            // See https://github.com/dotnet/corefx/issues/22098 for more information.
            if (Uri.TryCreate(entityName, UriKind.Absolute, out Uri uriValue) &&
                uriValue.IsWellFormedOriginalString())
            {
                entityName = uriValue.PathAndQuery;
                return entityName.TrimStart('/');
            }
            return entityName;
        }
    }
}
