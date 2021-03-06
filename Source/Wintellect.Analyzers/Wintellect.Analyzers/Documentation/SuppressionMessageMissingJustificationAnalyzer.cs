﻿/*------------------------------------------------------------------------------
Wintellect.Analyzers - .NET Compiler Platform ("Roslyn") Analyzers and CodeFixes
Copyright (c) Wintellect. All rights reserved
Licensed under the Apache License, Version 2.0
See License.txt in the project root for license information
------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml;

namespace Wintellect.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SuppressionMessageMissingJustificationAnalyzer : DiagnosticAnalyzer
    {
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.SuppressionMessageMissingJustificationAnalyzer,
                                                                             Resources.SuppressionMessageMissingJustificationAnalyzerTitle,
                                                                             Resources.SuppressionMessageMissingJustificationAnalyzerMessageFormat,
                                                                             Resources.CategoryDocumentation,
                                                                             DiagnosticSeverity.Warning,
                                                                             isEnabledByDefault: true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // Request to be called back on all the symbols that can have a SuppressionMessageAttribute applied to them.
            context.RegisterSymbolAction(AnalyzeSuppressMessage, 
                                         SymbolKind.Method, 
                                         SymbolKind.Field, 
                                         SymbolKind.Property,
                                         SymbolKind.NamedType,
                                         SymbolKind.NetModule);
        }

        private void AnalyzeSuppressMessage(SymbolAnalysisContext context)
        {
            // Are we looking at generated code?
            if (!context.Symbol.IsGeneratedOrNonUserCode())
            {
                // Look at the attributes for SuppressMessage.
                var attributes = context.Symbol.GetAttributes();
                for (int i = 0; i < attributes.Count(); i++)
                {
                    if (attributes[i].AttributeClass.Name.Equals("SuppressMessageAttribute"))
                    {
                        Boolean hasJustification = false;

                        // Look for the named parameters for Justification and if it doesn't exist, 
                        // or is empty, report the error.
                        var namedParams = attributes[i].NamedArguments;
                        for (int j = 0; j < namedParams.Count(); j++)
                        {
                            if (namedParams[j].Key.Equals("Justification"))
                            {
                                if (String.IsNullOrEmpty(namedParams[j].Value.Value.ToString()))
                                {
                                    var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                                    context.ReportDiagnostic(diagnostic);
                                }
                                hasJustification = true;
                            }
                        }

                        if (!hasJustification)
                        {
                            var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
