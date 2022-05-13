// <copyright file="QandAResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class QandAResponse
    {
        /// <summary>
        /// Gets or sets the name of the user that originally asked by the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the question that was asked originally asked by the user.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the answer for the question that has been asked.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the answer for the question that has been asked.
        /// </summary>
        public double? Score { get; set; }
    }
}
