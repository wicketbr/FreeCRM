namespace Try.Core
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Razor.Language;

    internal class CompileToCSharpResult
    {
        public RazorProjectItem? ProjectItem { get; set; }

        public string Code { get; set; } = String.Empty;

        public string FilePath { get; set; } = String.Empty;

        public IEnumerable<CompilationDiagnostic> Diagnostics { get; set; } = [];
    }
}
