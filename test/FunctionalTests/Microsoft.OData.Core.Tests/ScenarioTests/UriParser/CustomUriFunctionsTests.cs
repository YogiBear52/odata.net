﻿using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm.Library;
using Xunit;
using FluentAssertions;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.OData.Core.Tests.ScenarioTests.UriParser
{
    /// <summary>
    /// Tests the CustomUriFunctions class.
    /// </summary>
    public class CustomUriFunctionsTests
    {
        #region Constants

        // Existing built-in uri functions
        private const string BUILT_IN_GEODISTANCE_FUNCTION_NAME = "geo.distance";

        private readonly FunctionSignatureWithReturnType GEO_DISTANCE_BUILTIN_FUNCTION_SIGNATURE = new FunctionSignatureWithReturnType(
                            EdmCoreModel.Instance.GetDouble(true),
                            EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true),
                            EdmCoreModel.Instance.GetSpatial(EdmPrimitiveTypeKind.GeometryPoint, true));

        #endregion

        #region Add Custom Function

        #region Validation

        [Fact]
        public void AddCustomFunction_FunctionCannotBeNull()
        {
            Action addNullFunctionAction = () =>
                CustomUriFunctions.AddCustomUriFunction("my.MyNullCustomFunction", null);

            addNullFunctionAction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void AddCustomFunction_FunctionNameCannotBeNull()
        {
            FunctionSignatureWithReturnType customFunctionSignature =
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), EdmCoreModel.Instance.GetInt32(false));

            Action addNullFunctionNameAction = () =>
                CustomUriFunctions.AddCustomUriFunction(null, customFunctionSignature);

            addNullFunctionNameAction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void AddCustomFunction_FunctionNameCannotBeEmptyString()
        {
            FunctionSignatureWithReturnType customFunctionSignature =
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), EdmCoreModel.Instance.GetInt32(false));

            Action addCustomFunctionSignature = () =>
                CustomUriFunctions.AddCustomUriFunction(string.Empty, customFunctionSignature);

            addCustomFunctionSignature.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void AddCustomFunction_CannotAddFunctionSignatureWithNullReturnType()
        {
            FunctionSignatureWithReturnType customFunctionSignatureWithNullReturnType =
                new FunctionSignatureWithReturnType(null, EdmCoreModel.Instance.GetInt32(false));

            Action addCustomFunctionSignature = () =>
                CustomUriFunctions.AddCustomUriFunction("my.customFunctionWithNoReturnType",
                                                        customFunctionSignatureWithNullReturnType);

            addCustomFunctionSignature.ShouldThrow<ArgumentNullException>();
        }

        #endregion

        [Fact]
        public void AddCustomFunction_CannotAddFunctionWithNameAlreadyExistsAsBuildIsFunction_OverrideBuiltinFalse_SameNameDifferentArguments()
        {
            FunctionSignatureWithReturnType customFunctionSignature =
                new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false),
                                                    EdmCoreModel.Instance.GetBoolean(false));

            Action addExistingCustomFunctionSignature = () =>
                CustomUriFunctions.AddCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME, customFunctionSignature);

            addExistingCustomFunctionSignature.ShouldThrow<ODataException>().
                WithMessage(Strings.CustomUriFunctions_AddCustomUriFunction_BuiltInExistsNoOverride(BUILT_IN_GEODISTANCE_FUNCTION_NAME));
        }

        [Fact]
        public void AddCustomFunction_CannotAddFunctionWithNameAlreadyExistsAsBuildIsFunction_OverrideBuiltinFalse_SameNameSameArguments()
        {
            Action addExistingCustomFunctionSignature = () =>
                CustomUriFunctions.AddCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME,
                                                        GEO_DISTANCE_BUILTIN_FUNCTION_SIGNATURE);

            addExistingCustomFunctionSignature.ShouldThrow<ODataException>().
                WithMessage(Strings.CustomUriFunctions_AddCustomUriFunction_BuiltInExistsNoOverride(BUILT_IN_GEODISTANCE_FUNCTION_NAME));
        }

        [Fact]
        public void AddCustomFunction_CannotAddFunctionWhichAlreadyExistsAsBuiltInWithSameFullSignature_OverrideBuiltIn()
        {
            try
            {
                // Add exisiting with override 'true'
                Action addCustomFunction = () =>
                    CustomUriFunctions.AddCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME,
                                                            GEO_DISTANCE_BUILTIN_FUNCTION_SIGNATURE,
                                                            true);

                // Assert
                addCustomFunction.ShouldThrow<ODataException>().
                    WithMessage(Strings.CustomUriFunctions_AddCustomUriFunction_BuiltInExistsFullSignature(BUILT_IN_GEODISTANCE_FUNCTION_NAME));
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME);
            }
        }

        [Fact]
        public void AddCustomFunction_ShouldAddFunctionWhichAlreadyExistsAsBuiltInWithSameName_OverrideBuiltIn()
        {
            try
            {
                FunctionSignatureWithReturnType customFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false),
                                                        EdmCoreModel.Instance.GetBoolean(false));

                // Add with override 'true'
                CustomUriFunctions.AddCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME, customFunctionSignature, true);

                FunctionSignatureWithReturnType[] resultFunctionSignaturesWithReturnType =
                    this.GetCustomFunctionSignaturesOrNull(BUILT_IN_GEODISTANCE_FUNCTION_NAME);

                // Assert
                resultFunctionSignaturesWithReturnType.Should().NotBeNull();
                resultFunctionSignaturesWithReturnType.Length.Should().Be(1);
                resultFunctionSignaturesWithReturnType[0].Should().Be(customFunctionSignature);
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(BUILT_IN_GEODISTANCE_FUNCTION_NAME);
            }
        }

        // Existing Custom Function
        [Fact]
        public void AddCustomFunction_CannotAddFunctionWithFullSignatureExistsAsCustomFunction()
        {
            string customFunctionName = "my.ExistingCustomFunction";
            try
            {
                // Preaper
                var existingCustomFunctionSignature = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                // Test
                var newCustomFunctionSignature = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));

                Action addCustomFunction = () =>
                    CustomUriFunctions.AddCustomUriFunction(customFunctionName, newCustomFunctionSignature);

                // Assert
                addCustomFunction.ShouldThrow<ODataException>().
                    WithMessage(Strings.CustomUriFunctions_AddCustomUriFunction_CustomFunctionOverloadExists(customFunctionName));
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        [Fact]
        public void AddCustomFunction_CannotAddFunctionWithFullSignatureExistsAsCustomFunction_OverrideBuiltIn()
        {
            string customFunctionName = "my.ExistingCustomFunction";
            try
            {
                // Preaper
                var existingCustomFunctionSignature = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                // Test
                var newCustomFunctionSignature = new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));

                Action addCustomFunction = () =>
                    CustomUriFunctions.AddCustomUriFunction(customFunctionName, newCustomFunctionSignature, true);

                // Asserts
                addCustomFunction.ShouldThrow<ODataException>().
                    WithMessage(Strings.CustomUriFunctions_AddCustomUriFunction_CustomFunctionOverloadExists(customFunctionName));
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        [Fact]
        public void AddCustomFunction_CustomFunctionDoesntExist_ShouldAdd()
        {
            string customFunctionName = "my.NewCustomFunction";
            try
            {
                // New not existing custom function
                var newCustomFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetInt32(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, newCustomFunctionSignature);

                // Assert
                // Make sure both signatures exists
                FunctionSignatureWithReturnType[] customFunctionSignatures = 
                    GetCustomFunctionSignaturesOrNull(customFunctionName);

                customFunctionSignatures.Length.Should().Be(1);
                customFunctionSignatures[0].Should().BeSameAs(newCustomFunctionSignature);
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        [Fact]
        public void AddCustomFunction_CustomFunctionDoesntExist_ShouldAdd_NoArgumnetsToFunctionSignature()
        {
            string customFunctionName = "my.NewCustomFunction";
            try
            {
                // New not existing custom function - function without any argumnets
                var newCustomFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, newCustomFunctionSignature);

                // Assert
                // Make sure both signatures exists
                FunctionSignatureWithReturnType[] customFunctionSignatures =
                    GetCustomFunctionSignaturesOrNull(customFunctionName);

                customFunctionSignatures.Length.Should().Be(1);
                customFunctionSignatures[0].Should().BeSameAs(newCustomFunctionSignature);
            }
            finally
            {
                // Clean from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        [Fact]
        public void AddCustomFunction_CustomFunctionNameExistsButNotFullSignature_ShouldAddAsAnOverload()
        {
            string customFunctionName = "my.ExistingCustomFunction";
            try
            {
                // Preaper
                FunctionSignatureWithReturnType existingCustomFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                //Test


                // Same name, but different signature
                var newCustomFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetInt32(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, newCustomFunctionSignature);

                // Assert
                // Make sure both signatures exists
                bool areSiganturesAdded =
                    GetCustomFunctionSignaturesOrNull(customFunctionName).
                        All(x => x.Equals(existingCustomFunctionSignature) || x.Equals(newCustomFunctionSignature));

                areSiganturesAdded.Should().BeTrue();
            }
            finally
            {
                // Clean both signatures from CustomUriFunctions cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        #endregion

        #region Remove Custom Function

        // Validation

        #region Validation

        [Fact]
        public void RemoveCustomFunction_NullFunctionName()
        {
            // Test
            Action removeFunction = () =>
                CustomUriFunctions.RemoveCustomUriFunction(null);

            // Assert
            removeFunction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void RemoveCustomFunction_EmptyStringFunctionName()
        {
            // Test
            Action removeFunction = () =>
                CustomUriFunctions.RemoveCustomUriFunction(string.Empty);

            // Assert
            removeFunction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void RemoveCustomFunction_NullFunctionSignature()
        {
            // Test
            Action removeFunction = () =>
                CustomUriFunctions.RemoveCustomUriFunction("FunctionName", null);

            // Assert
            removeFunction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void RemoveCustomFunction_FunctionSignatureWithoutAReturnType()
        {
            FunctionSignatureWithReturnType existingCustomFunctionSignature =
                   new FunctionSignatureWithReturnType(null, EdmCoreModel.Instance.GetBoolean(false));

            // Test
            Action removeFunction = () =>
                CustomUriFunctions.RemoveCustomUriFunction("FunctionName", existingCustomFunctionSignature);

            // Assert
            removeFunction.ShouldThrow<ArgumentNullException>();
        }

        #endregion

        // Remove existing
        [Fact]
        public void RemoveCustomFunction_ShouldRemoveAnExistingFunction_ByName()
        {
            string customFunctionName = "my.ExistingCustomFunction";

            // Preaper
            FunctionSignatureWithReturnType existingCustomFunctionSignature =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
            CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

            GetCustomFunctionSignaturesOrNull(customFunctionName)[0].
                Equals(existingCustomFunctionSignature).
                Should().BeTrue();

            // Test
            bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);

            // Assert
            isRemoveSucceeded.Should().BeTrue();
            GetCustomFunctionSignaturesOrNull(customFunctionName).Should().BeNull();
        }


        // Remove not existing
        [Fact]
        public void RemoveCustomFunction_CannotRemoveFunctionWhichDoesntExist_ByName()
        {
            string customFunctionName = "my.ExistingCustomFunction";

            // Test
            bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);

            // Assert
            isRemoveSucceeded.Should().BeFalse();
        }

        // Remove signature, function name doesn't exist
        [Fact]
        public void RemoveCustomFunction_CannotRemoveFunctionWhichDoesntExist_ByNameAndSignature()
        {
            string customFunctionName = "my.ExistingCustomFunction";
            FunctionSignatureWithReturnType customFunctionSignature =
                 new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));

            // Test
            bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName, customFunctionSignature);

            // Assert
            isRemoveSucceeded.Should().BeFalse();
        }

        // Remove signature, function name exists, signature doesn't
        [Fact]
        public void RemoveCustomFunction_CannotRemoveFunctionWithSameNameAndDifferentSignature()
        {
            string customFunctionName = "my.ExistingCustomFunction";

            try
            {
                // Preaper
                FunctionSignatureWithReturnType existingCustomFunctionSignature =
                        new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                GetCustomFunctionSignaturesOrNull(customFunctionName)[0].Equals(existingCustomFunctionSignature).Should().BeTrue();

                // Preaper

                // Function with different siganture
                FunctionSignatureWithReturnType customFunctionSignatureToRemove =
                        new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetInt16(false), EdmCoreModel.Instance.GetBoolean(false));

                // Test

                // Try Remove a function with the same name but different siganture
                bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName, customFunctionSignatureToRemove);

                // Assert
                isRemoveSucceeded.Should().BeFalse();
            }
            finally
            {
                // Clean up cahce
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }

        }

        // Remove signature, function and signature exists
        [Fact]
        public void RemoveCustomFunction_RemoveFunctionWithSameNameAndSignature()
        {
            string customFunctionName = "my.ExistingCustomFunction";

            try
            {
                // Preaper
                FunctionSignatureWithReturnType existingCustomFunctionSignature =
                        new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                GetCustomFunctionSignaturesOrNull(customFunctionName)[0].Equals(existingCustomFunctionSignature).Should().BeTrue();

                // Test
                bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                // Assert
                isRemoveSucceeded.Should().BeTrue();

                GetCustomFunctionSignaturesOrNull(customFunctionName).Should().BeNull();
            }
            finally
            {
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        // Remove one overload
        [Fact]
        public void RemoveCustomFunction_RemoveFunctionWithSameNameAndSignature_OtherOverloadsExists()
        {
            string customFunctionName = "my.ExistingCustomFunction";

            try
            {
                // Preaper
                FunctionSignatureWithReturnType existingCustomFunctionSignature =
                        new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetDouble(false), EdmCoreModel.Instance.GetBoolean(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                FunctionSignatureWithReturnType existingCustomFunctionSignatureTwo =
                    new FunctionSignatureWithReturnType(EdmCoreModel.Instance.GetBoolean(false), EdmCoreModel.Instance.GetDate(false));
                CustomUriFunctions.AddCustomUriFunction(customFunctionName, existingCustomFunctionSignatureTwo);

                // Validate that the two overloads as 
                GetCustomFunctionSignaturesOrNull(customFunctionName).
                    All(funcSignature =>    funcSignature.Equals(existingCustomFunctionSignature) ||
                                            funcSignature.Equals(existingCustomFunctionSignatureTwo)).
                        Should().BeTrue();

                // Remove the first overload, second overload should not be removed
                bool isRemoveSucceeded = CustomUriFunctions.RemoveCustomUriFunction(customFunctionName, existingCustomFunctionSignature);

                // Assert
                isRemoveSucceeded.Should().BeTrue();

                FunctionSignatureWithReturnType[] overloads = GetCustomFunctionSignaturesOrNull(customFunctionName);
                overloads.Length.Should().Be(1);
                overloads[0].Should().Be(existingCustomFunctionSignatureTwo);
            }
            finally
            {
                // Clean up cache
                CustomUriFunctions.RemoveCustomUriFunction(customFunctionName);
            }
        }

        #endregion

        #region Private Methods

        private FunctionSignatureWithReturnType[] GetCustomFunctionSignaturesOrNull(string customFunctionName)
        {
            FunctionSignatureWithReturnType[] resultFunctionSignaturesWithReturnType = null;
            CustomUriFunctions.TryGetCustomFunction(customFunctionName, out resultFunctionSignaturesWithReturnType);

            return resultFunctionSignaturesWithReturnType as FunctionSignatureWithReturnType[];
        }

        #endregion
    }
}
