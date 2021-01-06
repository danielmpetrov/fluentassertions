﻿using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentAssertions.Data;
using FluentAssertions.Execution;

namespace FluentAssertions.Equivalency
{
    public class DataTableEquivalencyStep : IEquivalencyStep
    {
        public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            return typeof(DataTable).IsAssignableFrom(config.GetExpectationType(context.RuntimeType, context.CompileTimeType));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "The code is easier to read without it.")]
        public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
        {
            var subject = context.Subject as DataTable;
            var expectation = context.Expectation as DataTable;

            if (expectation == null)
            {
                if (subject != null)
                {
                    AssertionScope.Current.FailWith("Expected {context:DataTable} value to be null, but found {0}", subject);
                }
            }
            else
            {
                if (subject == null)
                {
                    if (context.Subject == null)
                    {
                        AssertionScope.Current.FailWith("Expected {context:DataTable} to be non-null, but found null");
                    }
                    else
                    {
                        AssertionScope.Current.FailWith("Expected {context:DataTable} to be of type {0}, but found {1} instead", expectation.GetType(), context.Subject.GetType());
                    }
                }
                else
                {
                    var dataSetConfig = config as DataEquivalencyAssertionOptions<DataSet>;
                    var dataTableConfig = config as DataEquivalencyAssertionOptions<DataTable>;

                    if (((dataSetConfig == null) || !dataSetConfig.AllowMismatchedTypes)
                     && ((dataTableConfig == null) || !dataTableConfig.AllowMismatchedTypes))
                    {
                        AssertionScope.Current
                            .ForCondition(subject.GetType() == expectation.GetType())
                            .FailWith("Expected {context:DataTable} to be of type '{0}'{reason}, but found '{1}'", expectation.GetType(), subject.GetType());
                    }

                    var selectedMembers = GetMembersFromExpectation(context, config)
                        .ToDictionary(member => member.Name);

                    CompareScalarProperties(subject, expectation, selectedMembers);

                    CompareCollections(context, parent, config, expectation, selectedMembers);
                }
            }

            return true;
        }

        private static void CompareScalarProperties(DataTable subject, DataTable expectation, Dictionary<string, IMember> selectedMembers)
        {
            // Note: The members here are listed in the XML documentation for the DataTable.BeEquivalentTo extension
            // method in DataTableAssertions.cs. If this ever needs to change, keep them in sync.
            if (selectedMembers.ContainsKey(nameof(expectation.TableName)))
            {
                AssertionScope.Current
                    .ForCondition(subject.TableName == expectation.TableName)
                    .FailWith("Expected {context:DataTable} to have TableName '{0}'{reason}, but found '{1}' instead", expectation.TableName, subject.TableName);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.CaseSensitive)))
            {
                AssertionScope.Current
                    .ForCondition(subject.CaseSensitive == expectation.CaseSensitive)
                    .FailWith("Expected {context:DataTable} to have CaseSensitive value of '{0}'{reason}, but found '{1}' instead", expectation.CaseSensitive, subject.CaseSensitive);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.DisplayExpression)))
            {
                AssertionScope.Current
                    .ForCondition(subject.DisplayExpression == expectation.DisplayExpression)
                    .FailWith("Expected {context:DataTable} to have DisplayExpression value of '{0}'{reason}, but found '{1}' instead", expectation.DisplayExpression, subject.DisplayExpression);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.HasErrors)))
            {
                AssertionScope.Current
                    .ForCondition(subject.HasErrors == expectation.HasErrors)
                    .FailWith("Expected {context:DataTable} to have HasErrors value of '{0}'{reason}, but found '{1}' instead", expectation.HasErrors, subject.HasErrors);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.Locale)))
            {
                AssertionScope.Current
                    .ForCondition(subject.Locale == expectation.Locale)
                    .FailWith("Expected {context:DataTable} to have Locale value of '{0}'{reason}, but found '{1}' instead", expectation.Locale, subject.Locale);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.Namespace)))
            {
                AssertionScope.Current
                    .ForCondition(subject.Namespace == expectation.Namespace)
                    .FailWith("Expected {context:DataTable} to have Namespace value of '{0}'{reason}, but found '{1}' instead", expectation.Namespace, subject.Namespace);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.Prefix)))
            {
                AssertionScope.Current
                    .ForCondition(subject.Prefix == expectation.Prefix)
                    .FailWith("Expected {context:DataTable} to have Prefix value of '{0}'{reason}, but found '{1}' instead", expectation.Prefix, subject.Prefix);
            }

            if (selectedMembers.ContainsKey(nameof(expectation.RemotingFormat)))
            {
                AssertionScope.Current
                    .ForCondition(subject.RemotingFormat == expectation.RemotingFormat)
                    .FailWith("Expected {context:DataTable} to have RemotingFormat value of '{0}'{reason}, but found '{1}' instead", expectation.RemotingFormat, subject.RemotingFormat);
            }
        }

        private static void CompareCollections(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config, DataTable expectation, Dictionary<string, IMember> selectedMembers)
        {
            // Note: The collections here are listed in the XML documentation for the DataTable.BeEquivalentTo extension
            // method in DataTableAssertions.cs. If this ever needs to change, keep them in sync.
            var collectionNames = new[]
            {
                nameof(expectation.ChildRelations),
                nameof(expectation.Columns),
                nameof(expectation.Constraints),
                nameof(expectation.ExtendedProperties),
                nameof(expectation.ParentRelations),
                nameof(expectation.PrimaryKey),
                nameof(expectation.Rows),
            };

            foreach (var collectionName in collectionNames)
            {
                if (selectedMembers.TryGetValue(collectionName, out var expectationMember))
                {
                    var matchingMember = FindMatchFor(expectationMember, context, config);

                    if (matchingMember != null)
                    {
                        IEquivalencyValidationContext nestedContext =
                                context.AsNestedMember(expectationMember, matchingMember);

                        if (nestedContext != null)
                        {
                            parent.AssertEqualityUsing(nestedContext);
                        }
                    }
                }
            }
        }

        private static IMember FindMatchFor(IMember selectedMemberInfo, IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            IEnumerable<IMember> query =
                from rule in config.MatchingRules
                let match = rule.Match(selectedMemberInfo, context.Subject, context.CurrentNode, config)
                where match != null
                select match;

            return query.FirstOrDefault();
        }

        private static IEnumerable<IMember> GetMembersFromExpectation(IEquivalencyValidationContext context,
            IEquivalencyAssertionOptions config)
        {
            IEnumerable<IMember> members = Enumerable.Empty<IMember>();

            foreach (IMemberSelectionRule rule in config.SelectionRules)
            {
                members = rule.SelectMembers(context.CurrentNode, members, new MemberSelectionContext
                {
                    CompileTimeType = context.CompileTimeType,
                    RuntimeType = context.RuntimeType,
                    Options = config
                });
            }

            return members;
        }
    }
}