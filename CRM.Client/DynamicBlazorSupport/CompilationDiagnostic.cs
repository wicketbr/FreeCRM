namespace Try.Core
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Razor.Language;
    using Microsoft.CodeAnalysis;

    public class CompilationDiagnostic
    {
        public string Code { get; set; } = String.Empty;

        public DiagnosticSeverity Severity { get; set; }

        public string Description { get; set; } = String.Empty;

        public int? Line { get; set; }

        public string File { get; set; } = String.Empty;

        public CompilationDiagnosticKind Kind { get; set; }

        internal static CompilationDiagnostic FromCSharpDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic != null) {
                var mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
                var file = Path.GetFileName(mappedLineSpan.Path);
                var line = mappedLineSpan.StartLinePosition.Line;

                if (file != CoreConstants.MainComponentFilePath) {
                    // Make it 1-based. Skip the main component where we add @page directive line
                    line++;
                } else {
                    // Offset for MudProviders
                    //line -= 4;
                }

                return new CompilationDiagnostic {
                    Kind = CompilationDiagnosticKind.CSharp,
                    Code = diagnostic.Descriptor.Id,
                    Severity = diagnostic.Severity,
                    Description = diagnostic.GetMessage(),
                    File = file,
                    Line = line,
                };
            } else {
                return new CompilationDiagnostic();
            }
        }

        internal static CompilationDiagnostic FromRazorDiagnostic(RazorDiagnostic diagnostic)
        {
            if (diagnostic != null) {
                return new CompilationDiagnostic {
                    Kind = CompilationDiagnosticKind.Razor,
                    Code = diagnostic.Id,
                    Severity = (DiagnosticSeverity)diagnostic.Severity,
                    Description = diagnostic.GetMessage(),
                    File = Path.GetFileName(diagnostic.Span.FilePath),

                    // Line = diagnostic.Span.LineIndex, // TODO: Find a way to calculate this
                };
            } else {
                return new CompilationDiagnostic();
            }
        }
    }
}
