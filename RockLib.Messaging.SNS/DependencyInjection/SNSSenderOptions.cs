﻿using RockLib.Messaging.SNS;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RockLib.Messaging.DependencyInjection
{
    /// <summary>
    /// Defines the settings for creating instances of <see cref="SNSSender"/>.
    /// </summary>
    public class SNSSenderOptions
    {
        private string? _topicArn;

        /// <summary>
        /// Gets or sets the arn of the SNS topic.
        /// </summary>
        public string TopicArn
        {
            [return: MaybeNull] get => _topicArn!;
            set => _topicArn = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the region of the SNS topic.
        /// </summary>
        public string? Region { get; set; }
    }
}