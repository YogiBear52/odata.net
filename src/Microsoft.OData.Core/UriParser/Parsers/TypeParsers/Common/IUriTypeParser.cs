﻿//---------------------------------------------------------------------
// <copyright file="IUriTypeParser.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.OData.Core.UriParser.Parsers.TypeParsers.Common
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.OData.Edm;

    #endregion

    /// <summary>
    /// Interface for UriTypeParser.
    /// To parse the uri of an OData request into objects, the ODataUriParser uses UriTypeParses.
    /// Implementation of this interface will parse a text of an EdmType to it's instance.
    /// </summary>
    public interface IUriTypeParser
    {
        /// <summary>
        /// Parse the given text of EdmType 'targetType' to it's object instance.
        /// Return 'Null' if the text could not be parsed to the requested 'targetType'.
        /// Assign 'parsingException' paramter only in case the text could be parsed to the requested 'targetType', but failed during the parsing proccess.
        /// </summary>
        /// <param name="text">Part of the uri which has to be parsed to a value of EdmType 'targetType'</param>
        /// <param name="targetType">The type which the uri text has to be parsed to</param>
        /// <param name="parsingException">Assign the exception only in case the text could be parsed to the 'targetType' but failed during the parsing process</param>
        /// <returns>If the parsing proceess has succeeded, returns the parsed object, otherwise returns 'Null'</returns>
        object ParseUriStringToType(string text, IEdmTypeReference targetType, out UriTypeParsingException parsingException);

        // Consider add this API:
        // bool TryParseUriStringToType(string text, IEdmTypeReference targetType,out object targetValue, out UriTypeParsingException parsingException);
        //      This can be problematic because of the the 'bool' return type + the out exception parameter.The 'Try' fuction could return 'false' with null exception because it doesn't support the given type
        //      And this not a 'standart' function and it can confuse the clients. Standart 'Try' function assign the out Exception paramter when return value is 'false'.
    }
}
