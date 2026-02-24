using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using PocketBase.Blazor.Models;
using PocketBase.Blazor.Options;

namespace PocketBase.Blazor.Clients.Crons
{
    /// <inheritdoc />
    public sealed class CronGenerator : ICronGenerator
    {
    /// <inheritdoc />
        public async Task GenerateAsync(
            CronManifest cronManifest,
            CronGenerationOptions options,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(cronManifest);
            ArgumentNullException.ThrowIfNull(options);

            if (options.CleanBeforeGenerate && Directory.Exists(options.ProjectDirectory))
            {
                Directory.Delete(options.ProjectDirectory, recursive: true);
            }

            await GenerateServerProjectFilesAsync(options, cancellationToken);

            string outputDir = Path.Combine(options.ProjectDirectory, options.OutputDirectory);
            await GenerateHandlersAsync(cronManifest, outputDir, options, cancellationToken);

            if (options.BuildBinary)
            {
                await BuildGoBinaryAsync(options, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task GenerateServerProjectFilesAsync(
            CronGenerationOptions options,
            CancellationToken cancellationToken)
        {
            string moduleName = options.ModuleName;
            string outputDir = Path.Combine(options.ProjectDirectory, options.OutputDirectory);

            // Create the necessary directories
            Directory.CreateDirectory(options.ProjectDirectory);
            Directory.CreateDirectory(outputDir);

            // Generate the go.mod file
            string goModContent = $"""
                module {moduleName}

                go 1.25.6

                require github.com/pocketbase/pocketbase v0.36.1
                """;
            string goModFilePath = Path.Combine(options.ProjectDirectory, "go.mod");
            await File.WriteAllTextAsync(goModFilePath, goModContent, cancellationToken);

            // Generate the main.go file
            string mainGoContent = $$"""
                package main

                import (
                    "log"
                    "net/http"

                    "{{moduleName}}/{{options.OutputDirectory}}"

                    "github.com/pocketbase/pocketbase"
                    "github.com/pocketbase/pocketbase/core"
                )

                func main() {
                    app := pocketbase.New()

                    app.OnServe().BindFunc(func(e *core.ServeEvent) error {
                        e.Router.POST("/internal/cron", func(re *core.RequestEvent) error {
                            var req cron.CronRequest

                            if err := re.BindBody(&req); err != nil {
                                return re.JSON(http.StatusBadRequest, err.Error())
                            }

                            if err := cron.RegisterCron(app, req); err != nil {
                                return re.JSON(http.StatusBadRequest, err.Error())
                            }

                            return re.JSON(http.StatusOK, map[string]any{
                                "status": "cron registered",
                                "id":     req.ID,
                            })
                        })

                        return e.Next()
                    })

                    if err := app.Start(); err != nil {
                        log.Fatal(err)
                    }
                }
                """;
            string mainGoFilePath = Path.Combine(options.ProjectDirectory, "main.go");
            await File.WriteAllTextAsync(mainGoFilePath, mainGoContent, cancellationToken);

            // Generate the cron/types.go file
            string cronTypesContent = $$"""
                package {{options.OutputDirectory}}

                type CronRequest struct {
                    ID         string         `json:"id"`
                    Expression string         `json:"expression"`
                    Payload    map[string]any `json:"payload"`
                }
                """;
            string cronTypesFilePath = Path.Combine(outputDir, "types.go");
            await File.WriteAllTextAsync(cronTypesFilePath, cronTypesContent, cancellationToken);

            // Generate the cron/registry.go file
            string cronRegistryContent = $$"""
                package {{options.OutputDirectory}}

                import "sync"

                type CronHandler func(payload map[string]any)

                var (
                    cronHandlers = map[string]CronHandler{}
                    cronPayloads = map[string]map[string]any{}
                    mu           sync.Mutex
                )

                func RegisterHandler(id string, handler CronHandler) {
                    cronHandlers[id] = handler
                }
                """;
            string cronRegistryFilePath = Path.Combine(outputDir, "registry.go");
            await File.WriteAllTextAsync(cronRegistryFilePath, cronRegistryContent, cancellationToken);

            // Generate the cron/runtime.go file
            string cronRuntimeContent = $$"""
                package {{options.OutputDirectory}}

                import (
                    "errors"

                    "github.com/pocketbase/pocketbase"
                )

                func RegisterCron(app *pocketbase.PocketBase, req CronRequest) error {
                    handler, ok := cronHandlers[req.ID]
                    if !ok {
                        return errors.New("unknown cron id: " + req.ID)
                    }

                    mu.Lock()
                    cronPayloads[req.ID] = req.Payload
                    mu.Unlock()

                    app.Cron().Remove(req.ID)

                    app.Cron().Add(req.ID, req.Expression, func() {
                        mu.Lock()
                        payload := cronPayloads[req.ID]
                        mu.Unlock()

                        handler(payload)
                    })

                    return nil
                }
                """;
            string cronRuntimeFilePath = Path.Combine(outputDir, "runtime.go");
            await File.WriteAllTextAsync(cronRuntimeFilePath, cronRuntimeContent, cancellationToken);
        }

        /// <inheritdoc />
        public async Task GenerateHandlersAsync(
            CronManifest manifest,
            string outputDir,
            CronGenerationOptions options,
            CancellationToken ct)
        {
            string templateSource = """
            // AUTO GENERATED - DO NOT EDIT
            // Code generated by PocketBase.Blazor.
            package {{ output_dir }}

            import (
            {%- for import in imports %}
                "{{ import }}"
            {%- endfor %}
            )

            func init() {
            {% for cron in crons %}
                RegisterHandler("{{ cron.id }}", func(payload map[string]any) {
            {{ cron.handler_body | indent: 8 }}
                })
            {% endfor %}
            }
            """;

            FluidParser parser = new FluidParser();

            if (!parser.TryParse(templateSource, out IFluidTemplate? template, out string? error))
                throw new InvalidOperationException($"Failed to parse template: {error}");

            HashSet<string> importedPackages = new HashSet<string> { "log" };
            List<ExpandoObject> processedCrons = new List<ExpandoObject>();

            foreach (CronDefinition cron in manifest.Crons)
            {
                string handlerBody = !string.IsNullOrWhiteSpace(cron.HandlerBody)
                    ? cron.HandlerBody
                    : $"log.Println(\"cron '{cron.Id}' executed\", payload)";

                if (cron.ImportPackages != null)
                {
                    foreach (string? pkg in cron.ImportPackages.Where(p => !string.IsNullOrWhiteSpace(p)))
                        importedPackages.Add(pkg.Trim());
                }

                dynamic expando = new ExpandoObject();
                expando.id = cron.Id;
                expando.handler_body = handlerBody;
                processedCrons.Add(expando);
            }

            TemplateContext context = new TemplateContext();
            context.SetValue("output_dir", options.OutputDirectory);
            context.SetValue("imports", importedPackages.OrderBy(p => p).ToList());
            context.SetValue("crons", processedCrons);

            context.Options.Filters.AddFilter("indent", (input, arguments, ctx) =>
            {
                if (input.ToStringValue() is string str)
                {
                    int indent = (int)(arguments.Count > 0 ? arguments.At(0).ToNumberValue() : 4);
                    string indentStr = new string(' ', indent);
                    string[] lines = str.Split('\n');
                    return StringValue.Create(string.Join("\n", lines.Select(line => indentStr + line)));
                }
                return input;
            });

            string result = await template.RenderAsync(context);

            string filePath = Path.Combine(outputDir, "handlers.go");
            await File.WriteAllTextAsync(filePath, result, ct);
        }

        /// <inheritdoc />
        public async Task BuildGoBinaryAsync(
            CronGenerationOptions options,
            CancellationToken ct)
        {
            // Initialize Go module
            ProcessStartInfo initPsi = new ProcessStartInfo
            {
                FileName = options.GoExecutable,
                Arguments = $"mod init {options.ModuleName}",
                WorkingDirectory = options.ProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process initProcess = Process.Start(initPsi)
                ?? throw new InvalidOperationException("Failed to start go mod init");

            string initStderr = await initProcess.StandardError.ReadToEndAsync(ct);
            await initProcess.WaitForExitAsync(ct);

            // Ignore error if module already exists (exit code 1)
            if (initProcess.ExitCode != 0 && initProcess.ExitCode != 1)
            {
                throw new InvalidOperationException(
                    $"go mod init failed:\n{initStderr}");
            }

            // Run go mod tidy
            ProcessStartInfo tidyPsi = new ProcessStartInfo
            {
                FileName = options.GoExecutable,
                Arguments = "mod tidy",
                WorkingDirectory = options.ProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process tidyProcess = Process.Start(tidyPsi)
                ?? throw new InvalidOperationException("Failed to start go mod tidy");

            string tidyStderr = await tidyProcess.StandardError.ReadToEndAsync(ct);
            await tidyProcess.WaitForExitAsync(ct);

            if (tidyProcess.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"go mod tidy failed:\n{tidyStderr}");
            }

            // Now build
            ProcessStartInfo buildPsi = new ProcessStartInfo
            {
                FileName = options.GoExecutable,
                Arguments = $"build -o {options.OutputBinary}",
                WorkingDirectory = options.ProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process buildProcess = Process.Start(buildPsi)
                ?? throw new InvalidOperationException("Failed to start go build");

            string stdout = await buildProcess.StandardOutput.ReadToEndAsync(ct);
            string stderr = await buildProcess.StandardError.ReadToEndAsync(ct);

            await buildProcess.WaitForExitAsync(ct);

            if (buildProcess.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Go build failed:\n{stderr}");
            }
        }
    }

}

/*
var builder = PocketBaseHostBuilder.CreateDefault();

await builder
    .UseLogger(logger)
    .UseCrons(
        cronGenerator,
        new CronManifest
        {
            Crons =
            [
                new()
                {
                    Id = "hello",
                    Description = "Hello cron",
                    HandlerBody = log.Println(\"cron hello executed\", payload)
                },
            ]
        },
        new CronGenerationOptions
        {
            ProjectDirectory = "cron-server",
            BuildBinary = true
        })
    .BuildAsync();

_host = await builder.BuildAsync();
await _host.StartAsync();
*/

