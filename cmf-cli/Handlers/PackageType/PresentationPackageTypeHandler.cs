﻿using Cmf.Common.Cli.Constants;
using Cmf.Common.Cli.Enums;
using Cmf.Common.Cli.Objects;
using Cmf.Common.Cli.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cmf.Common.Cli.Handlers
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Cmf.Common.Cli.Handlers.PackageTypeHandler" />
    public class PresentationPackageTypeHandler : PackageTypeHandler
    {
        #region Private Methods

        /// <summary>
        /// Generates the presentation configuration file.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        private void GeneratePresentationConfigFile(DirectoryInfo packageOutputDir)
        {
            Log.Debug("Generating Presentation config.json");
            string path = $"{packageOutputDir.FullName}/{CliConstants.CmfPackagePresentationConfig}";

            List<string> packageList = new();
            List<string> transformInjections = new();

            DirectoryInfo cmfPackageDirectory = CmfPackage.GetFileInfo().Directory;

            foreach (ContentToPack contentToPack in CmfPackage.ContentToPack)
            {
                if (contentToPack.Action == null || contentToPack.Action == PackAction.Pack)
                {
                    // TODO: Validate if contentToPack.Source exists before
                    DirectoryInfo[] packDirectories = cmfPackageDirectory.GetDirectories(contentToPack.Source);

                    foreach (DirectoryInfo packDirectory in packDirectories)
                    {
                        dynamic packageJson = packDirectory.GetPackageJsonFile();
                        if (packageJson != null)
                        {
                            string packageName = packageJson.name;

                            // For IoT Packages we should ignore the driver packages
                            if (!packageName.Contains(CliConstants.Driver, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                packageList.Add($"'{packageName}'");
                            }
                        }
                    }
                }
                else if (contentToPack.Action == PackAction.Transform)
                {
                    transformInjections.Add(contentToPack.Source);
                }
            }

            if (packageList.HasAny())
            {
                // Get Template
                string fileContent = GenericUtilities.GetEmbeddedResourceContent($"{CliConstants.FolderTemplates}/{CmfPackage.PackageType}/{CliConstants.CmfPackagePresentationConfig}");

                string packagesToRemove = string.Empty;
                List<string> packagesToAdd = new();

                for (int i = 0; i < packageList.Count; i++)
                {
                    if (CmfPackage.PackageType == PackageType.IoT)
                    {
                        packagesToRemove += $"@.path=={packageList[i]}";
                    }
                    else
                    {
                        packagesToRemove += $"@=={packageList[i]}";
                    }

                    if (packageList.Count > 1 &&
                        i != packageList.Count - 1)
                    {
                        packagesToRemove += " || ";
                    }

                    string packageToAdd = packageList[i].Replace("'", "\"");
                    if (CmfPackage.PackageType == PackageType.IoT)
                    {
                        packageToAdd = string.Format("{{\"path\": {0} }}", packageToAdd);
                    }

                    packagesToAdd.Add(packageToAdd);
                }

                fileContent = fileContent.Replace(CliConstants.TokenPackagesToRemove, packagesToRemove);
                fileContent = fileContent.Replace(CliConstants.TokenPackagesToAdd, string.Join(",", packagesToAdd));
                fileContent = fileContent.Replace(CliConstants.TokenVersion, CmfPackage.Version);

                string injection = string.Empty;
                if (transformInjections.HasAny())
                {
                    // we actually want a trailing comma here, because the inject token is in the middle of the document. If this changes we need to put more logic here.
                    var injections = transformInjections.Select(injection => File.ReadAllText(injection) + ",");
                    injection = string.Join(System.Environment.NewLine, injections);
                }
                fileContent = fileContent.Replace(CliConstants.TokenJDTInjection, injection);

                File.WriteAllText(path, fileContent);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the <see cref="PresentationPackageTypeHandler" /> class.
        /// </summary>
        /// <param name="cmfPackage">The CMF package.</param>
        public PresentationPackageTypeHandler(CmfPackage cmfPackage) : base(cmfPackage)
        {
            cmfPackage.SetDefaultValues
            (
                steps:
                    new List<Step>
                    {
                        new Step(StepType.DeployFiles)
                        {
                            ContentPath = "node_modules/**"
                        },

                        new Step(StepType.TransformFile)
                        {
                            File = "config.json",
                            TagFile = true
                        }
                    }

            );

            DefaultContentToIgnore.AddRange(new List<string>
            {
                "node_modules",
                "test",
                "*.ts",
                "node.exe",
                "CompileProject.ps1",
                "node_modules_cache.zip"
            });
        }

        /// <summary>
        /// Bumps the specified version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="buildNr">The version for build Nr.</param>
        /// <param name="bumpInformation">The bump information.</param>
        public override void Bump(string version, string buildNr, Dictionary<string, object> bumpInformation = null)
        {
            base.Bump(version, buildNr, bumpInformation);

            string parentDirectory = CmfPackage.GetFileInfo().DirectoryName;
            string[] filesToUpdate = Directory.GetFiles(parentDirectory, "package.json", SearchOption.AllDirectories);
            foreach (var fileName in filesToUpdate)
            {
                if (fileName.Contains("node_modules"))
                {
                    continue;
                }
                string json = File.ReadAllText(fileName);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                if (jsonObj["version"] == null)
                {
                    throw new CliException(string.Format(CliMessages.MissingMandatoryProperty, "version", fileName));
                }

                jsonObj["version"] = GenericUtilities.RetrieveNewPresentationVersion(jsonObj["version"].ToString(), version, buildNr);

                File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented));
            }

            filesToUpdate = Directory.GetFiles(parentDirectory, "*metadata.ts", SearchOption.AllDirectories);
            foreach (var fileName in filesToUpdate)
            {
                if (fileName.Contains("node_modules")
                    || fileName.Contains("\\src\\style")) // prevent metadata.ts in the \src\style from being taken into account
                {
                    continue;
                }
                string metadataFile = File.ReadAllText(fileName);
                string regex = @"version: \""[0-9.-]*\""";
                var metadataVersion = Regex.Match(metadataFile, regex, RegexOptions.Singleline)?.Value?.Split("\"")[1];
                metadataVersion = GenericUtilities.RetrieveNewPresentationVersion(metadataVersion, version, buildNr);
                metadataFile = Regex.Replace(metadataFile, regex, string.Format("version: \"{0}\"", metadataVersion));
                File.WriteAllText(fileName, metadataFile);
            }
        }

        /// <summary>
        /// Packs the specified package output dir.
        /// </summary>
        /// <param name="packageOutputDir">The package output dir.</param>
        /// <param name="outputDir">The output dir.</param>
        public override void Pack(DirectoryInfo packageOutputDir, DirectoryInfo outputDir)
        {
            GeneratePresentationConfigFile(packageOutputDir);

            base.Pack(packageOutputDir, outputDir);
        }

        #endregion
    }
}