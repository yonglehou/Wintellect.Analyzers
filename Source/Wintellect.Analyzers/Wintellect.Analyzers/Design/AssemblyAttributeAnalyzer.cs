﻿/*------------------------------------------------------------------------------
Wintellect.Analyzers - .NET Compiler Platform ("Roslyn") Analyzers and CodeFixes
Copyright (c) Wintellect. All rights reserved
Licensed under the MIT license
------------------------------------------------------------------------------*/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Wintellect.Analyzers
{
    // This rule should support Visual Basic, but I can't get any of the test code
    // working in VS 2015 CTP5 for VB.NET. I'll come back to this on the next CTP.
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    //[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public class AssemblyAttributeAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor companyRule = new DiagnosticDescriptor(DiagnosticIds.AssembliesHaveCompanyAttributeAnalyzer,
                                                                                   Resources.AssembliesHaveCompanyAttributeAnalyzerTitle,
                                                                                   Resources.AssembliesHaveCompanyAttributeAnalyzerMessageFormat,
                                                                                   Resources.CategoryDesign,
                                                                                   DiagnosticSeverity.Warning,
                                                                                   true);

        private static DiagnosticDescriptor copyrightRule = new DiagnosticDescriptor(DiagnosticIds.AssembliesHaveCopyrightAttributeAnalyzer,
                                                                                     Resources.AssembliesHaveCopyrightAttributeAnalyzerTitle,
                                                                                     Resources.AssembliesHaveCopyrightAttributeAnalyzerMessageFormat,
                                                                                     Resources.CategoryDesign,
                                                                                     DiagnosticSeverity.Warning,
                                                                                     true);

        private static DiagnosticDescriptor descriptionRule = new DiagnosticDescriptor(DiagnosticIds.AssembliesHaveDescriptionAttributeAnalyzer,
                                                                                       Resources.AssembliesHaveDescriptionAttributeAnalyzerTitle,
                                                                                       Resources.AssembliesHaveDescriptionAttributeAnalyzerMessageFormat,
                                                                                       Resources.CategoryDesign,
                                                                                       DiagnosticSeverity.Warning,
                                                                                       true);

        private static DiagnosticDescriptor titleRule = new DiagnosticDescriptor(DiagnosticIds.AssembliesHaveTitleAttributeAnalyzer,
                                                                                 Resources.AssembliesHaveTitleAttributeAnalyzerTitle,
                                                                                 Resources.AssembliesHaveTitleAttributeAnalyzerMessageFormat,
                                                                                 Resources.CategoryDesign,
                                                                                 DiagnosticSeverity.Warning,
                                                                                 true);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(companyRule, copyrightRule, descriptionRule, titleRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationEndAction(AnalyzeCompilation);
        }

        private void AnalyzeCompilation(CompilationEndAnalysisContext context)
        {
            // Get the particular attributes I need to look for.
            var companyAttributeSymbol = KnownTypes.CompanyAttribute(context.Compilation);
            var copyrightAttributeSymbol = KnownTypes.CopyrightAttribute(context.Compilation);
            var descriptionAttributeSymbol = KnownTypes.DescriptionAttribute(context.Compilation);
            var titleAttributeSymbol = KnownTypes.TitleAttribute(context.Compilation);

            // Assume they are all not found.
            bool companyAttributeGood = false;
            bool copyrightAttributeGood = false;
            bool descriptionAttributeGood = false;
            bool titleAttributeGood = false;

            // Pound through each attribute in the assembly checking that the specific ones
            // are present and the parameters are not empty.
            foreach (var attribute in context.Compilation.Assembly.GetAttributes())
            {
                if ((companyAttributeSymbol != null) && (attribute.AttributeClass.Equals(companyAttributeSymbol)))
                {
                    companyAttributeGood = CheckAttributeParameter(attribute);
                    continue;
                }

                if ((copyrightAttributeSymbol != null) && (attribute.AttributeClass.Equals(copyrightAttributeSymbol)))
                {
                    copyrightAttributeGood = CheckAttributeParameter(attribute);
                    continue;
                }

                if ((descriptionAttributeSymbol != null) && (attribute.AttributeClass.Equals(descriptionAttributeSymbol)))
                {
                    descriptionAttributeGood = CheckAttributeParameter(attribute);
                    continue;
                }

                if ((titleAttributeSymbol != null) && (attribute.AttributeClass.Equals(titleAttributeSymbol)))
                {
                    titleAttributeGood = CheckAttributeParameter(attribute);
                    continue;
                }
            }

            // If any of the assembly wide attributes are missing or empty, trigger a warning.
            if (!companyAttributeGood)
            {
                context.ReportDiagnostic(Diagnostic.Create(companyRule, Location.None));
            }

            if (!copyrightAttributeGood)
            {
                context.ReportDiagnostic(Diagnostic.Create(copyrightRule, Location.None));
            }

            if (!descriptionAttributeGood)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptionRule, Location.None));
            }

            if (!titleAttributeGood)
            {
                context.ReportDiagnostic(Diagnostic.Create(titleRule, Location.None));
            }
        }

        private static bool CheckAttributeParameter(AttributeData attribute)
        {
            if (attribute.ConstructorArguments.Length == 1)
            {
                String param = attribute.ConstructorArguments[0].Value.ToString();
                if (!String.IsNullOrEmpty(param))
                {
                    return true;
                }
            }

            return false;
        }

        private static class KnownTypes
        {
            public static INamedTypeSymbol CompanyAttribute(Compilation compilation)
            {
                return compilation.GetTypeByMetadataName("System.Reflection.AssemblyCompanyAttribute");
            }

            public static INamedTypeSymbol CopyrightAttribute(Compilation compilation)
            {
                return compilation.GetTypeByMetadataName("System.Reflection.AssemblyCopyrightAttribute");
            }
            public static INamedTypeSymbol DescriptionAttribute(Compilation compilation)
            {
                return compilation.GetTypeByMetadataName("System.Reflection.AssemblyDescriptionAttribute");
            }
            public static INamedTypeSymbol TitleAttribute(Compilation compilation)
            {
                return compilation.GetTypeByMetadataName("System.Reflection.AssemblyTitleAttribute");
            }
        }
    }
}