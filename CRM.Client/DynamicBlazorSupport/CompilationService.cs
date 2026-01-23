namespace Try.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Routing;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.AspNetCore.Razor.Language;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Razor;
    using Microsoft.JSInterop;

    //class SimpleRazorConfiguration : RazorConfiguration
    //{
    //    string _ConfigurationName = "";
    //    public override string ConfigurationName => _ConfigurationName;
    //    IReadOnlyList<RazorExtension> _Extensions;
    //    public override IReadOnlyList<RazorExtension> Extensions => _Extensions;
    //    RazorLanguageVersion _LanguageVersion;
    //    public override RazorLanguageVersion LanguageVersion => _LanguageVersion;
    //    bool _UseConsolidatedMvcViews = false;
    //    public override bool UseConsolidatedMvcViews => _UseConsolidatedMvcViews;
    //    public SimpleRazorConfiguration(RazorLanguageVersion razorLanguageVersion, string configuration, IReadOnlyList<RazorExtension> extensions, bool useConsolidatedMvcViews = false)
    //    {
    //        _LanguageVersion = razorLanguageVersion;
    //        _ConfigurationName = configuration;
    //        _Extensions = extensions;
    //        _UseConsolidatedMvcViews = useConsolidatedMvcViews;
    //    }
    //}

    public class CompilationService
    {
        private Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

        //public const string DefaultRootNamespace = $"{nameof(Try)}.{nameof(UserComponents)}";
        public const string DefaultRootNamespace = "CRM";

        //private const string WorkingDirectory = "/TryMudBlazor/";
        private const string WorkingDirectory = "/CRM/";
        private static readonly string[] DefaultImports =
        [
            "@using System.ComponentModel.DataAnnotations",
            "@using System.Linq",
            "@using System.Net.Http",
            "@using System.Net.Http.Json",
            "@using Microsoft.AspNetCore.Components.Forms",
            "@using Microsoft.AspNetCore.Components.Routing",
            "@using Microsoft.AspNetCore.Components.Web",
            "@using Microsoft.JSInterop",
            "@using MudBlazor",
        ];

        //        private const string MudBlazorServices = @"
        //<MudDialogProvider FullWidth=""true"" MaxWidth=""MaxWidth.ExtraSmall"" />
        //<MudSnackbarProvider/>

        //";

        // Instead of the mudblazor stuff just add empty lines.
        private const string MudBlazorServices = @"



";

        // Creating the initial compilation + reading references is on the order of 250ms without caching
        // so making sure it doesn't happen for each run.
        private static CSharpCompilation _baseCompilation = null!;
        private static CSharpParseOptions _cSharpParseOptions = null!;

        private readonly RazorProjectFileSystem fileSystem = new VirtualRazorProjectFileSystem();
        private readonly RazorConfiguration configuration = new RazorConfiguration(
            RazorLanguageVersion.Latest,
            ConfigurationName: "Blazor",
            Extensions: ImmutableArray<RazorExtension>.Empty);

        //private readonly RazorConfiguration configuration = new SimpleRazorConfiguration(
        //    RazorLanguageVersion.Latest,
        //    "Blazor",
        //    ImmutableArray<RazorExtension>.Empty
        //);

        MetadataReferenceService.BlazorWasm.BlazorWasmMetadataReferenceService BlazorWasmMetadataReferenceService;

        public CompilationService(MetadataReferenceService.BlazorWasm.BlazorWasmMetadataReferenceService blazorWasmMetadataReferenceService)
        {
            BlazorWasmMetadataReferenceService = blazorWasmMetadataReferenceService;
        }

        Task? _Init = null;
        Task Init => _Init ??= InitAsync();

        //public async Task InitAsync(Func<IReadOnlyCollection<string>, ValueTask<IReadOnlyList<byte[]>>> getReferencedDllsBytesFunc)
        //{
        //    var basicReferenceAssemblyRoots = new[]
        //    {
        //        typeof(Console).Assembly, // System.Console
        //        typeof(Uri).Assembly, // System.Private.Uri
        //        typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Private.CoreLib
        //        typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
        //        typeof(IQueryable).Assembly, // System.Linq.Expressions
        //        typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
        //        typeof(HttpClient).Assembly, // System.Net.Http
        //        typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
        //        typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
        //        typeof(MudBlazor.MudButton).Assembly, // MudBlazor
        //        typeof(WebAssemblyHostBuilder).Assembly, // Microsoft.AspNetCore.Components.WebAssembly
        //        typeof(FluentValidation.AbstractValidator<>).Assembly,
        //    };
        //    var assemblyNames = await getReferencedDllsBytesFunc(basicReferenceAssemblyRoots
        //        .SelectMany(assembly => assembly.GetReferencedAssemblies().Concat(
        //        [
        //            assembly.GetName()
        //        ]))
        //        .Select(assemblyName => assemblyName.Name)
        //        .ToHashSet());

        //    var basicReferenceAssemblies = assemblyNames
        //        .Select(peImage => MetadataReference.CreateFromImage(peImage, MetadataReferenceProperties.Assembly))
        //        .ToList();

        //    _baseCompilation = CSharpCompilation.Create(
        //        DefaultRootNamespace,
        //        Array.Empty<SyntaxTree>(),
        //        basicReferenceAssemblies,
        //        new CSharpCompilationOptions(
        //            OutputKind.DynamicallyLinkedLibrary,
        //            optimizationLevel: OptimizationLevel.Release,
        //            concurrentBuild: false,
        //            //// Warnings CS1701 and CS1702 are disabled when compiling in VS too
        //            specificDiagnosticOptions: new[]
        //            {
        //                new KeyValuePair<string, ReportDiagnostic>("CS1701", ReportDiagnostic.Suppress),
        //                new KeyValuePair<string, ReportDiagnostic>("CS1702", ReportDiagnostic.Suppress),
        //            }));

        //    _cSharpParseOptions = new CSharpParseOptions(LanguageVersion.Preview);

        //    _Init = Task.CompletedTask;
        //}

        async Task InitAsync()
        {
            var basicReferenceAssemblyRoots = new[]
            {
                typeof(Console).Assembly, // System.Console
                typeof(Uri).Assembly, // System.Private.Uri
                typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Private.CoreLib
                typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
                typeof(IQueryable).Assembly, // System.Linq.Expressions
                typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
                typeof(HttpClient).Assembly, // System.Net.Http
                typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
                typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
                typeof(MudBlazor.MudButton).Assembly, // MudBlazor
                typeof(WebAssemblyHostBuilder).Assembly, // Microsoft.AspNetCore.Components.WebAssembly
                typeof(FluentValidation.AbstractValidator<>).Assembly,
            };

            var referenceTasks = new List<Task<MetadataReference>>();

            //var assemblyNames = await getReferencedDllsBytesFunc(basicReferenceAssemblyRoots
            //    .SelectMany(assembly => assembly.GetReferencedAssemblies().Concat(
            //    [
            //        assembly.GetName()
            //    ]))
            //    .Select(assemblyName => assemblyName.Name)
            //    .ToHashSet());

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            //var basicReferenceAssemblies = assemblyNames
            //    .Select(peImage => MetadataReference.CreateFromImage(peImage, MetadataReferenceProperties.Assembly))
            //    .ToList();

            foreach (var assembly in assemblies) {
                if (assembly.IsDynamic) {
                    continue;
                }
                //Console.WriteLine($"Adding reference to assembly: {assembly.FullName}");

                referenceTasks.Add(BlazorWasmMetadataReferenceService.CreateAsync(MetadataReferenceService.Abstractions.Types.AssemblyDetails.FromAssembly(assembly)));
            }
            await Task.WhenAll(referenceTasks);
            var references = referenceTasks.Select(o => o.Result).ToList();

            _baseCompilation = CSharpCompilation.Create(
                DefaultRootNamespace,
                Array.Empty<SyntaxTree>(),
                references, //basicReferenceAssemblies,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    concurrentBuild: false,
                    //// Warnings CS1701 and CS1702 are disabled when compiling in VS too
                    specificDiagnosticOptions: new[]
                    {
                        new KeyValuePair<string, ReportDiagnostic>("CS1701", ReportDiagnostic.Suppress),
                        new KeyValuePair<string, ReportDiagnostic>("CS1702", ReportDiagnostic.Suppress),
                    }));

            _cSharpParseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        }

        int _i = 0;

        /// <summary>
        /// Compile razor code to a Blazor Component Type
        /// </summary>
        /// <param name="razorCode"></param>
        /// <param name="updateStatusFunc"></param>
        /// <returns></returns>
        public async Task<Type?> CompileRazorCodeToBlazorComponentType(string razorCode, Func<string, Task>? updateStatusFunc = null)
        {
            var hash = MD5.ComputeHashString(System.Text.Encoding.UTF8.GetBytes(razorCode));
            if (_cachedTypes.ContainsKey(hash)) {
                var cachedOutput = _cachedTypes[hash];
                if (cachedOutput != null) {
                    return cachedOutput;
                }
            }

            var compileResult = await CompileAsync(new CodeFile($"/App/MyApp{++_i}.razor", razorCode), updateStatusFunc);
            var assembly = compileResult.LoadAssembly();
            var component = assembly?.ExportedTypes.FirstOrDefault(o => o.IsSubclassOf(typeof(ComponentBase)));

            if (!_cachedTypes.ContainsKey(hash) && component != null) {
                _cachedTypes[hash] = component;
            }

            return component;
        }

        public async Task<Assembly?> CompileAsync(string csCode, Func<string, Task>? updateStatusFunc = null)
        {
            var compileResult = await CompileAsync(new CodeFile($"/App/MyApp{++_i}.cs", csCode), updateStatusFunc);
            var assembly = compileResult.LoadAssembly();
            return assembly;
        }

        public Task<CompileToAssemblyResult> CompileAsync(CodeFile codeFile, Func<string, Task>? updateStatusFunc = null)
        {
            return CompileAsync(new[] { codeFile }, updateStatusFunc);
        }

        public async Task<CompileToAssemblyResult> CompileAsync(ICollection<CodeFile> codeFiles, Func<string, Task>? updateStatusFunc = null)
        {
            ArgumentNullException.ThrowIfNull(codeFiles);
            if (!Init.IsCompleted) {
                await (updateStatusFunc?.Invoke("Initializing...") ?? Task.CompletedTask);
                await Init;
            }
            await (updateStatusFunc?.Invoke("Compiling...") ?? Task.CompletedTask);
            var cSharpResults = await CompileToCSharpAsync(codeFiles, updateStatusFunc);
            var result = CompileToAssembly(cSharpResults);
            await (updateStatusFunc?.Invoke(result.Compiled ? "Compile Success" : "Compile Failed") ?? Task.CompletedTask);
            return result;
        }

        public async Task<CompileToAssemblyResult> CompileToAssemblyAsync(
            ICollection<CodeFile> codeFiles,
            Func<string, Task> updateStatusFunc) // TODO: try convert to event
        {
            ArgumentNullException.ThrowIfNull(codeFiles);

            var cSharpResults = await this.CompileToCSharpAsync(codeFiles, updateStatusFunc);

            await (updateStatusFunc?.Invoke("Compiling Assembly") ?? Task.CompletedTask);
            var result = CompileToAssembly(cSharpResults);

            return result;
        }

        private static CompileToAssemblyResult CompileToAssembly(IReadOnlyList<CompileToCSharpResult> cSharpResults)
        {
            if (cSharpResults.Any(r => r.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))) {
                return new CompileToAssemblyResult { Diagnostics = cSharpResults.SelectMany(r => r.Diagnostics).ToList() };
            }

            var syntaxTrees = new SyntaxTree[cSharpResults.Count];
            for (var i = 0; i < cSharpResults.Count; i++) {
                var cSharpResult = cSharpResults[i];
                syntaxTrees[i] = CSharpSyntaxTree.ParseText(cSharpResult.Code, _cSharpParseOptions, cSharpResult.FilePath);
            }

            var finalCompilation = _baseCompilation.AddSyntaxTrees(syntaxTrees);

            var compilationDiagnostics = finalCompilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Info);

            var result = new CompileToAssemblyResult {
                Compilation = finalCompilation,
                Diagnostics = compilationDiagnostics
                    .Select(CompilationDiagnostic.FromCSharpDiagnostic)
                    .Concat(cSharpResults.SelectMany(r => r.Diagnostics))
                    .ToList(),
            };

            if (result.Diagnostics.All(x => x.Severity != DiagnosticSeverity.Error)) {
                using var peStream = new MemoryStream();
                finalCompilation.Emit(peStream);

                result.AssemblyBytes = peStream.ToArray();
            }

            return result;
        }

        private static VirtualProjectItem CreateRazorProjectItem(string fileName, string fileContent)
        {
            var fullPath = WorkingDirectory + fileName;
            fullPath = fullPath.Replace("//", "/");

            // File paths in Razor are always of the form '/a/b/c.razor'
            var filePath = fileName;
            if (!filePath.StartsWith('/'))
            {
                filePath = '/' + filePath;
            }
            filePath = filePath.Replace("//", "/");

            fileContent = fileContent.Replace("\r", string.Empty);

            return new VirtualProjectItem(
                WorkingDirectory,
                filePath,
                fullPath,
                fileName,
                FileKinds.Component,
                Encoding.UTF8.GetBytes(fileContent.TrimStart()));
        }

        private async Task<IReadOnlyList<CompileToCSharpResult>> CompileToCSharpAsync(
            ICollection<CodeFile> codeFiles,
            Func<string, Task>? updateStatusFunc)
        {
            // The first phase won't include any metadata references for component discovery. This mirrors what the build does.
            var projectEngine = this.CreateRazorProjectEngine(Array.Empty<MetadataReference>());

            // Result of generating declarations
            var declarations = new CompileToCSharpResult[codeFiles.Count];
            var index = 0;
            foreach (var codeFile in codeFiles) {
                if (codeFile.Type == CodeFileType.Razor) {
                    var fileContent = index == 0 ? MudBlazorServices : string.Empty;
                    //var fileContent = String.Empty;
                    fileContent += codeFile.Content;
                    var projectItem = CreateRazorProjectItem(codeFile.Path, fileContent);

                    var codeDocument = projectEngine.ProcessDeclarationOnly(projectItem);
                    var cSharpDocument = codeDocument.GetCSharpDocument();

                    //Console.WriteLine("Generated Code: " + cSharpDocument.GeneratedCode);

                    declarations[index] = new CompileToCSharpResult
                    {
                        FilePath = codeFile.Path,
                        ProjectItem = projectItem,
                        Code = cSharpDocument.GeneratedCode,
                        Diagnostics = cSharpDocument.Diagnostics.Select(CompilationDiagnostic.FromRazorDiagnostic).ToList(),
                    };
                } else {
                    declarations[index] = new CompileToCSharpResult
                    {
                        FilePath = codeFile.Path,
                        Code = codeFile.Content,
                        Diagnostics = Enumerable.Empty<CompilationDiagnostic>(), // Will actually be evaluated later
                    };
                }

                index++;
            }

            // Result of doing 'temp' compilation
            var tempAssembly = CompileToAssembly(declarations);
            if (tempAssembly.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) {
                return [new CompileToCSharpResult { Diagnostics = tempAssembly.Diagnostics }];
            }

            if (tempAssembly.Compilation == null) {
                return [new CompileToCSharpResult()];
            }

            // Add the 'temp' compilation as a metadata reference
            var references = new List<MetadataReference>(_baseCompilation.References) { tempAssembly.Compilation.ToMetadataReference() };

            projectEngine = CreateRazorProjectEngine(references);

            await (updateStatusFunc?.Invoke("Preparing Project") ?? Task.CompletedTask);

            var results = new CompileToCSharpResult[declarations.Length];
            for (index = 0; index < declarations.Length; index++)
            {
                var declaration = declarations[index];
                var isRazorDeclaration = declaration.ProjectItem != null;

                if (isRazorDeclaration) {
                    if (declaration.ProjectItem != null) {
                        var codeDocument = projectEngine.Process(declaration.ProjectItem);
                        var cSharpDocument = codeDocument.GetCSharpDocument();

                        //Console.WriteLine("cSharpDocument_" + index.ToString() + ": " + cSharpDocument.GeneratedCode);

                        results[index] = new CompileToCSharpResult {
                            FilePath = declaration.FilePath,
                            ProjectItem = declaration.ProjectItem,
                            Code = cSharpDocument.GeneratedCode,
                            Diagnostics = cSharpDocument.Diagnostics.Select(CompilationDiagnostic.FromRazorDiagnostic).ToList(),
                        };
                    }
                }
                else
                {
                    results[index] = declaration;
                }
            }

            return results;
        }

        private RazorProjectEngine CreateRazorProjectEngine(IReadOnlyList<MetadataReference> references) =>
            RazorProjectEngine.Create(configuration, fileSystem, builder =>
            {
                builder.SetRootNamespace(DefaultRootNamespace);
                builder.AddDefaultImports(DefaultImports);

                // Features that use Roslyn are mandatory for components
                CompilerFeatures.Register(builder);

                builder.Features.Add(new CompilationTagHelperFeature());
                builder.Features.Add(new DefaultMetadataReferenceFeature { References = references });
            });
    }

    public static class MD5
    {
        public static byte[] ComputeHash(byte[] input)
        {
            uint num = 1732584193u;
            uint num2 = 4023233417u;
            uint num3 = 2562383102u;
            uint num4 = 271733878u;
            int num5 = (56 - (input.Length + 1) % 64) % 64;
            byte[] array = new byte[input.Length + 1 + num5 + 8];
            Array.Copy(input, array, input.Length);
            array[input.Length] = 128;
            Array.Copy(BitConverter.GetBytes(input.Length * 8), 0, array, array.Length - 8, 4);
            for (int i = 0; i < array.Length / 64; i++) {
                uint[] array2 = new uint[16];
                for (int j = 0; j < 16; j++) {
                    array2[j] = BitConverter.ToUInt32(array, i * 64 + j * 4);
                }

                uint num6 = num;
                uint num7 = num2;
                uint num8 = num3;
                uint num9 = num4;
                uint num10 = 0u;
                uint num11 = 0u;
                uint num12 = 0u;
                while (true) {
                    switch (num12) {
                        case 0u:
                        case 1u:
                        case 2u:
                        case 3u:
                        case 4u:
                        case 5u:
                        case 6u:
                        case 7u:
                        case 8u:
                        case 9u:
                        case 10u:
                        case 11u:
                        case 12u:
                        case 13u:
                        case 14u:
                        case 15u:
                            num10 = num7 & num8 | ~num7 & num9;
                            num11 = num12;
                            goto IL_0138;
                        case 16u:
                        case 17u:
                        case 18u:
                        case 19u:
                        case 20u:
                        case 21u:
                        case 22u:
                        case 23u:
                        case 24u:
                        case 25u:
                        case 26u:
                        case 27u:
                        case 28u:
                        case 29u:
                        case 30u:
                        case 31u:
                        case 32u:
                        case 33u:
                        case 34u:
                        case 35u:
                        case 36u:
                        case 37u:
                        case 38u:
                        case 39u:
                        case 40u:
                        case 41u:
                        case 42u:
                        case 43u:
                        case 44u:
                        case 45u:
                        case 46u:
                        case 47u:
                        case 48u:
                        case 49u:
                        case 50u:
                        case 51u:
                        case 52u:
                        case 53u:
                        case 54u:
                        case 55u:
                        case 56u:
                        case 57u:
                        case 58u:
                        case 59u:
                        case 60u:
                        case 61u:
                        case 62u:
                        case 63u:
                            if (num12 >= 16 && num12 <= 31) {
                                num10 = num9 & num7 | ~num9 & num8;
                                num11 = (5 * num12 + 1) % 16u;
                            } else if (num12 >= 32 && num12 <= 47) {
                                num10 = num7 ^ num8 ^ num9;
                                num11 = (3 * num12 + 5) % 16u;
                            } else if (num12 >= 48) {
                                num10 = num8 ^ (num7 | ~num9);
                                num11 = 7 * num12 % 16u;
                            }

                            goto IL_0138;
                    }

                    break;
                IL_0138:
                    uint num13 = num9;
                    num9 = num8;
                    num8 = num7;
                    num7 += leftRotate(num6 + num10 + K[num12] + array2[num11], s[num12]);
                    num6 = num13;
                    num12++;
                }

                num += num6;
                num2 += num7;
                num3 += num8;
                num4 += num9;
            }
            var hashBytes = new byte[16];
            BitConverter.GetBytes(num).CopyTo(hashBytes, 0);
            BitConverter.GetBytes(num2).CopyTo(hashBytes, 4);
            BitConverter.GetBytes(num3).CopyTo(hashBytes, 8);
            BitConverter.GetBytes(num4).CopyTo(hashBytes, 12);
            return hashBytes;
        }
        public static string ComputeHashString(byte[] input) => string.Join("", ComputeHash(input).Select(o => o.ToString("x2")));

        private static int[] s = new int[64]
        {
        7, 12, 17, 22, 7, 12, 17, 22, 7, 12,
        17, 22, 7, 12, 17, 22, 5, 9, 14, 20,
        5, 9, 14, 20, 5, 9, 14, 20, 5, 9,
        14, 20, 4, 11, 16, 23, 4, 11, 16, 23,
        4, 11, 16, 23, 4, 11, 16, 23, 6, 10,
        15, 21, 6, 10, 15, 21, 6, 10, 15, 21,
        6, 10, 15, 21
        };

        private static uint[] K = new uint[64]
        {
        3614090360u, 3905402710u, 606105819u, 3250441966u, 4118548399u, 1200080426u, 2821735955u, 4249261313u, 1770035416u, 2336552879u,
        4294925233u, 2304563134u, 1804603682u, 4254626195u, 2792965006u, 1236535329u, 4129170786u, 3225465664u, 643717713u, 3921069994u,
        3593408605u, 38016083u, 3634488961u, 3889429448u, 568446438u, 3275163606u, 4107603335u, 1163531501u, 2850285829u, 4243563512u,
        1735328473u, 2368359562u, 4294588738u, 2272392833u, 1839030562u, 4259657740u, 2763975236u, 1272893353u, 4139469664u, 3200236656u,
        681279174u, 3936430074u, 3572445317u, 76029189u, 3654602809u, 3873151461u, 530742520u, 3299628645u, 4096336452u, 1126891415u,
        2878612391u, 4237533241u, 1700485571u, 2399980690u, 4293915773u, 2240044497u, 1873313359u, 4264355552u, 2734768916u, 1309151649u,
        4149444226u, 3174756917u, 718787259u, 3951481745u
        };

        private static uint leftRotate(uint x, int c)
        {
            return x << c | x >> 32 - c;
        }
    }
}
